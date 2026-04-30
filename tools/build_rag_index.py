"""
build_rag_index.py
==================

离线构建 Satone RAG 索引：读取 heroine_all_dialogues.json -> 清洗 + 分类 + 调用
Ollama embedding -> 输出紧凑二进制索引 satone_rag_index.bin。

游戏运行时由 RAGClient.cs 加载该文件并完成检索。

二进制格式（小端序，所有字符串 UTF-8）：
    Magic        : 4 bytes "SRAG"
    Version      : int32 = 1
    VecDim       : int32 (例如 1024)
    Count        : int32
    EmbedModel   : int32 length + UTF-8 bytes
    每条记录:
        Category : byte (见 CATEGORY_MAP)
        IdLen    : int32 + UTF-8 ScenarioGroupID
        JaLen    : int32 + UTF-8 Ja
        ZhLen    : int32 + UTF-8 ZhHans
        Vec      : VecDim * float32

使用：
    python tools/build_rag_index.py
依赖：
    - Ollama 在 http://127.0.0.1:11434 运行
    - 已 pull 嵌入模型，默认 bge-m3
"""

from __future__ import annotations

import argparse
import json
import os
import struct
import sys
import time
from pathlib import Path
from typing import Iterable
from urllib import request as urlrequest
from urllib.error import HTTPError, URLError

ROOT = Path(__file__).resolve().parent.parent
DEFAULT_INPUT = ROOT / "game-decompiled" / "heroine_all_dialogues.json"
DEFAULT_OUTPUT = (
    Path(r"C:\Program Files (x86)\Steam\steamapps\common\Chill with You Lo-Fi Story")
    / "BepInEx" / "plugins" / "AIChat" / "satone_rag_index.bin"
)
DEFAULT_OLLAMA = "http://127.0.0.1:11434"
DEFAULT_MODEL = "bge-m3"

# 分类编码必须和 C# RAGClient.cs 中的枚举一致
CATEGORY_MAP = {
    "smalltalk": 0,
    "selftalk": 1,
    "clickheroine": 2,
    "pomodoro": 3,
    "motion": 4,
    "main_general": 5,
    "tutorial": 6,
    "later_extra": 7,
    "other": 8,
}

# 默认排除：剧情高潮章节（避免日常对话被剧透/重要剧情对白污染）
DEFAULT_MAIN_EXCLUDE_FROM = 28  # main_28 及之后默认不进入索引

# 可选：联动 / 特殊事件多为支线，对日常对话无锚点意义
EXCLUDE_GROUP_PREFIXES = (
    "special_",
    "event_",
    "gamedemo_",
)


def categorize(scenario_group_id: str) -> str | None:
    """根据 ScenarioGroupID 决定分类，返回 None 表示丢弃。"""
    sid = scenario_group_id.lower()

    # spoiler / 不相关支线
    if sid.startswith(EXCLUDE_GROUP_PREFIXES):
        return None

    if sid.startswith("main_"):
        # 抓出主线章节号
        try:
            ep = int(sid.split("_")[1])
        except (IndexError, ValueError):
            return None
        if ep >= DEFAULT_MAIN_EXCLUDE_FROM:
            return None
        return "main_general"

    if sid.startswith("smalltalk"):
        return "smalltalk"
    if sid.startswith("selftalk"):
        return "selftalk"
    if sid.startswith("clickheroine"):
        return "clickheroine"
    if sid.startswith("pomodoro"):
        return "pomodoro"
    if sid.startswith("motion"):
        return "motion"
    if sid.startswith("tutorial"):
        return "tutorial"
    if sid.startswith(("later_", "extra_")):
        return "later_extra"

    return "other"


def is_meaningful(text: str) -> bool:
    """过滤太短或纯标点的句子。"""
    if not text:
        return False
    stripped = text.strip()
    if len(stripped) < 3:
        return False
    # 全是省略号/标点的句子（"……" / "...!?" 等）丢弃
    punct_only = all(ch in "…．。、，,.!?！？・ーぁぃぅぇぉっゃゅょ " for ch in stripped)
    if punct_only:
        return False
    return True


def load_dialogues(path: Path) -> list[dict]:
    with path.open("r", encoding="utf-8") as f:
        return json.load(f)


def clean_dialogues(raw: Iterable[dict]) -> list[dict]:
    """清洗 + 分类 + 去重。"""
    seen_keys: set[tuple[str, str]] = set()
    cleaned: list[dict] = []
    for entry in raw:
        ja = (entry.get("Ja") or "").strip()
        zh = (entry.get("ZhHans") or "").strip()
        sid = entry.get("ScenarioGroupID", "")
        if not is_meaningful(ja):
            continue
        category = categorize(sid)
        if category is None:
            continue
        # 用 (category, ja) 去重，避免不同 group 引用同一条短语
        key = (category, ja)
        if key in seen_keys:
            continue
        seen_keys.add(key)

        cleaned.append({
            "id": sid,
            "category": category,
            "ja": ja.replace("\n", " ").strip(),
            "zh": zh.replace("\n", " ").strip(),
        })
    return cleaned


def call_ollama_embeddings(host: str, model: str, prompt: str, timeout: float = 30.0) -> list[float]:
    body = json.dumps({"model": model, "prompt": prompt}).encode("utf-8")
    req = urlrequest.Request(
        f"{host.rstrip('/')}/api/embeddings",
        data=body,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urlrequest.urlopen(req, timeout=timeout) as resp:
        payload = json.loads(resp.read().decode("utf-8"))
    vec = payload.get("embedding") or payload.get("data") or []
    if not vec:
        raise RuntimeError(f"empty embedding for: {prompt!r}; payload={payload}")
    return vec


def warmup_ollama(host: str, model: str) -> int:
    """探活 + 拿到向量维度。"""
    print(f"[warmup] 探测 Ollama @ {host} 模型 {model} ...", flush=True)
    try:
        vec = call_ollama_embeddings(host, model, "テスト", timeout=120.0)
    except (HTTPError, URLError) as e:
        print(f"[ERROR] Ollama 不可用：{e}\n请确认 ollama 服务在运行，并 `ollama pull {model}`", file=sys.stderr)
        sys.exit(2)
    except RuntimeError as e:
        print(f"[ERROR] 嵌入模型返回异常：{e}", file=sys.stderr)
        sys.exit(2)
    print(f"[warmup] OK，向量维度 = {len(vec)}", flush=True)
    return len(vec)


def encode_record(buf: bytearray, rec: dict, vec: list[float], dim: int) -> None:
    cat = CATEGORY_MAP[rec["category"]]
    id_bytes = rec["id"].encode("utf-8")
    ja_bytes = rec["ja"].encode("utf-8")
    zh_bytes = rec["zh"].encode("utf-8")

    buf.append(cat)
    buf += struct.pack("<i", len(id_bytes)); buf += id_bytes
    buf += struct.pack("<i", len(ja_bytes)); buf += ja_bytes
    buf += struct.pack("<i", len(zh_bytes)); buf += zh_bytes
    if len(vec) != dim:
        raise RuntimeError(f"向量维度不一致: 期望 {dim}, 实际 {len(vec)}")
    buf += struct.pack(f"<{dim}f", *vec)


def write_index(out_path: Path, records: list[dict], vectors: list[list[float]], dim: int, model: str) -> None:
    out_path.parent.mkdir(parents=True, exist_ok=True)
    buf = bytearray()
    buf += b"SRAG"
    buf += struct.pack("<i", 1)
    buf += struct.pack("<i", dim)
    buf += struct.pack("<i", len(records))

    model_bytes = model.encode("utf-8")
    buf += struct.pack("<i", len(model_bytes)); buf += model_bytes

    for rec, vec in zip(records, vectors):
        encode_record(buf, rec, vec, dim)

    with out_path.open("wb") as f:
        f.write(buf)


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description="Build Satone RAG index")
    p.add_argument("--input", default=str(DEFAULT_INPUT), help="heroine_all_dialogues.json 路径")
    p.add_argument("--output", default=str(DEFAULT_OUTPUT), help="输出索引文件路径")
    p.add_argument("--ollama", default=DEFAULT_OLLAMA, help="Ollama base URL")
    p.add_argument("--model", default=DEFAULT_MODEL, help="嵌入模型名称")
    p.add_argument("--main-exclude-from", type=int, default=DEFAULT_MAIN_EXCLUDE_FROM,
                   help="主线第几章及之后整体排除（默认 28）")
    p.add_argument("--limit", type=int, default=0, help="只处理前 N 条（调试用，0 表示全部）")
    return p.parse_args()


def main() -> None:
    args = parse_args()

    global DEFAULT_MAIN_EXCLUDE_FROM
    DEFAULT_MAIN_EXCLUDE_FROM = args.main_exclude_from

    input_path = Path(args.input)
    output_path = Path(args.output)

    if not input_path.exists():
        print(f"[ERROR] 找不到输入文件：{input_path}", file=sys.stderr)
        sys.exit(1)

    raw = load_dialogues(input_path)
    print(f"[load ] 原始条目: {len(raw)}", flush=True)

    cleaned = clean_dialogues(raw)
    print(f"[clean] 清洗后条目: {len(cleaned)}", flush=True)

    if args.limit > 0:
        cleaned = cleaned[: args.limit]
        print(f"[limit] 只处理前 {len(cleaned)} 条 (调试模式)", flush=True)

    # 类别统计
    cat_count: dict[str, int] = {}
    for r in cleaned:
        cat_count[r["category"]] = cat_count.get(r["category"], 0) + 1
    print("[stats] 分类分布:")
    for k, v in sorted(cat_count.items(), key=lambda kv: -kv[1]):
        print(f"        {k:14s} {v}")

    dim = warmup_ollama(args.ollama, args.model)

    vectors: list[list[float]] = []
    start = time.time()
    last_log = start
    for i, rec in enumerate(cleaned):
        try:
            vec = call_ollama_embeddings(args.ollama, args.model, rec["ja"])
        except Exception as e:
            print(f"[ERROR] 第 {i} 条嵌入失败 (id={rec['id']}): {e}", file=sys.stderr)
            sys.exit(3)
        vectors.append(vec)

        now = time.time()
        if now - last_log > 2.0 or i == len(cleaned) - 1:
            done = i + 1
            rate = done / max(now - start, 1e-3)
            eta = (len(cleaned) - done) / max(rate, 1e-3)
            print(f"[embed] {done}/{len(cleaned)}  {rate:.1f} it/s  ETA {eta:.1f}s", flush=True)
            last_log = now

    write_index(output_path, cleaned, vectors, dim, args.model)
    size_kb = output_path.stat().st_size / 1024
    print(f"[done ] 已写入 {output_path}  ({size_kb:.1f} KB, {len(cleaned)} 条, dim={dim})")


if __name__ == "__main__":
    main()

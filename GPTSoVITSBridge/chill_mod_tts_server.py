# -*- coding: utf-8 -*-
"""
Chill AIMod ⇄ GPT-SoVITS 推理桥接（阶段 1：manifest 驱动 + 分句 + 多段音频拼接）。

引擎：
  - inference_webui（默认，支持 v3 LoRA 微调权重）
  - tts_infer_pack（预留；阶段 2 v2ProPlus 启用，需新版 GPT-SoVITS）

Mod 契约：POST /tts JSON 与 GET /health 不变。
"""

from __future__ import annotations

import argparse
import io
import json
import os
import sys
import traceback
from typing import Any, Optional

BRIDGE_VERSION = "0.2.0-phase1"

# api_v2 / TTS_infer_pack 的 cut 名 → inference_webui 中文键（再由 i18n 转当前语言标签）
_CUT_TO_WEBUI_ZH: dict[str, str] = {
    "none": "不切",
    "cut0": "不切",
    "不切": "不切",
    "cut1": "凑四句一切",
    "凑四句一切": "凑四句一切",
    "cut2": "凑50字一切",
    "凑50字一切": "凑50字一切",
    "cut3": "按中文句号。切",
    "按中文句号。切": "按中文句号。切",
    "cut4": "按英文句号.切",
    "按英文句号.切": "按英文句号.切",
    "cut5": "按标点符号切",
    "按标点符号切": "按标点符号切",
}


def _parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description="Chill AIMod GPT-SoVITS TTS bridge")
    p.add_argument("--gptsovits-home", required=True, help="GPT-SoVITS 便携包根目录")
    p.add_argument("--gpt", required=True, help="GPT 权重 .ckpt（相对 home 或绝对路径）")
    p.add_argument("--sovits", required=True, help="SoVITS 权重 .pth")
    p.add_argument("--host", default="127.0.0.1")
    p.add_argument("--port", type=int, default=9880)
    p.add_argument("--language", default="zh_CN", help="GPT-SoVITS i18n")
    p.add_argument("--preload", action="store_true", help="启动时预加载模型")
    p.add_argument(
        "--manifest",
        default="",
        help="manifest.json 路径（默认：<home>/manifest.json）",
    )
    return p.parse_args()


args = _parse_args()
_HOME = os.path.abspath(args.gptsovits_home)
_MANIFEST_PATH = (
    os.path.abspath(args.manifest)
    if args.manifest.strip()
    else os.path.join(_HOME, "manifest.json")
)


def _load_manifest() -> dict[str, Any]:
    defaults: dict[str, Any] = {
        "engineVersion": BRIDGE_VERSION,
        "engine": "inference_webui",
        "modelVersion": "v3",
        "textSplitMethod": "cut1",
        "parallelInfer": True,
        "preload": True,
    }
    if not os.path.isfile(_MANIFEST_PATH):
        return defaults
    try:
        with open(_MANIFEST_PATH, "r", encoding="utf-8") as f:
            data = json.load(f)
        if isinstance(data, dict):
            defaults.update(data)
    except Exception as e:  # noqa: BLE001
        print(f"[ChillBridge] WARN: manifest load failed: {e}")
    return defaults


MANIFEST = _load_manifest()


def _abs_under_home(p: str) -> str:
    p = p.strip('"').strip()
    return p if os.path.isabs(p) else os.path.normpath(os.path.join(_HOME, p))


# CLI 权重优先；manifest 作回退
_gpt_cli = _abs_under_home(args.gpt)
_sov_cli = _abs_under_home(args.sovits)
_gpt_mf = _abs_under_home(str(MANIFEST.get("gptWeights", ""))) if MANIFEST.get("gptWeights") else ""
_sov_mf = _abs_under_home(str(MANIFEST.get("sovitsWeights", ""))) if MANIFEST.get("sovitsWeights") else ""

os.environ.setdefault("language", args.language)
_model_ver = str(MANIFEST.get("modelVersion", "v3")).lower()
os.environ.setdefault("version", _model_ver)
os.environ["gpt_path"] = _gpt_cli if os.path.isfile(_gpt_cli) else (_gpt_mf or _gpt_cli)
os.environ["sovits_path"] = _sov_cli if os.path.isfile(_sov_cli) else (_sov_mf or _sov_cli)

_iv_state: dict[str, Any] = {"module": None, "error": None}
_tts_pack_state: dict[str, Any] = {"pipeline": None, "error": None}


def ensure_inference_webui():
    if _iv_state["module"] is not None:
        return _iv_state["module"]
    if _iv_state["error"] is not None:
        raise RuntimeError(str(_iv_state["error"]))

    os.makedirs(_HOME, exist_ok=True)
    os.chdir(_HOME)
    for item in reversed([_HOME, os.path.join(_HOME, "GPT_SoVITS")]):
        if item not in sys.path:
            sys.path.insert(0, item)

    os.environ.setdefault("language", args.language)
    os.environ.setdefault("version", _model_ver)
    os.environ["gpt_path"] = os.environ.get("gpt_path", _gpt_cli)
    os.environ["sovits_path"] = os.environ.get("sovits_path", _sov_cli)
    os.environ.setdefault("infer_ttswebui", os.environ.get("infer_ttswebui", "9872"))

    try:
        import inference_webui as iw  # pylint: disable=import-outside-toplevel

        _iv_state["module"] = iw
        return iw
    except Exception:  # noqa: BLE001
        _iv_state["error"] = traceback.format_exc()
        raise


def resolve_how_to_cut(iw_mod, raw: Optional[str]) -> str:
    """将 manifest / Mod 的 cut1 等映射为 inference_webui 下拉项标签。"""
    key = (raw or MANIFEST.get("textSplitMethod") or "cut1").strip()
    zh_key = _CUT_TO_WEBUI_ZH.get(key, _CUT_TO_WEBUI_ZH.get(key.lower(), "凑四句一切"))
    return iw_mod.i18n(zh_key)


def coerce_lang_label(iw_mod, raw: str) -> str:
    if not raw:
        raw = "ja"
    s = raw.strip()
    if s in iw_mod.dict_language:
        return s
    code_map = {
        "ja": "all_ja",
        "jp": "all_ja",
        "zh": "all_zh",
        "cn": "all_zh",
        "en": "en",
        "ko": "all_ko",
        "yue": "all_yue",
    }
    inner = code_map.get(s.lower())
    if inner is None:
        raise ValueError(f"unsupported text_lang/prompt_lang: {raw}")
    for label, val in iw_mod.dict_language.items():
        if val == inner:
            return label
    raise ValueError(f"cannot map lang code to UI label: {raw}")


def _concat_generator_output(gen) -> tuple[int, Any]:
    """拼接 get_tts_wav 多次 yield 的 PCM，修复「只保留最后一段」导致的断音/缺段。"""
    import numpy as np  # pylint: disable=import-outside-toplevel

    last_sr: Optional[int] = None
    parts: list[Any] = []
    for item in gen:
        last_sr, pcm = item
        parts.append(np.asarray(pcm).astype(np.int16).reshape(-1))

    if last_sr is None or not parts:
        raise RuntimeError("generator produced no audio")

    if len(parts) == 1:
        merged = parts[0]
    else:
        merged = np.concatenate(parts)
        print(f"[ChillBridge] concatenated {len(parts)} audio segment(s), samples={merged.size}")

    return last_sr, merged


def run_synthesis_webui(payload: dict) -> tuple[int, bytes]:
    iw_mod = ensure_inference_webui()

    ref_path = os.path.abspath(payload["ref_audio_path"].strip('"').strip())
    if not os.path.isfile(ref_path):
        raise FileNotFoundError(ref_path)

    text = payload.get("text") or ""
    prompt_text = (payload.get("prompt_text") or "").strip()
    if not text.strip():
        raise ValueError("text is empty")
    if not prompt_text:
        raise ValueError("v3 requires prompt_text (non-empty reference transcript)")

    prompt_lang_label = coerce_lang_label(iw_mod, payload.get("prompt_lang") or "ja")
    text_lang_label = coerce_lang_label(iw_mod, payload.get("text_lang") or "ja")
    how_to_cut = resolve_how_to_cut(iw_mod, payload.get("text_split_method"))

    top_k = int(payload.get("top_k", 16))
    top_p = float(payload.get("top_p", 1.0))
    temperature = float(payload.get("temperature", 1.0))
    speed = float(payload.get("speed", 1.0))
    sample_steps = int(payload.get("sample_steps", 16))
    if_sr = bool(payload.get("if_sr", False))
    pause_second = float(payload.get("pause_second", 0.3))

    print(
        f"[ChillBridge] synthesize engine=webui cut={how_to_cut!r} "
        f"chars={len(text)} sample_steps={sample_steps} if_sr={if_sr}"
    )

    gen = iw_mod.get_tts_wav(
        ref_wav_path=ref_path,
        prompt_text=prompt_text,
        prompt_language=prompt_lang_label,
        text=text,
        text_language=text_lang_label,
        how_to_cut=how_to_cut,
        top_k=top_k,
        top_p=top_p,
        temperature=temperature,
        ref_free=False,
        speed=speed,
        sample_steps=sample_steps,
        if_sr=if_sr,
        pause_second=pause_second,
        if_freeze=False,
        inp_refs=None,
    )

    last_sr, pcm = _concat_generator_output(gen)

    import soundfile as sf  # pylint: disable=import-outside-toplevel

    buf = io.BytesIO()
    sf.write(buf, pcm, last_sr, format="WAV", subtype="PCM_16")
    return last_sr, buf.getvalue()


def _write_tts_infer_custom_yaml() -> str:
    """为阶段 2 预写 custom 配置（当前引擎仍为 webui 时不加载）。"""
    cfg_path = os.path.join(_HOME, "GPT_SoVITS", "configs", "chill_tts_infer.yaml")
    os.makedirs(os.path.dirname(cfg_path), exist_ok=True)
    custom = {
        "device": "cuda",
        "is_half": True,
        "version": MANIFEST.get("modelVersion", "v3"),
        "t2s_weights_path": os.environ.get("gpt_path", ""),
        "vits_weights_path": os.environ.get("sovits_path", ""),
        "bert_base_path": os.path.join(
            _HOME, "GPT_SoVITS/pretrained_models/chinese-roberta-wwm-ext-large"
        ),
        "cnhuhbert_base_path": os.path.join(
            _HOME, "GPT_SoVITS/pretrained_models/chinese-hubert-base"
        ),
    }
    doc = {"custom": custom}
    try:
        import yaml  # pylint: disable=import-outside-toplevel

        with open(cfg_path, "w", encoding="utf-8") as f:
            yaml.dump(doc, f, allow_unicode=True, default_flow_style=False)
    except Exception as e:  # noqa: BLE001
        print(f"[ChillBridge] WARN: could not write {cfg_path}: {e}")
    return cfg_path


def run_synthesis_tts_pack(payload: dict) -> tuple[int, bytes]:
    """TTS_infer_pack 路径（需新版 GPT-SoVITS 且 version 为 v1/v2/v2Pro 等）。"""
    if _tts_pack_state["pipeline"] is None and _tts_pack_state["error"] is not None:
        raise RuntimeError(str(_tts_pack_state["error"]))

    os.makedirs(_HOME, exist_ok=True)
    os.chdir(_HOME)
    for item in reversed([_HOME, os.path.join(_HOME, "GPT_SoVITS")]):
        if item not in sys.path:
            sys.path.insert(0, item)

    yaml_path = _write_tts_infer_custom_yaml()

    try:
        from GPT_SoVITS.TTS_infer_pack.TTS import TTS, TTS_Config  # pylint: disable=import-outside-toplevel

        if _tts_pack_state["pipeline"] is None:
            cfg = TTS_Config(yaml_path)
            ver = str(cfg.version).lower()
            if ver not in ("v1", "v2"):
                raise RuntimeError(
                    f"TTS_infer_pack in this bundle only supports v1/v2 (got {ver!r}). "
                    "Keep manifest engine=inference_webui for v3, or upgrade GPT-SoVITS in phase 2."
                )
            _tts_pack_state["pipeline"] = TTS(cfg)
            _tts_pack_state["pipeline"].init_t2s_weights(os.environ["gpt_path"])
            _tts_pack_state["pipeline"].init_vits_weights(os.environ["sovits_path"])
            print("[ChillBridge] TTS_infer_pack pipeline ready.")

        pipe = _tts_pack_state["pipeline"]
        req = {
            "text": payload.get("text") or "",
            "text_lang": (payload.get("text_lang") or "ja").lower(),
            "ref_audio_path": os.path.abspath(payload["ref_audio_path"].strip('"').strip()),
            "prompt_text": payload.get("prompt_text") or "",
            "prompt_lang": (payload.get("prompt_lang") or "ja").lower(),
            "text_split_method": payload.get("text_split_method")
            or MANIFEST.get("textSplitMethod")
            or "cut1",
            "batch_size": int(payload.get("batch_size", 4)),
            "parallel_infer": bool(
                payload.get("parallel_infer", MANIFEST.get("parallelInfer", True))
            ),
            "speed_factor": float(payload.get("speed", 1.0)),
            "fragment_interval": float(payload.get("pause_second", 0.3)),
            "streaming_mode": False,
        }
        if payload.get("top_k") is not None:
            req["top_k"] = int(payload["top_k"])
        if payload.get("top_p") is not None:
            req["top_p"] = float(payload["top_p"])
        if payload.get("temperature") is not None:
            req["temperature"] = float(payload["temperature"])

        sr, audio = next(pipe.run(req))
        import numpy as np  # pylint: disable=import-outside-toplevel
        import soundfile as sf  # pylint: disable=import-outside-toplevel

        buf = io.BytesIO()
        arr = np.asarray(audio)
        if arr.dtype != np.int16:
            peak = np.abs(arr).max()
            if peak > 1.0:
                arr = arr / peak
            arr = (arr * 32767.0).astype(np.int16)
        sf.write(buf, arr, sr, format="WAV", subtype="PCM_16")
        return sr, buf.getvalue()
    except Exception:  # noqa: BLE001
        _tts_pack_state["error"] = traceback.format_exc()
        raise


def run_synthesis(payload: dict) -> tuple[int, bytes]:
    engine = str(
        payload.get("engine") or MANIFEST.get("engine") or "inference_webui"
    ).lower()
    if engine in ("tts_infer_pack", "tts_pack", "api_v2"):
        return run_synthesis_tts_pack(payload)
    return run_synthesis_webui(payload)


def build_app():
    try:
        from fastapi import FastAPI  # pylint: disable=import-outside-toplevel
        from fastapi.responses import JSONResponse, Response  # pylint: disable=import-outside-toplevel
        from pydantic import BaseModel, Field  # pylint: disable=import-outside-toplevel
    except ImportError as e:
        raise SystemExit(f"Requires fastapi+pydantic+uvicorn: {e}") from e

    class TtsBody(BaseModel):
        text: str
        text_lang: str = Field("ja")
        ref_audio_path: str
        prompt_text: str = ""
        prompt_lang: str = Field("ja")
        text_split_method: Optional[str] = None
        top_k: Optional[int] = None
        top_p: Optional[float] = None
        temperature: Optional[float] = None
        speed: Optional[float] = None
        sample_steps: Optional[int] = None
        if_sr: Optional[bool] = None
        pause_second: Optional[float] = None
        parallel_infer: Optional[bool] = None
        batch_size: Optional[int] = None
        engine: Optional[str] = None

    app = FastAPI(title="Chill AIMod TTS", version=BRIDGE_VERSION)

    @app.get("/health")
    async def health():
        engine = str(MANIFEST.get("engine", "inference_webui"))
        loaded = False
        if engine in ("tts_infer_pack", "tts_pack", "api_v2"):
            loaded = _tts_pack_state["pipeline"] is not None
        else:
            loaded = _iv_state["module"] is not None

        err = _iv_state.get("error") or _tts_pack_state.get("error")
        return {
            "ok": True,
            "bridgeVersion": BRIDGE_VERSION,
            "engine": engine,
            "modelVersion": MANIFEST.get("modelVersion", "v3"),
            "models_loaded": loaded,
            "home": _HOME,
            "gpt": os.environ.get("gpt_path"),
            "sovits": os.environ.get("sovits_path"),
            "textSplitMethod": MANIFEST.get("textSplitMethod", "cut1"),
            "manifest": _MANIFEST_PATH,
            "load_error_preview": (err or "")[:400] if err else None,
        }

    @app.post("/tts")
    async def tts_endpoint(body: TtsBody):
        try:
            try:
                d = body.model_dump(exclude_none=True)
            except AttributeError:
                d = body.dict(exclude_none=True)
            sr, wav_bytes = run_synthesis(d)
            return Response(
                content=wav_bytes,
                media_type="audio/wav",
                headers={"X-Sample-Rate": str(sr), "X-Bridge-Version": BRIDGE_VERSION},
            )
        except FileNotFoundError as e:
            return JSONResponse(
                {"message": str(e), "code": "ref_audio_not_found"},
                status_code=400,
            )
        except ValueError as e:
            return JSONResponse({"message": str(e), "code": "validation"}, status_code=400)
        except Exception as e:  # noqa: BLE001
            tb = traceback.format_exc()
            return JSONResponse(
                {"message": str(e), "code": "internal", "trace": tb[-4000:]},
                status_code=500,
            )

    return app


def main():
    os.environ["_CHILL_BRIDGE_HOME"] = _HOME
    engine = str(MANIFEST.get("engine", "inference_webui"))

    print(f"[ChillBridge] version={BRIDGE_VERSION}")
    print(f"[ChillBridge] GPT-SoVITS home: {_HOME}")
    print(f"[ChillBridge] manifest: {_MANIFEST_PATH}")
    print(f"[ChillBridge] engine={engine} modelVersion={MANIFEST.get('modelVersion', 'v3')}")
    print(f"[ChillBridge] textSplitMethod={MANIFEST.get('textSplitMethod', 'cut1')}")
    print(f"[ChillBridge] gpt={os.environ.get('gpt_path')}")
    print(f"[ChillBridge] sovits={os.environ.get('sovits_path')}")

    _write_tts_infer_custom_yaml()

    app = build_app()

    if args.preload or MANIFEST.get("preload", True):
        print("[ChillBridge] Preloading models...")
        if engine in ("tts_infer_pack", "tts_pack", "api_v2"):
            try:
                run_synthesis_tts_pack(
                    {
                        "text": "テスト",
                        "text_lang": "ja",
                        "ref_audio_path": os.environ.get("sovits_path", ""),
                        "prompt_text": "テスト",
                        "prompt_lang": "ja",
                    }
                )
            except Exception as e:  # noqa: BLE001
                print(f"[ChillBridge] WARN: tts_infer_pack preload skipped: {e}")
        else:
            ensure_inference_webui()
            print("[ChillBridge] inference_webui loaded.")

    import uvicorn  # pylint: disable=import-outside-toplevel

    uvicorn.run(app, host=args.host, port=args.port, log_level="info")


if __name__ == "__main__":
    main()

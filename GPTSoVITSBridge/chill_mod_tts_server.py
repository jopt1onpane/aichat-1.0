# -*- coding: utf-8 -*-
"""
Chill AIMod ⇄ GPT-SoVITS v3 推理桥接（FastAPI，路线 B）。
部署：将本脚本复制到 GPT-SoVITS 便携包根目录（与 webui.py、GPT_SoVITS/ 同级），
      使用该包内的 runtime\\python.exe 启动。

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
阶段 1 自检（未完成后续 bat/默认 cfg 也可用）
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
1. 便携包目录下执行：
   runtime\\python.exe chill_mod_tts_server.py ^
     --gptsovits-home . ^
     --gpt GPT_weights_v3\\你的.ckpt ^
     --sovits SoVITS_weights_v3\\你的.pth ^
     --preload
   （首次加载模型较慢，可加 --preload 在服务启动时完成导入）
2. 另开终端（需 curl；PowerShell 可用 curl.exe）：
   curl.exe -X POST "http://127.0.0.1:9880/tts" ^
     -H "Content-Type: application/json" ^
     -d "{\\"text\\":\\"おはよう\\",\\"text_lang\\":\\"ja\\",\\"ref_audio_path\\":\\"绝对路径\\\\ref.wav\\",\\"prompt_text\\":\\"（与 ref 对齐的日文）\\",\\"prompt_lang\\":\\"ja\\"}" ^
     --output out.wav
3. 用播放器打开 out.wav；若 400 JSON，读 message 字段排错。
4. /health GET 在无 --preload 时 models_loaded=false 仍为 200（仅进程就绪）。
"""

import argparse
import io
import os
import sys
import traceback
from typing import Any, Optional


def _parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description="Chill AIMod GPT-SoVITS v3 TTS bridge")
    p.add_argument(
        "--gptsovits-home",
        required=True,
        help="GPT-SoVITS 便携包根目录（含 GPT_SoVITS 子文件夹）",
    )
    p.add_argument("--gpt", required=True, help="GPT 权重 .ckpt 路径（可为相对或绝对）")
    p.add_argument("--sovits", required=True, help="SoVITS 权重 .pth 路径（可为相对或绝对）")
    p.add_argument("--host", default="127.0.0.1", help="绑定地址（默认仅供本机）")
    p.add_argument("--port", type=int, default=9880, help="监听端口（与 AIMod.cfg 默认一致）")
    p.add_argument(
        "--language",
        default="zh_CN",
        help="GPT-SoVITS i18n（影响 dict_language UI 标签，一般用 zh_CN）",
    )
    p.add_argument(
        "--preload",
        action="store_true",
        help="启动即加载 inference_webui（否则首次 /tts 才加载）",
    )
    p.add_argument(
        "--reload-weights-timeout",
        type=float,
        default=0,
        help="预留：暂不实现周期性重载（0 表示不使用）",
    )
    return p.parse_args()


args = _parse_args()
_HOME = os.path.abspath(args.gptsovits_home)
os.environ.setdefault("language", args.language)
os.environ.setdefault("version", "v3")


def _abs_under_home(p: str) -> str:
    p = p.strip('"').strip()
    return p if os.path.isabs(p) else os.path.normpath(os.path.join(_HOME, p))


os.environ["gpt_path"] = _abs_under_home(args.gpt)
os.environ["sovits_path"] = _abs_under_home(args.sovits)


_iv_state: dict[str, Any] = {"module": None, "error": None}


def ensure_inference_webui():
    """切换工作目录并导入 inference_webui（仅首次）。工作目录须与官方 WebUI 一致。"""
    if _iv_state["module"] is not None:
        return _iv_state["module"]
    if _iv_state["error"] is not None:
        raise RuntimeError(str(_iv_state["error"]))
    os.makedirs(_HOME, exist_ok=True)
    os.chdir(_HOME)

    sp_l = [_HOME, os.path.join(_HOME, "GPT_SoVITS")]
    for item in reversed(sp_l):
        if item not in sys.path:
            sys.path.insert(0, item)

    os.environ.setdefault("language", args.language)
    os.environ.setdefault("version", "v3")
    os.environ["gpt_path"] = _abs_under_home(args.gpt)
    os.environ["sovits_path"] = _abs_under_home(args.sovits)
    os.environ.setdefault("infer_ttswebui", os.environ.get("infer_ttswebui", "9872"))

    try:
        import inference_webui as iw  # pylint: disable=import-outside-toplevel

        _iv_state["module"] = iw
        return iw
    except Exception as e:  # noqa: BLE001
        _iv_state["error"] = traceback.format_exc()
        raise


def coerce_lang_label(iw_mod, raw: str) -> str:
    """TTSClient 传 ja/zh/en；推理 UI 要用 dict_language 的键。"""
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


def run_synthesis(payload: dict) -> tuple[int, bytes]:
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

    cut_no = iw_mod.i18n("不切")

    top_k = int(payload.get("top_k", 16))
    top_p = float(payload.get("top_p", 1.0))
    temperature = float(payload.get("temperature", 1.0))
    speed = float(payload.get("speed", 1.0))
    sample_steps = int(payload.get("sample_steps", 16))
    if_sr = bool(payload.get("if_sr", False))
    pause_second = float(payload.get("pause_second", 0.3))

    gen = iw_mod.get_tts_wav(
        ref_wav_path=ref_path,
        prompt_text=prompt_text,
        prompt_language=prompt_lang_label,
        text=text,
        text_language=text_lang_label,
        how_to_cut=cut_no,
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

    last_sr: Optional[int] = None
    last_pcm: Optional[Any] = None
    for item in gen:
        last_sr, last_pcm = item
    if last_sr is None or last_pcm is None:
        raise RuntimeError("generator produced no audio")

    import numpy as np  # pylint: disable=import-outside-toplevel
    import soundfile as sf  # pylint: disable=import-outside-toplevel

    buf = io.BytesIO()
    wav_arr = np.asarray(last_pcm).astype(np.int16)
    sf.write(buf, wav_arr, last_sr, format="WAV", subtype="PCM_16")
    return last_sr, buf.getvalue()


def build_app():  # noqa: C901
    # 延迟导入，便于无 torch 环境下仍能通过 --help
    try:
        from fastapi import FastAPI  # pylint: disable=import-outside-toplevel
        from fastapi.responses import JSONResponse, Response  # pylint: disable=import-outside-toplevel
        from pydantic import BaseModel, Field  # pylint: disable=import-outside-toplevel
    except ImportError as e:
        raise SystemExit(f"Requires fastapi+pydantic+uvicorn in interpreter: {e}") from e

    class TtsBody(BaseModel):
        """与 AIChat.Services.TTSClient.Post JSON 对齐；扩展字段可为 Mod 可选升级用。"""

        text: str
        text_lang: str = Field("ja")
        ref_audio_path: str
        prompt_text: str = ""
        prompt_lang: str = Field("ja")
        top_k: Optional[int] = None
        top_p: Optional[float] = None
        temperature: Optional[float] = None
        speed: Optional[float] = None
        sample_steps: Optional[int] = None
        if_sr: Optional[bool] = None
        pause_second: Optional[float] = None

    app = FastAPI(title="Chill AIMod TTS v3", version="0.1-phase1")

    @app.get("/health")
    async def health():
        ok = _iv_state["module"] is not None
        return {
            "ok": True,
            "models_loaded": ok,
            "home": _HOME,
            "load_error_preview": (_iv_state.get("error") or "")[:200] if _iv_state.get("error") else None,
        }

    @app.post("/tts")
    async def tts_endpoint(body: TtsBody):  # noqa: D401
        try:
            try:
                d = body.model_dump(exclude_none=True)
            except AttributeError:
                d = body.dict(exclude_none=True)
            sr, wav_bytes = run_synthesis(d)
            headers = {"X-Sample-Rate": str(sr)}
            return Response(content=wav_bytes, media_type="audio/wav", headers=headers)
        except FileNotFoundError as e:
            return JSONResponse(
                {"message": str(e), "code": "ref_audio_not_found"},
                status_code=400,
            )
        except ValueError as e:
            return JSONResponse({"message": str(e), "code": "validation"}, status_code=400)
        except Exception as e:  # noqa: BLE001
            tb = traceback.format_exc()
            return JSONResponse({"message": str(e), "code": "internal", "trace": tb[-4000:]}, status_code=500)

    return app


def main():
    os.environ["_CHILL_BRIDGE_HOME"] = _HOME

    print(f"[ChillBridge] GPT-SoVITS home: {_HOME}")
    print(f"[ChillBridge] gpt={os.environ.get('gpt_path')}")
    print(f"[ChillBridge] sovits={os.environ.get('sovits_path')}")

    app = build_app()

    if args.preload:
        print("[ChillBridge] Preloading inference_webui (may take tens of seconds)...")
        ensure_inference_webui()
        print("[ChillBridge] Models loaded.")

    import uvicorn  # pylint: disable=import-outside-toplevel

    uvicorn.run(app, host=args.host, port=args.port, log_level="info")


if __name__ == "__main__":
    main()

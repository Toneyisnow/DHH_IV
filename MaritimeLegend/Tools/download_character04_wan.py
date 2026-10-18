"""Download only the three official Wan 2.2 5B ComfyUI model files."""
import concurrent.futures
import hashlib
import json
from pathlib import Path
import sys
import time
import urllib.request

root = Path(sys.argv[1]).resolve() / "models"
repo = "https://huggingface.co/Comfy-Org/Wan_2.2_ComfyUI_Repackaged"
files = [
    "diffusion_models/wan2.2_ti2v_5B_fp16.safetensors",
    "text_encoders/umt5_xxl_fp8_e4m3fn_scaled.safetensors",
    "vae/wan2.2_vae.safetensors",
]
with urllib.request.urlopen(repo.replace("huggingface.co/", "huggingface.co/api/models/") + "/tree/main/split_files?recursive=true", timeout=60) as response:
    metadata = {item["path"]: item for item in json.load(response)}

def download(relative):
    target = root / relative
    target.parent.mkdir(parents=True, exist_ok=True)
    info = metadata["split_files/" + relative]
    expected = info["size"]
    sha = info.get("lfs", {}).get("oid")
    if target.exists() and target.stat().st_size == expected:
        print("Already downloaded:", relative, flush=True)
        return
    partial = target.with_suffix(".part")
    offset = partial.stat().st_size if partial.exists() else 0
    request = urllib.request.Request(repo + "/resolve/main/split_files/" + relative)
    if offset:
        request.add_header("Range", f"bytes={offset}-")
    with urllib.request.urlopen(request, timeout=120) as response:
        if response.status != 206:
            offset = 0
        last = time.monotonic()
        with partial.open("ab" if offset else "wb") as output:
            while chunk := response.read(8 * 1024 * 1024):
                output.write(chunk)
                offset += len(chunk)
                if time.monotonic() - last > 30:
                    print(f"{target.name}: {offset / expected:.1%} ({offset / 1e9:.2f}/{expected / 1e9:.2f} GB)", flush=True)
                    last = time.monotonic()
    if partial.stat().st_size != expected:
        raise RuntimeError(f"Incomplete model: {relative}")
    if sha:
        with partial.open("rb") as source:
            actual = hashlib.file_digest(source, "sha256").hexdigest()
        if actual != sha:
            raise RuntimeError(f"SHA256 mismatch: {relative}")
    partial.replace(target)
    print("Verified:", relative, flush=True)

with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
    list(pool.map(download, files))

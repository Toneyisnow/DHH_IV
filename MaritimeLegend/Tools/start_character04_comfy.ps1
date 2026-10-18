$ErrorActionPreference = 'Stop'
$taskRoot = 'H:\DHH_IV'
$env:TEMP = Join-Path $taskRoot 'Temp'
$env:TMP = $env:TEMP
$env:PIP_CACHE_DIR = Join-Path $taskRoot 'Cache\pip'
$env:HF_HOME = Join-Path $taskRoot 'Cache\huggingface'
$env:TORCH_HOME = Join-Path $taskRoot 'Cache\torch'
$env:XDG_CACHE_HOME = Join-Path $taskRoot 'Cache'
$env:CUDA_CACHE_PATH = Join-Path $taskRoot 'Cache\cuda'
$env:TORCHINDUCTOR_CACHE_DIR = Join-Path $taskRoot 'Cache\torchinductor'
$env:TRITON_CACHE_DIR = Join-Path $taskRoot 'Cache\triton'
$env:PYTHONDONTWRITEBYTECODE = '1'
$env:YOLO_CONFIG_DIR = Join-Path $taskRoot 'Cache\ultralytics'
$env:MPLCONFIGDIR = Join-Path $taskRoot 'Cache\matplotlib'
$env:NO_ALBUMENTATIONS_UPDATE = '1'
@($env:TEMP, $env:PIP_CACHE_DIR, $env:HF_HOME, $env:TORCH_HOME, $env:CUDA_CACHE_PATH, $env:TORCHINDUCTOR_CACHE_DIR, $env:TRITON_CACHE_DIR) | ForEach-Object { New-Item -ItemType Directory -Force -Path $_ | Out-Null }
Set-Location (Join-Path $taskRoot 'ComfyUI')
& '.\.venv\Scripts\python.exe' -u main.py --listen 127.0.0.1 --port 8188 --lowvram --offline --disable-auto-launch --disable-all-custom-nodes --whitelist-custom-nodes ComfyUI-AdvancedLivePortrait Character04FaceAnalysis

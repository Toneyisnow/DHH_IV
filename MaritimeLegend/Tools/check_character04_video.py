"""Read generated video metadata and extract unchanged frames for visual review."""
import argparse
import json
from pathlib import Path
import av

parser = argparse.ArgumentParser()
parser.add_argument('video', type=Path)
parser.add_argument('--output', type=Path, required=True)
args = parser.parse_args()
args.output.mkdir(parents=True, exist_ok=True)
with av.open(str(args.video)) as container:
    stream = container.streams.video[0]
    report = {
        'file': str(args.video.resolve()),
        'width': stream.width,
        'height': stream.height,
        'fps': float(stream.average_rate),
        'codec': stream.codec_context.name,
        'declared_frames': stream.frames,
    }
    frames = list(container.decode(video=0))
    report['decoded_frames'] = len(frames)
    report['duration_seconds'] = len(frames) / float(stream.average_rate)
    for index in sorted({0, len(frames)//4, len(frames)//2, 3*len(frames)//4, len(frames)-1}):
        frames[index].to_image().save(args.output / f'{args.video.stem}-frame-{index:03}.png')
    (args.output / f'{args.video.stem}-metadata.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
    print(json.dumps(report))

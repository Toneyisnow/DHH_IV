"""ComfyUI diagnostic node for the installed AdvancedLivePortrait plugin."""
import json
from pathlib import Path
import sys
import cv2
import numpy as np
import torch
import folder_paths
from PIL import Image

class Character04FaceAnalysis:
    @classmethod
    def INPUT_TYPES(cls):
        return {'required': {'image': ('IMAGE',), 'crop_factor': ('FLOAT', {'default': 1.7, 'min': 1.0, 'max': 3.0})}}

    RETURN_TYPES = ('EDITOR_LINK', 'IMAGE', 'MASK')
    RETURN_NAMES = ('prepared_face', 'detection_overlay', 'face_edit_mask')
    FUNCTION = 'analyze'
    CATEGORY = 'Character04/Analysis'
    OUTPUT_NODE = True

    def analyze(self, image, crop_factor):
        module = next(m for name, m in list(sys.modules.items()) if name.endswith('ComfyUI-AdvancedLivePortrait.nodes'))
        engine = module.g_engine
        rgb = (image[0].cpu().numpy() * 255).clip(0, 255).astype(np.uint8)
        boxes = engine.get_face_bboxes(rgb)
        if len(boxes) != 1:
            raise ValueError(f'Expected exactly one detected face, got {len(boxes)}. Do not animate a fallback full-image crop.')
        prepared = engine.prepare_source(image, crop_factor)
        report = {
            'detector': 'face_yolov8n', 'detection_threshold': 0.7,
            'face_count': len(boxes), 'face_boxes_xyxy': boxes.tolist(),
            'image_size': [rgb.shape[1], rgb.shape[0]],
            'crop_transform': prepared.crop_trans_m.tolist(),
            'feature_volume_shape': list(prepared.f_s_user.shape),
            'keypoint_type': '21 learned implicit 3D motion keypoints, NOT anatomical face landmarks or a body skeleton',
            'source_motion': {k: v.detach().cpu().tolist() for k, v in prepared.x_s_info.items()},
            'transformed_keypoints': prepared.x_s_user.detach().cpu().tolist(),
        }
        out = Path(folder_paths.get_output_directory()) / 'Character04B-Face'
        out.mkdir(parents=True, exist_ok=True)
        (out / 'face-analysis.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
        overlay = rgb.copy()
        x1, y1, x2, y2 = boxes[0].astype(int)
        cv2.rectangle(overlay, (x1, y1), (x2, y2), (50, 210, 100), 2)
        cv2.putText(overlay, 'Detected face / edit region only', (12, rgb.shape[0]-20), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (40, 120, 60), 2)
        mask = prepared.mask_ori[..., 0]
        return ([prepared], torch.from_numpy(overlay.copy()).float().unsqueeze(0)/255, torch.from_numpy(mask.copy()).unsqueeze(0))

NODE_CLASS_MAPPINGS = {'Character04FaceAnalysis': Character04FaceAnalysis}
NODE_DISPLAY_NAME_MAPPINGS = {'Character04FaceAnalysis': 'B portrait: detect and analyze face'}

class Character04CapeWind:
    @classmethod
    def INPUT_TYPES(cls):
        return {'required': {'images': ('IMAGE',), 'wind_strength': ('FLOAT', {'default': 1.0, 'min': 0, 'max': 2, 'step': 0.1}), 'pendant_degrees': ('FLOAT', {'default': 7.0, 'min': 0, 'max': 12, 'step': 0.5})}}

    RETURN_TYPES = ('IMAGE', 'MASK')
    RETURN_NAMES = ('animated_images', 'local_motion_regions')
    FUNCTION = 'animate'
    CATEGORY = 'Character04/Animation'

    def animate(self, images, wind_strength, pendant_degrees):
        batch = images.cpu().numpy()
        count, height, width, _ = batch.shape
        if (width, height) != (704, 768):
            raise ValueError('This painted cape rig is authored for the 704x768 B half-body portrait.')
        yy, xx = np.mgrid[:height, :width].astype(np.float32)

        def region(points, feather=4):
            mask = np.zeros((height, width), np.float32)
            cv2.fillPoly(mask, [np.array(points, np.int32)], 1.0)
            return cv2.GaussianBlur(mask, (0, 0), feather)

        shoulder = region([(281,252),(309,264),(324,299),(283,322),(230,353),(169,391),(104,434),(84,423),(121,371),(169,317),(224,272)])
        shoulder *= np.clip((320-xx)/190, 0, 1) * np.clip((yy-250)/110, 0, 1)
        shoulder *= np.clip((np.hypot(xx-320, yy-296)-36)/30, 0, 1)
        left_cuff = np.zeros_like(shoulder)
        right_cuff = np.zeros_like(shoulder)
        if getattr(self, 'sleeve_motion', False):
            shoulder[:] = 0
            # Animate only the loose white fabric above each leather wrist cuff.
            left_cuff = region([(153,481),(183,484),(219,498),(242,515),(234,527),(217,523),(195,516),(172,513)], 2)
            right_cuff = region([(592,555),(602,566),(614,588),(623,611),(615,620),(601,610),(592,590),(585,574)], 2)
        left_tail = region([(128,598),(156,624),(207,644),(222,655),(196,724),(181,768),(104,768),(102,699)])
        left_tail *= np.clip((yy-600)/165, 0, 1)
        right_tail = region([(613,690),(636,725),(658,768),(577,768),(563,727),(575,716),(607,704)])
        right_tail *= np.clip((yy-690)/78, 0, 1)
        pendant = region([(332,671),(346,673),(349,699),(343,724),(327,724),(322,699),(328,680)], 2)
        # Rotate around the ring below the compass; the compass disk itself stays fixed.
        pendant *= np.clip((yy-671)/8, 0, 1)
        all_regions = np.maximum.reduce([shoulder, left_tail, right_tail, pendant, left_cuff, right_cuff])
        out = []
        for index, frame in enumerate(batch):
            phase = 2*np.pi*index/max(count-1, 1)
            if index in (0, count-1):
                out.append(frame.copy())
                continue
            wave = np.sin(phase)
            lag = np.sin(phase-0.45)-np.sin(-0.45)
            dx = wind_strength*(shoulder*6.5*wave + left_tail*8.0*lag + right_tail*5.5*wave)
            dy = wind_strength*(shoulder*2.8*np.sin(2*phase) + left_tail*1.5*wave + right_tail*1.5*lag)
            dx += wind_strength*(left_cuff*1.8*wave - right_cuff*1.5*lag)
            dy += wind_strength*(left_cuff*0.9*np.sin(2*phase) + right_cuff*0.8*wave)
            angle = np.deg2rad(pendant_degrees)*np.sin(phase)
            px, py = xx-338, yy-672
            inverse_x = np.cos(angle)*px + np.sin(angle)*py + 338
            inverse_y = -np.sin(angle)*px + np.cos(angle)*py + 672
            map_x = xx-dx + pendant*(inverse_x-xx)
            map_y = yy-dy + pendant*(inverse_y-yy)
            warped = cv2.remap(frame, map_x.astype(np.float32), map_y.astype(np.float32), cv2.INTER_CUBIC, borderMode=cv2.BORDER_REFLECT_101)
            # Preserve untouched pixels exactly, including the original facial animation.
            active = (np.abs(map_x-xx)+np.abs(map_y-yy)) > 0.01
            result = frame.copy()
            result[active] = warped[active].clip(0, 1)
            out.append(result)
        return (torch.from_numpy(np.stack(out)), torch.from_numpy(all_regions).unsqueeze(0))

NODE_CLASS_MAPPINGS['Character04CapeWind'] = Character04CapeWind
NODE_DISPLAY_NAME_MAPPINGS['Character04CapeWind'] = 'B portrait: pinned cape and pendant motion'

class Character04SleeveWind(Character04CapeWind):
    sleeve_motion = True

NODE_CLASS_MAPPINGS['Character04SleeveWind'] = Character04SleeveWind
NODE_DISPLAY_NAME_MAPPINGS['Character04SleeveWind'] = 'B portrait: sleeve cuffs and pendant motion'

class Character04OriginalBlink:
    @classmethod
    def INPUT_TYPES(cls):
        return {'required': {}}
    RETURN_TYPES = ('IMAGE',)
    FUNCTION = 'load'
    CATEGORY = 'Character04/Animation'
    def load(self):
        directory = Path(folder_paths.get_output_directory())/'Character04B-Face'/'frames'
        paths = [directory/f'blink_{index:05}_.png' for index in range(1, 49)]
        frames = [np.asarray(Image.open(path).convert('RGB'), dtype=np.float32)/255 for path in paths]
        return (torch.from_numpy(np.stack(frames)),)

NODE_CLASS_MAPPINGS['Character04OriginalBlink'] = Character04OriginalBlink
NODE_DISPLAY_NAME_MAPPINGS['Character04OriginalBlink'] = 'B portrait: original 01 lossless frames'

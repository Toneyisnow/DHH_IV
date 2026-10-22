# V2 save-selection portraits

The save scene now samples approved V2 illustrations rather than drawing block avatars. Source originals stay in `../ModularWardrobe/`; exact copies used as runtime atlases are under `MaritimeLegend/Assets/Resources/Sprites/Characters/Character01` through `Character08`, each in `SavePortraitV2/`. Asian female uses the approved slim-waist board. Source files are not repainted, recolored, or replaced.

`Appearance.portraitV2` is an optional complete illustrated preset identifier (`01-C1`, `04-H8`, `05-M3`, `08-A3`). C/H have eight options; M/A have six. These refer to complete drawings already on the boards, not independently compositable layers. An explicit preset survives serialization and takes priority over legacy preview selection. Future character creation should set it only when the corresponding actual outfit is selected, or replace this preset system with validated production layers.

Legacy fallback, without rewriting save files:

| Old baseId | V2 character |
| --- | --- |
| 0 / 1 | 03 Asian male / 04 Asian female |
| 2 / 3 | 01 European male / 02 European female |
| 5 | 04 Asian female, explicitly approved by the user for existing slot 1 |
| 6 / 7 | 05 African male / 06 African female |
| 4 / 8 / 9 | Unmapped; do not change identities silently |

Legacy cloak flag selects a complete M preset using outfitId; otherwise a hat flag selects an H preset using hairId; otherwise primaryColor selects a C preset. This is a compatibility preview, not an exact conversion of old palette colors, hairstyles, or accessory combinations. In particular, cloak and hat drawings are not composited together. Unmapped or malformed appearance keys use the neutral card while preserving the saved character's name and load action.

Card aspect ratio stays 340 × 430, with proportional artwork, a paper mat, and a narrow frame. Headwear rows are letterboxed to avoid stretching faces or cropping broad hats. Highlight and hit geometry follow the lifted card. Preview textures are cached per slot and destroyed when leaving the scene.

Validation: C# compile and headless compatibility/serialization checks. Unity visual/play-mode verification was attempted but could not start because automatic approval review reported its model was at capacity. No runtime screenshot or passed visual check is claimed.


## Restored frame and waist crop revision

The original `SaveSelection/card1.png` ornate frame is reused, replacing only its inner 376×348 photo opening at (76,120) of the 530×600 source. The original `frame_glow` and 1.113 scale are restored. Portraits now fill that fixed opening without individual mats or letterboxing. C/M/A board crops use waist landmarks with the same aspect ratio.

`WaistPortraits/01-H4.png` and `06-H2.png` were authored using built-in imagegen against the corresponding V2 wardrobe boards to extend shoulder-only H drawings through their waist belts. Runtime copies are `Waist-H4.png` and `Waist-H2.png` in their character SavePortraitV2 resource folders. Preserve the approved face/headwear and C1 costume. The runtime crops the completed images to their belt landmarks. Other H variants without a completed waist asset temporarily use the complete C1 figure; they do not display a shoulder-only crop.

Card captions use `sailing.calendar.elapsedGameDays` and the existing `GameCalendar.Date` format; old saves with no calendar start at day zero, matching the game's migration. Real-world save timestamps remain in the hover details.


## Costume-only identity correction

The first generated H4/H2 waist images were rejected for differing from the approved V2 anatomy/style. They remain historical files only and are no longer the runtime source. Current runtime H4/H2 images are copied from `WaistPortraits/01-H4-identity-locked-v2.png` and `06-H2-identity-locked-v2.png`. The female edit uses the user's original-character crop as its sole anatomical reference. The male edit uses `01-Europe-v2-slim.png` for identity/body and the wardrobe board only for the H4 cap. Body shape, pose and face must not be redesigned as part of costume editing. These are generative costume edits, not pixel-identical layered composites; retain the originals as the authority for future review.

# Art asset layout

- Keep editable art sources, Blender files, generation workflows, intermediate renders, and art review material in repository-root `ArtSource/`. Do not recreate `MaritimeLegend/ArtSource/` inside the Unity project.
- Copy the latest game-ready assets into `MaritimeLegend/Assets/Resources/`, organized by asset type, then character or feature (for example `Videos/Characters/Character04/`, `Sprites/Characters/Character04/`, and `Models/Characters/Character04/`). Keep source originals separately.
- Local AI models, runtimes, download caches, and temporary generation files belong in `H:/DHH_IV/` when D: has insufficient space. Do not create additional model environments or caches on C:.

# Character art direction (user-confirmed 2026-10-18)

- The approved character art is the second-version set in `ArtSource/CharacterArt_V2/`: eight characters, male and female for Europe, Asia, Africa, and South America/Latin America, in the user's requested 1600s clothing direction. Preserve their established identities and designs.
- A save-slot "half portrait" means this illustrated character cropped consistently from the top of the head/headwear through the waist belt (updated user direction), including the character's equipped headwear, cloak/mantle, and visible accessories. It does **not** mean a screenshot or rasterization of the temporary block-character renderer.
- Wardrobe references are in `ArtSource/CharacterArt_V2/ModularWardrobe/catalog.json` and its character boards; accessory references are in `ModularWardrobe/Accessories/`. Preserve the C (color), H (headwear/hair), M (mantle), A (accessory) identifiers. Use the latest corrected boards named in the catalog, including the slim-waist Asian female revision.
- The user plans to replace the block characters in character creation with this same eight-character set. Character creation, saved appearance data, and save-slot portraits must ultimately share the same character and equipment selection; do not build a separate block-based portrait system.
- These existing boards are concept sheets, not production-ready transparent layers. Do not claim arbitrary C/H/M/A composition is supported until correctly aligned layers or complete matching outfit renders have been authored and verified. The legacy ten-template `baseId` numbering is not the V2 eight-character numbering; migrate explicitly without silently reinterpreting old saves.
- Save cards must preserve the original ornate card frame and original frame_glow highlight. Replace only the common inner photo opening (x=76, y=120, width=376, height=348 in the original 530×600 card); retain the original highlight scale 1.113. No new thin frame or variable photo letterboxing. All portraits end at the waist belt. Labels show character name plus saved in-game date, or 新档案N for empty slots.

- User approved the existing slot 1 legacy West Asian female to use the V2 Asian female portrait preview; preserve her save progress. Other unmapped legacy identities must not silently be reassigned.

- Costume variants and save portraits must retain the approved V2 figure's identity, face silhouette, head-to-body ratio, neck length, shoulder width, limb proportions, waist position, pose, and painterly style. Only clothing, cloak, headwear, and accessories may vary. Never use a head-only portrait to regenerate the torso; use the approved full figure as the edit base. The user's African female proportional reference is preserved at `ArtSource/CharacterArt_V2/SaveSelection/WaistPortraits/06-user-approved-proportions.png`. Earlier enlarged-head/fashion-portrait waist renders are rejected references, not anatomy masters.

# Trading UI terminology

- Ship and warehouse cargo slots are uniformly called “货仓” in player-facing Chinese text. Use “空货仓” for empty slots; avoid “货舱”, “船舱”, and “仓位” for these slots.

# Art asset layout

- Keep editable art sources, Blender files, generation workflows, intermediate renders, and art review material in repository-root `ArtSource/`. Do not recreate `MaritimeLegend/ArtSource/` inside the Unity project.
- Copy the latest game-ready assets into `MaritimeLegend/Assets/Resources/`, organized by asset type, then character or feature (for example `Videos/Characters/Character04/`, `Sprites/Characters/Character04/`, and `Models/Characters/Character04/`). Keep source originals separately.
- Local AI models, runtimes, download caches, and temporary generation files belong in `H:/DHH_IV/` when D: has insufficient space. Do not create additional model environments or caches on C:.

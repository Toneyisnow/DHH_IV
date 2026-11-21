# Town lighting reference study — 2026-11-08

Applied to the runtime CityDiorama used by CityScene.

- Sun: 48 degree elevation, -48 degree yaw; warm light at 1.22 intensity on clear days.
- Cool ambient fill keeps shaded walls readable; stronger clear-day shadows, softer overcast shadows.
- Lower shadow normal bias keeps building and character shadows anchored.
- Blue-grey aerial perspective starts behind the player focus band; exponential falloff avoids a hard fog saturation plane.
- Reduced sepia grading, saturation and vignette; wider sharp band and gentler depth blur.
- Daytime lantern spill reduced to avoid orange pools competing with sunlight.

Review captures and build/runtime logs belong in Review/. Run the existing city smoke suite with -cityArtOutput pointing there to preserve previous character review material.

Validation: C# compile passed (two pre-existing warnings); isolated Unity player build passed; full CITY_ART_PLAYMODE_PASS suite passed. Inspected Town-layout and Venice-rain runtime renders. Retained town/weather screenshots; redundant character-only captures removed.

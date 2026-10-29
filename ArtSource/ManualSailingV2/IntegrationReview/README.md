# V2 manual sailing integration — 2026-10-27

Game entry: city → 港口 → 手动航行. The existing SailingScene remains the functional route (also used by battle), and loads the SoutheastAsiaV2 runtime prefab as its presentation. Simulation, supplies, encounters, calendar, save slot and destination remain owned by the existing game.

Controls: left-click heading target; A/D sail trim; W/S opening; R reset trim; Space stop/resume; right-drag yaw; wheel distance; 1/2/3 sunny/sunset/night. Right-side sail and helm instruments retain their original controls. The existing eight-direction ship art is used with upper-canvas deformation for trim/opening/billow, not a new 3D sail rig. Followers use collision-safe spacing.

Camera: SailingScene exposes pitch (default 35), minimumZoom and maximumZoom. Sky fraction is 15%. Zoom is now 24–120; a camera-facing exploration composite removes the former above-fog minimum-distance restriction. Runtime renders to 1920×1080 on isolated layer 28; leaving sailing hides the layer and restores global sky/lighting.

Clouds: a shared 600-triangle, five-puff mesh is GPU-instanced at seeded random positions. Cloud base is now Y=7.5, another 25% lower than Y=10. Surface alpha is 0.75 (less transparent), with stepped cartoon lighting. Very close clouds fade out to avoid obstructing the camera. Terrain and water shadows use the actual cloud mesh silhouette projected along the current light direction, with a 2048x1536 periodic mask and a narrow bilinear edge. Their core attenuation is 52% and 48% in linear shading, respectively. No per-cloud GameObjects.

Exploration: camera-facing fog composite, using sea-plane ray intersections and land depth; axial hex radius 12 world units; 96×64 state grid. Values 0=fully covered, 1=40% covered, 2=open. The lower half of the screen uses black; the 75-85% screen-height band blends toward the active sky color at the 85% horizon (10% of screen height). Coverage opacity remains unchanged, including over distant elevated terrain. Sight uses the existing world vision balance, vision skill and scout/navigator bonuses, measured as Euclidean world distance from the actual ship position. Hex centers inside that circle open; a band one tile spacing (20.7846 world units) beyond it is half-covered. The displayed contour follows hex tiles with a softened noisy edge. Promotions only increase, including when vision shrinks or the ship leaves. State persists in SailingState.southeastAsiaExploration; return/depart and save/load do not reset it. The shader maps sea view rays to sea level and reconstructs terrain hits from depth, so mask coordinates remain attached to the map even below cloud height. Linear-space alpha is corrected so 40% black is visually distinct. Noisy, softened boundaries retain fully opaque unknown interiors.

Old regional saves retain economy/progress; on first V2 entry their former map/position are retained in previousManualMapId/previousManualX/previousManualY and the fleet is relocated to a validated South China Sea anchorage with a notice. Regional coordinates do not discover unrelated world-map ports. Existing regional return-to-origin behavior is retained; this change does not author a new network of destination ports on the Southeast Asia terrain.

Performance: original chunk LODs and instanced props retained; fog adds one quad; clouds use one shared instanced puff mesh. Circle classification updates with ship position/vision; the hex texture uploads only when persistent tile states change or a save is rebound. 6,144 integer states per save; no hex GameObjects. Scene remains active during sailing to avoid repeatedly enabling hundreds of LOD renderers.

Validation:
- C# compile: pass (existing mapPan warning).
- DataConfigurationChecks, ManualSailingChecks, SailingArchitectureChecks, scene build-list validation: pass.
- Full integrated Windows player build and new shader checks: pass.
- Runtime: world-distance circle classification at centered and off-center ship positions, one-tile partial band, monotonic reveal, JSON round trip, safe anchorage, 35° camera, project/unproject, trim, panel click rejection, target movement, stop, SaveStore save/load, return/depart persistence, inactive scene after leaving: pass.
- Existing HarborFlowChecks: actual 港口 → 手动航行 button, rejected departure without supplies, auto route option, selected destination/state preservation: pass.
- Screenshots reviewed: game-manual-sailing.png, map-cloud-fog.png.

The standard BuildGame.Build pipeline still reports an unrelated DomainChecks failure: its fixed expectation of 50 products does not match the current catalog. That test was not removed or weakened. The dedicated MaritimeLegend.Editor.SailingV2Integration.Build entry builds the same eight functional scenes and runs the sailing-specific checks above.

To rebuild: use a separate Unity checkout/staging project while the main editor is open, then run -executeMethod MaritimeLegend.Editor.SailingV2Integration.Build. To regenerate the prefab from the standalone scene: Maritime Legend → Manual Sailing V2 → Prepare integrated V2 scene.


## Zoom, cloud and circular visibility revision
- Endpoints 15/120 and intermediate 60 tested with opaque unknown fog and an unobscured sky.
- A deliberately unknown elevated object above the sea horizon is hidden by depth-aware fog at distance 15.
- Same-pixel comparison with and without fog: half-covered background retains 0.5846 of displayed brightness.
- Existing explored cells are not reset or darkened; old saves can retain a larger historical footprint than the new current sight circle. Perspective makes the near side look larger in pixels; visibility is symmetric in world distance, not screen distance.
- Final build: Builds/SailingV2-CircleClouds/StartWindowed.cmd (1920×1080).


## Sail power and low cartoon clouds revision
- Existing full-opening cruise speed is the 1x reference: windSpeed × 4 × skill-resolved sailing speed.
- Actual speed = reference × opening × (0.02 + 1.98 × wind fill).
- Wind fill uses the same sail-angle/wind-direction alignment as the right-hand instrument. A/D changes fill, W/S changes opening. Furled sails or Space stop give zero displacement; full opening and best fill give 2x reference; weakest fill gives 0.02x reference. Collision handling still limits final displacement.
- Instrument fill and canvas billowing are synchronized with opening/wind fill; the HUD shows sail opening and power multiplier.
- Added actual-displacement checks for max 2x, half opening, furled stop, weakest/adverse wind and monotonic improvement with sail alignment.
- Camera range updated to 24–120. Circle fog, partial band, elevated occlusion and save persistence regressions retained.
- Final player: Builds/SailingV2-SailClouds/StartWindowed.cmd. Screenshots/logs: ArtSource/ManualSailingV2/SailCloudReview/. cloud-detail.png is a dedicated cloud inspection image with the fog veil temporarily hidden; gameplay screenshots retain normal exploration fog.


## Lower, denser clouds and stronger shadows
- Base height 20 → 15 (25% lower); puff shapes and 75% surface opacity retained.
- Actual deterministic map population 219 → 438 clusters. Eligibility threshold calibrated to this grid, rather than assuming a perfectly uniform hash distribution.
- Ground cloud shadows strengthened and also applied to the water surface. Projection follows sunlight direction and stops above cloud-top height. No per-cloud shadow cameras or shadow-map passes added.
- Player: Builds/SailingV2-LowCloudShadows/StartWindowed.cmd. Review images and build/runtime logs: ArtSource/ManualSailingV2/LowCloudShadowReview/.


## Random clouds following wind
- 438 random cloud positions per save, with spacing checks to reduce overlap; no row/column grid pattern.
- Seed and accumulated X/Z drift are additive SailingState fields. Re-entering/loading retains the same weather layout and position.
- Cloud velocity reads the current gameplay wind direction and uses windSpeed × 4 × minSailSpeed × 0.02 in map UV units (the weakest nonzero sail speed at minimum navigation skill). UV displacement is converted using the same 960×540 world mapping as ships. Stopping the ship does not stop clouds; pausing gameplay does.
- Integrate velocity into a double-precision periodic offset. Wind changes update direction without recalculating past motion or teleporting clouds. Periodic wrapping occurs beyond view range.
- Cloud shadows use a once-per-seed baked 1024×768 footprint texture with the identical offset. CPU mesh positions and shader shadows remain aligned without per-frame mask uploads or per-cloud shadow cameras.
- Runtime checks passed: eastward drift speed, changed-wind continuity, seed/drift serialization, existing fog/camera/sailing regressions. Screenshot inspected.
- Player: Builds/SailingV2-WindClouds/StartWindowed.cmd. Review: ArtSource/ManualSailingV2/WindCloudReview/.


## Ship-height clouds, silhouette shadows and horizon fog
- Cloud base 15 -> 10. Five-puff shape, random population and persistent wind drift retained.
- Shadow mask rasterizes the actual low-poly cloud triangles projected along the active sun/moon light. The same mask drifts with the visible meshes; elevated terrain corrects the sample along that light ray. Mask regenerated only when seed, light or height changes. Bilinear edge is about one 0.7-world-unit texel, replacing the old broad elliptical blur.
- Sky and exploration shaders share the exact palette function. Screen heights <=50% stay black; 50-85% transitions to horizon color; unknown elevated geometry above the sea horizon matches the sky behind it. Alpha and permanent discovery are unchanged.
- Temporary Sunny / Sunset / Night buttons at the top of the sailing screen switch visual lighting only. Keyboard 1/2/3 remains available; saved calendar and gameplay weather are unchanged.
- Validation: dedicated eight-scene build and V2 integration checks passed; all three sky modes, lower-half black, 40% band, elevated terrain occlusion, 24/60/120 zoom, cloud height/drift/save persistence. Final screenshots inspected, including horizon smoothing to remove a thin sky seam. Player: Builds/SailingV2-HorizonClouds/StartWindowed.cmd (1920x1080 windowed). Evidence: ArtSource/ManualSailingV2/HorizonCloudReview/.


## Narrow horizon blend and lower clouds
- Horizon transition now occupies exactly 10% of viewport height: 75% to 85% measured from the bottom (15%-25% from the top), immediately below the unchanged 15% sky region. Fully covered pixels below the transition stay black.
- Cloud base 10 -> 7.5 (25% lower). Mesh-projected shadows automatically follow the new height; density, wind drift and cloud shape are unchanged.
- Validation: dedicated build and V2 runtime regression passed, including black fog immediately below the 10% transition in all three sky modes and cloud base 7.5. Screenshot inspected. Player: Builds/SailingV2-NarrowHorizon/StartWindowed.cmd (1920x1080 windowed); evidence: ArtSource/ManualSailingV2/NarrowHorizonReview/.

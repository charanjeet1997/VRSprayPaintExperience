# VR Spray Painting

A realistic spray-painting prototype for Meta Quest 2 / Quest 3, built with Unity 6, URP and OpenXR.
Pick up a spray gun, find the right distance, and lay down paint that builds up, blends and covers like real paint.

![Paint booth overview](Docs/images/overview.png)

| Spray gun | Paint palette |
|---|---|
| ![Spray gun](Docs/images/spray_gun.png) | ![Palette](Docs/images/palette.png) |
| **Guided onboarding** | **Baked fixture lighting** |
| ![Onboarding guide](Docs/images/guide_panel.png) | ![Booth lighting](Docs/images/booth_ceiling.png) |

## Features

### Spray gun
- **Real spray cone.** The spot grows with distance (distance × tan(cone/2)). Tilting the gun stretches it into an ellipse.
- **Distance zones**, shown by a ring on the surface:

  | Ring | Distance | Result |
  |---|---|---|
  | Orange | under 15 cm | wet, heavy |
  | Green | 15–25 cm | sweet spot |
  | Yellow | 25–40 cm | dry, speckled |
  | Red | over 40 cm | no paint |

- **Close range.** The gun still paints very close to the surface. It stops only when the nozzle clips into it.
- **Adjustable cone.** Thumbstick up gives a tighter spray, down a wider one (8°–35°).
- **Feedback.** VFX Graph droplets and mist, spray sounds (valve, hiss, choke-off) and controller haptics.

### Paint simulation
- **Paint texture.** A half-float texture per surface stores colour plus paint volume (coats).
- **Natural colour blending.** Uses Kubelka–Munk film-over-background mixing:
  - A thin new coat tints the colour underneath.
  - A heavy coat covers it.
  - Sparse overspray stays as separate dots.
- **Fast coverage.** One pass covers most of the surface, like real spray paint. The result doesn't depend on frame rate.
- **Performance.** Each stamp only redraws its own small rectangle of the texture, which keeps it cheap on Quest.

### Real-world defects
- **Runs and drips.** Paint thicker than it can hold runs: a drip bead slides down the panel, leaving a narrow trail of the same colour and ending in a bead (`PaintRunSimulator`). Holding still, going too slow or spraying too close causes them.
- **Dry spray** when too far, **sputter** from an unshaken can, **heavy edges** from starting/stopping on the panel.

### Spray can realism
- **Shake before use.** Real shaking is detected from the can's movement: each stroke clacks (mixing ball) and buzzes the controller. The mix settles back over ~90 s.
- **Unshaken = sputter.** Less paint, spits in spurts, grainy coat.

### Spray gun or spray can
Choose the sprayer on the main menu (saved). Each is a `SprayerProfile` asset (`Resources/Sprayers`):

| | Spray gun (HVLP) | Spray can (aerosol) |
|---|---|---|
| Working distance (green) | 15–25 cm | 20–30 cm |
| Max distance | 40 cm | 45 cm |
| Spray pattern | Adjustable with the thumbstick (8–35°) | Fixed round spray (25°) |
| Paint flow | Higher | Lower |
| Sound | Air hiss | Aerosol hiss, button click |

Lessons, the live readings and the feedback use the active sprayer's range. Lesson progress is kept per sprayer, and a lesson can be limited to one sprayer. A new sprayer = a profile asset plus a `SprayerModel` (look, nozzle, grip, colliders, resting spot) on the held sprayer.

### VR hands
Controllers show as hands. Grip closes the hand, the trigger curls the index finger (`ControllerHandPoser`).

### Training: lessons and assessment
This is a training simulator, not a game. It teaches one real-world skill at a time and scores you against real-world standards.

- **Main menu.** Pick a lesson, Free Paint or the getting-started tour.
- **Lessons**, unlocked in order (pass one to open the next). Each lesson runs **Explain → Watch → Try → Report → Result**:

  | # | Lesson | Live reading | Practice goal | Scored on |
  |---|---|---|---|---|
  | S | Safety and prep | Checklist | Respirator + goggles on, booth fan on | (completion) |
  | 0 | Shake the can *(can only)* | Mix % | Shake until mixed, then spray | Can mixed |
  | 1 | Hold the sprayer straight | Angle | 10 s spraying | Sprayer angle |
  | 2 | Keep the right distance | Distance | 10 s spraying | Distance |
  | 3 | Move at a steady speed | Speed | 12 s spraying | Speed, Even coat |
  | 4 | Overlap your passes | Distance | 6 passes | Coverage, Even coat |
  | 5 | Trigger on and off the edge | none | 4 passes | Trigger control, Coverage |
  | 7 | Two thin coats | Distance | Coat 1 (6 passes) → wait to dry → coat 2 | Coverage, Even coat |
  | 8 | Final test | none (cues off) | Safety check, then paint the panel | All skills + all defects |
- **Watch.** The real gun demonstrates the right and the wrong way (tilted, too close, too far) and you see the result on the panel.
- **Try.** A live reading above the gun (angle or distance) turns green when you are doing it right.
- **Scorecard report** (two pages):
  - *Summary*: score vs the pass mark, number of coats, **film thickness in µm** against the target (bare / thin / on target / heavy), **defects with point deductions** (runs & sags, dry spray, orange peel, missed spots, safety gear) and what to fix.
  - *Technique*: a bar per skill, a score **per coat**, and a **graph of distance and angle over time** with the target bands.
- **Assessment report (details).** Scores per skill (gun angle, distance, speed, coverage, even coat, paint on target) and plain-language feedback. It names the mistake, what it causes in real life and how to fix it, e.g. *"You held the spray on one spot for 1.4 s. In real life that much paint runs and drips."*
- **Thickness view.** A heat map of the coat: blue = see-through, green = good, red = too thick (would run).
- **Free Paint "Check my work".** Assesses all skills on anything you paint.
- **Progress** (best score, passed) is saved on the device.
- **History**: every attempt is saved (date, activity, sprayer, score, coats, film, defects, skills) with the trend, and can be **exported as CSV** (`Application.persistentDataPath/attempts.csv`; on Quest under `Android/data/<package>/files`).
- **Instructor settings**: pass mark, defect strictness (off / small / medium / large), drying time between coats, training cues on/off.
- **How to use**: shown on first launch and from the menu: controllers, pointing at buttons, live cues, the report, safety.

### Accuracy (real-world numbers)
| | Value | Source |
|---|---|---|
| Distance | 15–25 cm (spray can ideal ~20 cm) | wikiHow, Instructables |
| Speed | 20–45 cm/s (≈ 1 ft/s) | wikiHow |
| Angle | straight on, within 10° | spray-finishing practice |
| Coats | thin coats: one pass is patchy, two coats cover; dry between coats | wikiHow, Instructables |
| Film thickness | 25 µm per unit of paint; target 40–75 µm dry film | `SprayerProfile.film` |
| Shaking | ~12–15 s of hard shaking in VR (real: 1–3 min) | wikiHow |
| Safety | respirator, goggles, ventilation before spraying | wikiHow, Instructables |

Fresh paint looks wet and glossy and dries matte; a sign under the panel counts down the drying time.

#### Training architecture (data-driven)
| Asset / type | Role | To extend |
|---|---|---|
| `LessonCatalog` | Menu order of all lessons | Add the lesson asset to the list |
| `LessonData` | Title, summary, prerequisites and an ordered list of **steps** | Create an asset; pick steps in the Inspector |
| `LessonStep` (`ExplainStep`, `DemoStep`, `PracticeStep`, `SafetyStep`, `AssessStep`) | One stage of a lesson; steps can repeat and mix | New subclass = new kind of stage |
| `PracticeGoal` (`SprayTimeGoal`, `PassCountGoal`, `ShakeGoal`, `DryGoal`) | When practice ends | New subclass (e.g. passes, coverage) |
| `DefectDefinition` (`RunsDefect`, `DrySprayDefect`, `OrangePeelDefect`, `MissedSpotsDefect`, `SafetyDefect`) | Measures one defect and deducts points (scaled by strictness) | New subclass + asset |
| `SkillDefinition` (`AngleSkill`, `DistanceSkill`, `SpeedSkill`, `CoverageSkill`, `EvennessSkill`, `OnTargetSkill`, `TriggerControlSkill`, `MixSkill`) | Scores one skill and explains a low score | New subclass + asset |
| `CoachReading` (`AngleReading`, `DistanceReading`, `SpeedReading`, `MixReading`) | Live reading on the sprayer | New subclass + asset |
| `HelpContent` | Pages of the "How to use" guide | Edit text in the Inspector |
| `IProgressStore` | Where results are saved (PlayerPrefs today) | New implementation (file, cloud, LMS) |

`LessonController` is a generic runner: it plays any step list and knows no lesson or skill in particular.
Text in lessons can use `{sweetMin}`, `{sweetMax}`, `{maxDistance}`, `{maxTilt}`, filled from the current sprayer.
Assets live in `Resources/Lessons`, `Resources/Skills` and `Resources/Coach`.

### Getting-started tour
- **Intro demo.** The real spray gun flies over and paints the panel by itself. It shows the sweet spot, too close, too far and colour blending, then returns to the stool.
- **Step-by-step guide.** Each step completes when you actually do it: grab, spray, build up a coat, change colour, reset.
- **Floating hints** point at the gun, the panel and the palette.

### Environment
- **Paint booth** with baked area lighting from the light fixtures, light probes and one realtime shadow-casting light.
- **Palette** to pick colours and clear the paint.
- **Gun return.** A dropped gun returns to its stool by itself.

## Controls (Quest)

| Action | Input |
|---|---|
| Pick up gun | Grip |
| Spray | Trigger |
| Tighter / wider spray | Thumbstick up / down |
| Press UI buttons | Point + Trigger |
| Clear all paint | B / Y (R on keyboard) |

## Requirements

- Unity **6000.3.23f1**
- URP 17.3, OpenXR 1.16, XR Interaction Toolkit 3.3.2, XR Hands 1.7, Input System 1.20, VFX Graph 17.3
- Meta Quest 2 or Quest 3 (Android build target)

## Getting started

1. Clone the repo.
2. **Import the paid plugins.** These Asset Store packages are not in the repo because their licence doesn't allow redistribution. Import them into `Assets/ExternalFramework/` before opening the project, or the shared framework won't compile:
   - **Best HTTP (Pro)** → `Assets/ExternalFramework/Best HTTP (Pro)/`
   - **EnhancedScroller** → `Assets/ExternalFramework/EnhancedScroller/`
3. Open the project in Unity 6000.3.23f1.
4. Open `Assets/VRSprayPainting/Scenes/PaintTest.unity`.
5. To run on device, switch to Android in Build Profiles, then Build & Run with the Quest connected.
6. To run in the editor, use Quest Link / Air Link with Play Mode.

### Editor tools

`Tools > VR Spray Painting`:

| Menu item | What it does |
|---|---|
| Run All / 1–3 | Installs the XR packages, sets Quest player settings and creates a minimal paint test scene |
| Build Environment | Rebuilds the paint booth (also re-bakes lighting) |
| Build Lighting (bake) | Fixture lights, light probes and lightmap bake. Re-run after moving scene geometry |
| Build Paintable Cube Mesh | Cube with a separate UV area per face, so paint doesn't show on every side |
| Build Spray VFX | Regenerates the droplet and mist VFX graphs |

## Project structure

```
Assets/VRSprayPainting/
├── Editor/         Builders for the scene, lighting, meshes and VFX
├── Resources/      Lessons/ (catalog + lessons), Skills/, Coach/
├── Scenes/         PaintTest scene + lighting settings
├── Scripts/
│   ├── Constants/  Shader IDs, data-binding / observer keys
│   ├── Controllers/SprayGun, SprayGunAudio, SprayAimMarker, SprayDemoController, SprayCoachReadout,
│   │               AppModeController, LessonController, AssessmentController,
│   │               OnboardingController, PaintResetInput, ReturnHome
│   ├── Interfaces/ IPaintable
│   ├── Models/     Painting (SprayPainter, PaintableSurface, PaintStamp),
│   │               Assessment (SprayTracker, CoatAnalyzer, AssessmentCalculator), Lessons data, Onboarding data
│   ├── UI/         Menu, lesson, report, onboarding and palette screens; lesson cards, score bars, swatches
│   └── Utils/      HintBob, LessonProgress
├── Shaders/        PaintStamp (paint deposit + KM mixing), PaintedSurface (URP lit + lightmaps)
├── Sounds/         Spray loop, start, stop, trigger click
└── VFX/            Spray droplet and mist graphs
```

`Assets/ExternalFramework/` contains a shared framework (service locator, data binding, UI system). Don't change it from this project.

## How painting works

1. **Aim.** `SprayPainter` raycasts from the nozzle and converts the cone footprint into an elliptical stamp in the surface's UV space, using the hit triangle.
2. **Stamp size.** The amount of paint scales with spot area and distance, and stamps are spaced along fast strokes.
3. **Deposit.** `PaintableSurface` draws the stamp with `PaintStamp.shader` into a scratch texture, only inside the stamp's rectangle, then copies it back.
4. **Display.** `PaintedSurface.shader` shows coverage as `1 - exp(-volume)` over the primer, with URP lighting.

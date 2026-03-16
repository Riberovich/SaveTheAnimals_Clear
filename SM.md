# 🐾 Save The Animals --- GDD (AI-Agent Brief)

**Platform:** Mobile (iOS/Android)\
**Engine:** Unity 2022 LTS\
**Format:** 2D, supports Portrait + Landscape (if possible)\
**Audience:** Kids 3--6\
**Design goal:** Maximum delight, zero pressure

> Source concept expanded for quality + long-term fun.

------------------------------------------------------------------------

## 1) High Concept

A joyful tap-only game where kids rescue cute animals floating in the
sky. Each tap pops a balloon, lowering the animal until it lands safely.
The game grows fun through *variety*, *collecting*, and *celebration*,
not difficulty.

------------------------------------------------------------------------

## 2) Design Pillars

### 2.1 Emotional Safety

-   No fail states
-   No timers
-   No punishment
-   No scary visuals/audio
-   Mistakes are impossible

**✅ DONE checklist** - \[ \] No fail state exists in code - \[ \] No
red "error" UI / warning tones - \[ \] No negative SFX / harsh
stingers - \[ \] Villain never interferes during gameplay

### 2.2 Tactile Satisfaction ("Juice")

Every tap should feel squishy, instant, and rewarding.

**✅ DONE checklist** - \[ \] Balloon scale-punch animation - \[ \] Pop
particles (Canvas-friendly) - \[ \] 8+ pop SFX; randomized - \[ \]
Subtle screen shake (optional, very light) - \[ \] Balloon
fragments/sparkles (simple)

### 2.3 Visible Progress

Kids see progress through: - New animals - New worlds/biomes - New
balloon types - Sticker book collection - Park growth (meta)

------------------------------------------------------------------------

## 3) Core Loops

### 3.1 Micro Loop (10--25 sec)

1.  Animal floats with balloons\
2.  Tap balloon → pop\
3.  Animal reacts (blink/wiggle/smile)\
4.  Animal descends slightly\
5.  Repeat until last balloon\
6.  Final pop → landing bounce + squish\
7.  Celebration burst + reward\
8.  Continue → next animal

### 3.2 Meso Loop (3--5 min session)

-   Save 3--5 animals
-   Fill a progress bar toward next unlock
-   Earn stickers + small celebrations

### 3.3 Macro Loop (Long-term)

Save animals → Collect stickers → Unlock biomes → Discover balloon
variety → Unlock rare "golden" animals → Grow "Animal Park"

------------------------------------------------------------------------

## 4) Gameplay Systems

### 4.1 Balloon Variety (fun growth, no stress)

Unlock new balloon types over time; each is a *theme* change, not
difficulty.

  Balloon Type   Behavior                 Reward Feel
  -------------- ------------------------ ----------------
  Normal         1 tap pop                baseline
  Glitter        extra confetti           visual delight
  Rainbow        big color burst          high joy
  Musical        plays a note             playful audio
  Bubble         bubbles after pop        extra VFX
  Giant          2 taps (clearly shown)   novelty

**Rules** - Never speeds up gameplay - Never requires fast reactions -
Never introduces failure

**✅ DONE checklist** - \[ \] Balloon type randomizer (weighted by
progression) - \[ \] Each type has unique VFX + SFX - \[ \] Giant
balloon shows "crack" state after tap 1

### 4.2 Animal Reaction System

Animals have: - Idle animations (float/breathe/blink) - Pop reactions
(surprised/smile/wave) - Landing celebrations (dance/run/clap)

**✅ DONE checklist** - \[ \] 3 idle animations - \[ \] 3 pop
reactions - \[ \] 3 landing animations - \[ \] Random picker with
cooldowns

------------------------------------------------------------------------

## 5) Worlds / Biomes

Each biome changes: - Background + ground - Balloon palette - Music -
Animal pool

**Example biomes** 1. Sunny Meadow\
2. Beach\
3. Candy Land\
4. Snow Hills\
5. Night Sky\
6. Jungle\
7. Space Clouds (safe, magical)

**✅ DONE checklist** - \[ \] Unique background per biome - \[ \] Unique
balloon skins/palette per biome - \[ \] Unique ambient music per biome -
\[ \] Unlock splash screen for new biome

------------------------------------------------------------------------

## 6) Meta: Animal Park (light, no management)

Saved animals appear in a "Happy Animal Park". Kids can tap animals in
the park for cute reactions.

**No economy management. No failure.**

**✅ DONE checklist** - \[ \] Park scene with simple wandering - \[ \]
Saved animals spawn & persist - \[ \] Tap reaction in park (sound +
animation) - \[ \] Park grows cosmetically with milestones

------------------------------------------------------------------------

## 7) Rewards & Collection

### 7.1 Sticker Book

Each animal unlocks a sticker. The book is large, visual, and
scrollable.

### 7.2 Celebration Moments

-   Every rescue: small celebration
-   Every 5 rescues: mini fireworks
-   Every 20 rescues: big unlock
-   Rare: golden animal (very low chance, purely positive)

------------------------------------------------------------------------

## 8) Screens & Flow

1.  **Intro Comic (optional/skip)**: silly villain balloon-launches
    animals (non-threatening)\
2.  **Main Menu**: Play, Park, Sticker Book, Settings, Parent Gate\
3.  **Animal Select** (optional): choose unlocked animals\
4.  **Gameplay**: tap balloons, rescue, reward\
5.  **Celebration**: short reward moment, auto-continue\
6.  **Park**: view/tap collected animals

**✅ DONE checklist** - \[ \] No text-dependent UX - \[ \] Big buttons,
minimal UI clutter - \[ \] Safe area support + both orientations

------------------------------------------------------------------------

## 9) Monetization (child-safe)

-   **Interstitial ads** only between sessions (never mid-level)
-   **Rewarded ad** optional: "Surprise Balloon" / "Bonus Sticker"
-   **IAP**: No Ads, Unlock All Animals, Cosmetic balloon packs\
-   **Parent Gate** required for any purchase/ad settings

**✅ DONE checklist** - \[ \] No mid-gameplay ads - \[ \] Parent gate
(hold + simple math) - \[ \] No dark patterns

------------------------------------------------------------------------

## 10) Audio

-   Soft ambient per biome
-   Pop SFX library (8+)
-   Cute animal voice blips
-   Landing thump + celebration stinger

------------------------------------------------------------------------

## 11) Technical Architecture (for Unity)

**Data** - `AnimalDef` (ScriptableObject): id, sprite/anim set, sounds,
rarity, biome tags\
- `BiomeDef` (ScriptableObject): bg, ground, music, balloon skins,
animal pool\
- `BalloonDef` (ScriptableObject): visuals, tapsToPop, VFX/SFX

**Core Systems** - `GameFlowController` (state machine: Menu → Select →
Play → Reward → Park) - `BalloonManager` (spawn, tap, pop, pooling) -
`AnimalController` (float/descend/land + reactions) -
`ProgressionManager` (unlocks, milestones, persistence) -
`RewardManager` (stickers, celebration) - `AudioManager` (mixing,
randomization) - `OrientationLayout` (portrait/landscape safe layouts)

**✅ DONE checklist** - \[ \] Pooling for balloons & VFX - \[ \]
ScriptableObject-driven content - \[ \] Single input system (tap) - \[
\] Persistence (PlayerPrefs/JSON)

------------------------------------------------------------------------

## 12) Scope Tiers

### MVP

-   1 biome, 3 animals, 2 balloon types, gameplay + reward

### Soft Launch

-   2 biomes, 10 animals, sticker book, basic park

### Full Launch

-   6+ biomes, 30+ animals, park growth, rare system, balloon variety

------------------------------------------------------------------------

# AI-Agent Execution Pipeline (Local Repo + Multi-Agent Workflow)

This is a practical pipeline for an AI coding agent to produce the game
inside a Git repo in small safe patches.

## A) Repo Layout (recommended)

    SaveTheAnimals/
      UnityProject/              # Unity root (Assets/, Packages/, ProjectSettings/)
      Docs/
        GDD_SaveTheAnimals.md
        Pipeline_AI_Agent.md
        StyleGuide.md
      ArtPlaceholders/
      Tools/
      .github/
        workflows/               # optional CI (lint/build)
      README.md

**✅ DONE checklist** - \[ \] Unity project inside repo - \[ \]
`.gitignore` for Unity - \[ \] One source of truth docs in `/Docs`

## B) Agent Roles (simple, effective)

1.  **ProducerAgent**
    -   Maintains task list & scope guardrails
2.  **GameplayAgent**
    -   Implements tap → pop → descend → land loop
3.  **UIAgent**
    -   Screens, navigation, safe area, orientation layouts
4.  **ContentAgent**
    -   ScriptableObjects, unlock tables, placeholder assets
5.  **QAAgent**
    -   Playmode checks, regression notes, build sanity

> If using only 1 agent, it runs these roles sequentially.

## C) Patch Rules (avoid chaos)

-   1 patch = 5--15 minutes of work\
-   Patch must include:
    -   what changed
    -   how to test
    -   rollback info (optional)
-   Never do large refactors unless requested.

**✅ DONE checklist** - \[ \] Each patch has a short changelog - \[ \]
Each patch includes "How to test in Unity" - \[ \] No patch breaks play
mode

## D) Task Slicing (example)

**Milestone M1: Core Fun** 
- A1: Tap balloon → pop animation + random
SFX 
- A2: Pop VFX (Canvas-compatible) - A3: Animal descend step per
pop 
- A4: Final land bounce + dust 
- A4.1: Animal Hanging Idle Animation
Subtle trembling/shivering motion while suspended by balloons Occasional
eye blink (low frequency, randomized interval) Fully procedural (no
Animator required for MVP) 
- A4.2: Pop Reaction Blink On every balloon
pop, animal briefly blinks/squeezes eyes Uses alternate eye sprite (swap
image, short duration, restore) Must not interrupt descend logic 
- A4.3:Landing Dust Effect Spawn dust VFX at ground contact position
Canvas-compatible particle system Trigger only on final balloon pop 
- A4.4: Post-Landing Celebration Animation Starts 1 second after landing
Procedural animation only (no Animator required) Includes: Soft bounce
(vertical movement) Squash & stretch (scale animation) Upper body sway
left/right (subtle rotation or anchored offset)
- A4.5: Ground-Based Parallax System

Replaced background movement with GroundLayer-driven movement.
GroundLayer acts as 100% depth reference.
Additional layers move proportionally for parallax depth.
Sky remains static.
Final pop snaps ground before landing begins.

- A4.6: GroundAnchor Landing System

Landing Y is no longer hardcoded.
Landing position derived from GroundAnchor inside GroundLayer.
Uses World → Screen → Local conversion for Canvas safety.
Includes fallback to legacy groundY.

- A4.7: Dynamic Ground Depth (Balloon Count Driven)

Ground start depth calculated as:

startY = groundFinalY - groundStepPerBalloon * (total - 1)

Allows per-level balloon count (expected max ≈ 12).
Ensures consistent final landing position.

- A4.8: Landing Shadow System

Procedural shadow appears on landing.
Shadow fades in and scales from 0.6 → 1.0.
Fully tunable parameters.
Independent CanvasGroup.

- A4.9: Resolution Independent Layout

Canvas Scaler switched to:
Scale With Screen Size
Reference 1080x1920
Match 0.5

Ensures consistent landing and parallax across devices.
- A5: Simple reward screen → Next 

**Milestone M2: Progression** 
- B1: Unlock animals by saved count 
- B2: Biome switching - B3: Sticker book

**Milestone M3: Park** - C1: Park scene - C2: Spawn collected animals -
C3: Tap reactions in park

## E) Standard Prompts for the AI Agent

### 1) "Implement Patch" Prompt (template)

-   Goal: `<single feature>`{=html}
-   Constraints: Unity 2022 LTS, 2D, tap-only, no fail states
-   Files allowed to modify: `<list>`{=html}
-   Definition of done: `<bullets>`{=html}
-   Output:
    1)  list changed files
    2)  code diff or full files
    3)  how to test

### 2) "QA Pass" Prompt (template)

-   Open Unity, press Play, verify:
    -   balloon pops
    -   no null refs
    -   animations play
    -   reward triggers
-   Provide a checklist + any fixes

## F) Guardrails (important)

-   No online dependencies unless approved
-   Keep everything deterministic & kid-safe
-   Avoid heavy shaders / expensive VFX on mobile
-   Use pooling for particles/balloons
-   Keep UI huge and readable

**✅ DONE checklist** - \[ \] Pooling in place - \[ \] No per-frame
allocations in hot paths - \[ \] No ad SDK in MVP branch - \[ \] Parent
gate requirement documented

## G) CI (optional but helpful)

-   Unity Test Runner playmode tests
-   Build check (Android) on main branch

------------------------------------------------------------------------

## "Definition of Done" (global)

-   Play button → gameplay starts
-   Tap balloon pops with sound + VFX
-   Animal descends each pop
-   Final pop triggers landing + celebration
-   Progress saved between runs
-   No fail states anywhere

**✅ DONE checklist** - \[ \] Full loop playable end-to-end - \[ \] No
errors in Console in Play Mode - \[ \] Runs in portrait + landscape
without broken UI - \[ \] Works on device (Android build) at 60 fps
target

  ----------------
  \# 🔧
  IMPLEMENTATION
  UPDATE (Lean
  Solo Dev Mode)

  This section
  reflects real
  implemented
  systems beyond
  the original
  draft.
  Documentation is
  kept minimal and
  practical.
  ----------------

## 🏗 Ground & Parallax Architecture (Gameplay + Technical)

### Core Principle

GroundLayer is the 100% movement reference.\
Background no longer moves.

### Layer Structure

BackgroundRig ├── SkyLayer (0%) ├── FarLayer (\~0.25x) ├── MidLayer
(\~0.6x) ├── GroundLayer (1.0x reference) └── ForegroundLayer (\~1.15x)

### Behavior

-   GroundLayer moves UP per balloon pop
-   Other layers follow proportionally
-   Sky remains static
-   Final pop snaps ground before landing
-   Supports future biome swap system

### Ground Start Depth

startY = groundFinalY - groundStepPerBalloon \* (total - 1)

Allows per-level balloon count flexibility\
(Level designer controlled, expected max ≈ 12 balloons)

------------------------------------------------------------------------

## 🎯 Landing FX (Updated)

Landing now includes:

1.  Ease-out fall
2.  Squish (scale X/Y)
3.  Bounce up
4.  Ease-in return
5.  Final scale normalization

### Ground Anchor System

-   Landing Y derived from GroundAnchor
-   Converted via World → Screen → Local space
-   Canvas-safe across resolutions
-   Fallback to legacy groundY if anchor missing

------------------------------------------------------------------------

## 🌑 Landing Shadow System

Procedural shadow appears only on landing.

Behavior: - Hidden while floating - Fades in on landing - Scales from
0.6 → 1.0 - Independent CanvasGroup - Fully tunable parameters

------------------------------------------------------------------------

## 👁 Idle & Pop Reaction Stability

-   Procedural idle tremble + blink
-   Pop reaction sprite swap
-   Coroutine-safe (no conflicts)
-   Does not override landing sprite

------------------------------------------------------------------------

## 📱 Canvas Scaler (Critical Fix)

Now using:

UI Scale Mode: Scale With Screen Size\
Reference Resolution: 1080x1920\
Match: 0.5

Ensures: - Stable parallax - Stable landing - Device-independent layout

------------------------------------------------------------------------

# 📌 Lean Changelog

v0.2 - Replaced background movement with ground-based parallax -
Implemented GroundAnchor landing system - Added procedural landing
shadow - Refined landing animation (4-stage) - Added multi-layer
parallax stack - Converted Canvas Scaler to resolution-safe mode

------------------------------------------------------------------------

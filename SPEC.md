# 🐾 Save The Animals — GDD (AI-Agent Brief)

**Platform:** Mobile (iOS/Android)  
**Engine:** Unity 2022 LTS  
**Format:** 2D, supports Portrait + Landscape (if possible)  
**Audience:** Kids 3–6  
**Design goal:** Maximum delight, zero pressure

> Source concept expanded for quality + long-term fun.

---

## 1) High Concept

A joyful tap-only game where kids rescue cute animals floating in the sky. Each tap pops a balloon, lowering the animal until it lands safely. The game grows fun through *variety*, *collecting*, and *celebration*, not difficulty.

---

## 2) Design Pillars

### 2.1 Emotional Safety
- No fail states
- No timers
- No punishment
- No scary visuals/audio
- Mistakes are impossible

**✅ DONE checklist**
- [ ] No fail state exists in code
- [ ] No red “error” UI / warning tones
- [ ] No negative SFX / harsh stingers
- [ ] Villain never interferes during gameplay

### 2.2 Tactile Satisfaction (“Juice”)
Every tap should feel squishy, instant, and rewarding.

**✅ DONE checklist**
- [ ] Balloon scale-punch animation
- [ ] Pop particles (Canvas-friendly)
- [ ] 8+ pop SFX; randomized
- [ ] Subtle screen shake (optional, very light)
- [ ] Balloon fragments/sparkles (simple)

### 2.3 Visible Progress
Kids see progress through:
- New animals
- New worlds/biomes
- New balloon types
- Sticker book collection
- Park growth (meta)

---

## 3) Core Loops

### 3.1 Micro Loop (10–25 sec)
1. Animal floats with balloons  
2. Tap balloon → pop  
3. Animal reacts (blink/wiggle/smile)  
4. Animal descends slightly  
5. Repeat until last balloon  
6. Final pop → landing bounce + squish  
7. Celebration burst + reward  
8. Continue → next animal

### 3.2 Meso Loop (3–5 min session)
- Save 3–5 animals
- Fill a progress bar toward next unlock
- Earn stickers + small celebrations

### 3.3 Macro Loop (Long-term)
Save animals → Collect stickers → Unlock biomes → Discover balloon variety → Unlock rare “golden” animals → Grow “Animal Park”

---

## 4) Gameplay Systems

### 4.1 Balloon Variety (fun growth, no stress)
Unlock new balloon types over time; each is a *theme* change, not difficulty.

| Balloon Type | Behavior | Reward Feel |
|---|---|---|
| Normal | 1 tap pop | baseline |
| Glitter | extra confetti | visual delight |
| Rainbow | big color burst | high joy |
| Musical | plays a note | playful audio |
| Bubble | bubbles after pop | extra VFX |
| Giant | 2 taps (clearly shown) | novelty |

**Rules**
- Never speeds up gameplay
- Never requires fast reactions
- Never introduces failure

**✅ DONE checklist**
- [ ] Balloon type randomizer (weighted by progression)
- [ ] Each type has unique VFX + SFX
- [ ] Giant balloon shows “crack” state after tap 1

### 4.2 Animal Reaction System
Animals have:
- Idle animations (float/breathe/blink)
- Pop reactions (surprised/smile/wave)
- Landing celebrations (dance/run/clap)

**✅ DONE checklist**
- [ ] 3 idle animations
- [ ] 3 pop reactions
- [ ] 3 landing animations
- [ ] Random picker with cooldowns

---

## 5) Worlds / Biomes

Each biome changes:
- Background + ground
- Balloon palette
- Music
- Animal pool

**Example biomes**
1. Sunny Meadow  
2. Beach  
3. Candy Land  
4. Snow Hills  
5. Night Sky  
6. Jungle  
7. Space Clouds (safe, magical)

**✅ DONE checklist**
- [ ] Unique background per biome
- [ ] Unique balloon skins/palette per biome
- [ ] Unique ambient music per biome
- [ ] Unlock splash screen for new biome

---

## 6) Meta: Animal Park (light, no management)

Saved animals appear in a “Happy Animal Park”.
Kids can tap animals in the park for cute reactions.

**No economy management. No failure.**

**✅ DONE checklist**
- [ ] Park scene with simple wandering
- [ ] Saved animals spawn & persist
- [ ] Tap reaction in park (sound + animation)
- [ ] Park grows cosmetically with milestones

---

## 7) Rewards & Collection

### 7.1 Sticker Book
Each animal unlocks a sticker. The book is large, visual, and scrollable.

### 7.2 Celebration Moments
- Every rescue: small celebration
- Every 5 rescues: mini fireworks
- Every 20 rescues: big unlock
- Rare: golden animal (very low chance, purely positive)

---

## 8) Screens & Flow

1. **Intro Comic (optional/skip)**: silly villain balloon-launches animals (non-threatening)  
2. **Main Menu**: Play, Park, Sticker Book, Settings, Parent Gate  
3. **Animal Select** (optional): choose unlocked animals  
4. **Gameplay**: tap balloons, rescue, reward  
5. **Celebration**: short reward moment, auto-continue  
6. **Park**: view/tap collected animals

**✅ DONE checklist**
- [ ] No text-dependent UX
- [ ] Big buttons, minimal UI clutter
- [ ] Safe area support + both orientations

---

## 9) Monetization (child-safe)

- **Interstitial ads** only between sessions (never mid-level)
- **Rewarded ad** optional: “Surprise Balloon” / “Bonus Sticker”
- **IAP**: No Ads, Unlock All Animals, Cosmetic balloon packs  
- **Parent Gate** required for any purchase/ad settings

**✅ DONE checklist**
- [ ] No mid-gameplay ads
- [ ] Parent gate (hold + simple math)
- [ ] No dark patterns

---

## 10) Audio

- Soft ambient per biome
- Pop SFX library (8+)
- Cute animal voice blips
- Landing thump + celebration stinger

---

## 11) Technical Architecture (for Unity)

**Data**
- `AnimalDef` (ScriptableObject): id, sprite/anim set, sounds, rarity, biome tags  
- `BiomeDef` (ScriptableObject): bg, ground, music, balloon skins, animal pool  
- `BalloonDef` (ScriptableObject): visuals, tapsToPop, VFX/SFX

**Core Systems**
- `GameFlowController` (state machine: Menu → Select → Play → Reward → Park)
- `BalloonManager` (spawn, tap, pop, pooling)
- `AnimalController` (float/descend/land + reactions)
- `ProgressionManager` (unlocks, milestones, persistence)
- `RewardManager` (stickers, celebration)
- `AudioManager` (mixing, randomization)
- `OrientationLayout` (portrait/landscape safe layouts)

**✅ DONE checklist**
- [ ] Pooling for balloons & VFX
- [ ] ScriptableObject-driven content
- [ ] Single input system (tap)
- [ ] Persistence (PlayerPrefs/JSON)

---

## 12) Scope Tiers

### MVP
- 1 biome, 3 animals, 2 balloon types, gameplay + reward

### Soft Launch
- 2 biomes, 10 animals, sticker book, basic park

### Full Launch
- 6+ biomes, 30+ animals, park growth, rare system, balloon variety

---

# AI-Agent Execution Pipeline (Local Repo + Multi-Agent Workflow)

This is a practical pipeline for an AI coding agent to produce the game inside a Git repo in small safe patches.

## A) Repo Layout (recommended)

```
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
```

**✅ DONE checklist**
- [ ] Unity project inside repo
- [ ] `.gitignore` for Unity
- [ ] One source of truth docs in `/Docs`

## B) Agent Roles (simple, effective)

1. **ProducerAgent**  
   - Maintains task list & scope guardrails
2. **GameplayAgent**  
   - Implements tap → pop → descend → land loop
3. **UIAgent**  
   - Screens, navigation, safe area, orientation layouts
4. **ContentAgent**  
   - ScriptableObjects, unlock tables, placeholder assets
5. **QAAgent**  
   - Playmode checks, regression notes, build sanity

> If using only 1 agent, it runs these roles sequentially.

## C) Patch Rules (avoid chaos)

- 1 patch = 5–15 minutes of work  
- Patch must include:
  - what changed
  - how to test
  - rollback info (optional)
- Never do large refactors unless requested.

**✅ DONE checklist**
- [ ] Each patch has a short changelog
- [ ] Each patch includes “How to test in Unity”
- [ ] No patch breaks play mode

## D) Task Slicing (example)

**Milestone M1: Core Fun**
- A1: Tap balloon → pop animation + random SFX
- A2: Pop VFX (Canvas-compatible)
- A3: Animal descend step per pop
- A4: Final land bounce + dust
- A5: Simple reward screen → Next

**Milestone M2: Progression**
- B1: Unlock animals by saved count
- B2: Biome switching
- B3: Sticker book

**Milestone M3: Park**
- C1: Park scene
- C2: Spawn collected animals
- C3: Tap reactions in park

## E) Standard Prompts for the AI Agent

### 1) “Implement Patch” Prompt (template)
- Goal: <single feature>
- Constraints: Unity 2022 LTS, 2D, tap-only, no fail states
- Files allowed to modify: <list>
- Definition of done: <bullets>
- Output:
  1) list changed files
  2) code diff or full files
  3) how to test

### 2) “QA Pass” Prompt (template)
- Open Unity, press Play, verify:
  - balloon pops
  - no null refs
  - animations play
  - reward triggers
- Provide a checklist + any fixes

## F) Guardrails (important)

- No online dependencies unless approved
- Keep everything deterministic & kid-safe
- Avoid heavy shaders / expensive VFX on mobile
- Use pooling for particles/balloons
- Keep UI huge and readable

**✅ DONE checklist**
- [ ] Pooling in place
- [ ] No per-frame allocations in hot paths
- [ ] No ad SDK in MVP branch
- [ ] Parent gate requirement documented

## G) CI (optional but helpful)

- Unity Test Runner playmode tests
- Build check (Android) on main branch

---

## “Definition of Done” (global)

- Play button → gameplay starts
- Tap balloon pops with sound + VFX
- Animal descends each pop
- Final pop triggers landing + celebration
- Progress saved between runs
- No fail states anywhere

**✅ DONE checklist**
- [ ] Full loop playable end-to-end
- [ ] No errors in Console in Play Mode
- [ ] Runs in portrait + landscape without broken UI
- [ ] Works on device (Android build) at 60 fps target

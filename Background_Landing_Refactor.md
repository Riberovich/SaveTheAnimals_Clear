# 🐾 Save The Animals

# Background & Landing Refactor --- Technical Design Brief

*(Standalone MD file)*

------------------------------------------------------------------------

## 1. Purpose

This document defines the full refactor of the background, landing
logic, and vertical positioning system to ensure:

-   Stable landing on all screen sizes
-   No visual gaps ("holes") on extreme aspect ratios
-   Correct alignment between animal and ground
-   Full portrait + landscape support
-   Tablet compatibility
-   Safe Area compliance
-   Parallax-ready architecture
-   Biome-ready background system

⚠️ This refactor must NOT break existing: - Balloon pop logic - Descent
logic - Landing FX - Audio - Input - Idle animations

------------------------------------------------------------------------

## 2. Problem Summary

Current issues:

1.  Background scales differently across devices.
2.  Visual gaps appear on some aspect ratios.
3.  Animal sometimes lands "in the air".
4.  Landing is not anchored to a physical ground reference.
5.  Background and gameplay logic are not structurally separated.

Root cause: Landing and positioning are not tied to a stable UI anchor.

------------------------------------------------------------------------

## 3. New Architecture Overview

### 3.1 Layout Structure

    Canvas
     └── SafeAreaRoot
          └── LayoutRoot
               ├── BackgroundRig
               ├── GameplayRig
               └── UIRoot

-   **BackgroundRig** -- All parallax layers
-   **GameplayRig** -- Animal + Balloons (FloatGroup)
-   **UIRoot** -- Buttons, overlays

------------------------------------------------------------------------

## 4. GroundAnchor (Single Source of Truth)

A dedicated RectTransform:

    BackgroundRig
     └── GroundLayer
          └── GroundAnchor

GroundAnchor defines:

-   The exact Y position of landing
-   Final vertical reference for the animal
-   Stable landing across all devices

Landing logic must use:

    groundY = GroundAnchor.anchoredPosition.y

No more hardcoded Y values.

------------------------------------------------------------------------

## 5. Background Layer System

### Layer Order (Back → Front)

1.  Sky (gradient)
2.  Far Mountains
3.  Far Clouds
4.  Near Clouds
5.  Mid Trees
6.  Ground Layer
7.  Foreground Layer

Each layer:

-   Separate RectTransform
-   Stretch or overscan setup
-   Biome-replaceable sprite

------------------------------------------------------------------------

## 6. Overscan Rules (No Gaps)

To prevent holes:

-   Sky → Stretch full screen
-   Ground → Stretch bottom full width
-   Other layers → Slight horizontal overscan (10--20% wider than
    screen)

No camera movement required.

------------------------------------------------------------------------

## 7. Start Height Auto Calculation

Let:

-   `N` = number of balloons
-   `stepPerPop` = descent step
-   `groundY` = GroundAnchor position
-   `landingPadding` = small visual offset

Then:

    startY = groundY + (N - 1) * stepPerPop + landingPadding

On game start:

-   Move GameplayRig (FloatGroup) to `startY`
-   Do NOT move background

This guarantees:

-   Final pop happens near ground
-   Landing always looks intentional

------------------------------------------------------------------------

## 8. Parallax System (Cheap & Optimized)

No physics. No camera.

Parallax is driven by descent progress:

    progress = 1 - (currentHeight / startHeight)

Each layer has a multiplier:

  Layer           Multiplier
  --------------- ------------
  Far Mountains   0.1
  Far Clouds      0.2
  Near Clouds     0.4
  Mid Trees       0.6
  Foreground      0.8

Position shift:

    layerOffset = progress * parallaxAmount * multiplier

Sky does not move.

------------------------------------------------------------------------

## 9. Biome System (Future Ready)

Define a structure:

    BiomeBackgroundSet
    - SkySprite
    - FarMountainsSprite
    - CloudsFarSprite
    - CloudsNearSprite
    - MidTreesSprite
    - GroundSprite
    - ForegroundSprite

Biome swap:

-   Replace sprites only
-   No layout or logic changes

------------------------------------------------------------------------

## 10. Canvas Configuration

Recommended:

-   UI Scale Mode: Scale With Screen Size
-   Reference Resolution: 1080x1920
-   Match: 0.5
-   Ground anchored to bottom
-   SafeAreaFitter applied to SafeAreaRoot

------------------------------------------------------------------------

## 11. Safety Constraints

Must NOT:

-   Remove existing objects
-   Refactor balloon logic
-   Change pop mechanics
-   Change landing FX behavior

Allowed:

-   Add GroundAnchor
-   Add ParallaxController
-   Add BackgroundRig
-   Adjust layout only

------------------------------------------------------------------------

## 12. Edge Cases to Test

-   iPhone tall ratio (19.5:9)
-   4:3 tablet
-   21:9 ultra-wide
-   Rotation during gameplay
-   Devices with notch
-   Very small phones

------------------------------------------------------------------------

## 13. Implementation Roadmap

### Phase 1 -- Stability

1.  Add LayoutRoot
2.  Add GroundAnchor
3.  Rebind landing to GroundAnchor

### Phase 2 -- Height Logic

4.  Implement start height calculation
5.  Remove hardcoded Y values

### Phase 3 -- Background Refactor

6.  Split background into layers
7.  Implement overscan

### Phase 4 -- Visual Depth

8.  Add ParallaxController
9.  Tune multipliers

### Phase 5 -- Biome Ready

10. Create BiomeBackgroundSet
11. Enable sprite swapping

------------------------------------------------------------------------

## 14. Expected Result

✔ Animal always lands on ground\
✔ No background holes\
✔ Stable across devices\
✔ Full orientation support\
✔ Visual depth via parallax\
✔ Biome-ready architecture\
✔ Zero regression in gameplay

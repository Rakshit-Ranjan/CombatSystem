# Current Task

## Milestone

CombatDirector V2

## Goal

Recreate Ghost of Tsushima encounter pacing.

## Principles

- Encounters should feel like repeated 1v1 duels.
- Multiple enemies maintain pressure without overwhelming the player.
- One enemy attacks while others circle, reposition, or wait.
- CombatDirector coordinates encounters, not low-level AI.

## Completed

- EnemyCombatFSM
- EnemyBrain
- EnemyController
- EnemyLocomotion
- Enemy movement animations
- CombatDirector V1
- CombatDirector V2 first pass
- Focus enemy selection
- Slot-angle assignment around the player
- Orbit-style slot movement for non-focus enemies
- Focus-lane avoidance so circling enemies avoid cutting through the active duel

## Current Focus

- Focus release and handoff timing
- Slot fairness / anti-reshuffle polish
- Tuning orbit radius, circling speed, and focus-lane width
- Playtesting 2-4 enemy readability

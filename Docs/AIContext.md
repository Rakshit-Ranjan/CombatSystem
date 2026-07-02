# AI Context

## Project Summary
Unity melee combat prototype with:
- player combo combat
- motion-driven movement
- enemy AI split into perception / brain / controller / combat
- hitbox / hurtbox collision pipeline
- terrain and grass tooling

## Current Architecture
- Player execution lives in `PlayerLocomotionController`, `InputBuffer`, `CombatFSM`, `PlayerHealth`.
- Enemy execution lives in `EnemyPerception`, `EnemyBrain`, `EnemyController`, `EnemyLocomotion`, `EnemyCombatFSM`, `EnemyHealth`.
- ScriptableObjects define attacks, chains, dodges, parries, hit reactions, and motion graphs.
- `Hitbox` emits `AttackContext`; defenders convert that into `DamageData` and reactions.

## Current Milestone
Finish the full 1v1 combat loop with reliable damage, dodge/parry validation, and stable hitbox lifecycle.

## Important Gameplay Systems
- `CombatFSM`: player combat state owner
- `EnemyCombatFSM`: enemy combat state owner
- `MotionGraph` / `MotionGraphSampler`: movement authored as curves
- `Hitbox` / `Hurtbox`: collision bridge
- `PlayerHealth` / `EnemyHealth`: HP owners

## Naming Conventions
- MonoBehaviours use PascalCase class names matching file names.
- Runtime helpers and data structs use PascalCase.
- Serialized private fields use camelCase.
- Enums live in shared utility files when reused across systems.

## Design Rules
- Do not merge AI decision logic into combat execution.
- Do not move hit validation into `Hitbox`.
- Keep health separate from combat-state ownership.
- Prefer updating existing docs instead of adding ad hoc notes elsewhere.

## MotionGraph Workflow
1. Author cumulative forward/right/up/yaw curves in `MotionGraph`.
2. Reference graph from `AttackData`, `DodgeData`, or `HitReactionData`.
3. Start sampler in owning FSM.
4. Sample every frame using normalized time.
5. Convert local deltas into world movement.

## Combat Philosophy
- Animation-aware state machines drive combat.
- Motion is data-authored.
- Hit detection sends context, not final damage resolution.
- Defender-side systems decide the outcome of a hit.

## AI Philosophy
- Perception senses.
- Brain decides intent.
- Controller routes ownership.
- Locomotion and CombatFSM execute.

## State Ownership
- Player combat state belongs to `CombatFSM`.
- Enemy combat state belongs to `EnemyCombatFSM`.
- Enemy high-level intent belongs to `EnemyBrain`.
- Character movement belongs to locomotion components unless combat temporarily overrides it.

## Component Responsibilities
- `InputBuffer`: combat input queue only
- `PlayerLocomotionController`: movement and locomotion animation driving
- `CombatFSM`: combat execution and reaction states
- `Hitbox`: collision + `AttackContext`
- `PlayerHealth` / `EnemyHealth`: HP only
- `EnemyController`: mediator between intent and execution

## Coding Conventions
- Prefer serialized private fields for designer-tunable runtime values.
- Prefer ScriptableObjects for reusable combat tuning.
- Prefer explicit component ownership over cross-cutting utility logic.
- Use `RequireComponent` when runtime dependencies are structural.

## Things AI Should Never Change
- Do not collapse `EnemyBrain`, `EnemyController`, `EnemyLocomotion`, and `EnemyCombatFSM` into one script.
- Do not move damage-resolution rules into `Hitbox`.
- Do not replace motion-graph-driven movement with hardcoded per-attack movement unless explicitly requested.
- Do not create duplicate docs instead of updating files in `Docs`.

## Current Implementation Status
- Player attacks, combos, dodges, parries, and hit reactions exist.
- Enemy windup, attack execution, and hit reactions exist.
- MotionGraph pipeline exists and is shared.
- Player health now exists and is wired into incoming enemy hits.
- Defensive damage rules and hitbox dedupe still need hardening.

## Three-Minute Mental Model
- Player and enemy both use FSM-driven combat.
- Motion comes from authored curves, not raw code constants.
- AI intent is separate from execution.
- Hitboxes send `AttackContext`; defenders choose what happens.
- The project is currently at “working prototype” stage, not fully hardened production combat.

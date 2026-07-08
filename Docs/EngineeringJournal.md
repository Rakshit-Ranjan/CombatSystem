# Engineering Journal

## 2026-07-03 - Documentation Baseline

### Feature
Created the initial `/Docs` package for the project.

### Problem
Project architecture and combat design knowledge were concentrated in code and conversation history, making onboarding and maintenance expensive.

### Alternatives
- Keep ad hoc notes in chat only
- Write one large README
- Create subsystem-specific living docs

### Decision
Create a maintainable `/Docs` folder with architecture, combat, AI, roadmap, AI context, code style, and feature tracking.

### Reason
The project already has enough moving parts that architecture must be captured outside the scripts themselves.

### Tradeoffs
- Requires ongoing maintenance
- Documentation can drift if not updated during feature work

### Future Improvements
- Add date-stamped entries after each major combat or AI milestone
- Record major bug fixes and system rewrites here

## EnemyBrain

### Problem
Enemy AI decision logic needed to stay separate from movement and combat execution.

### Alternatives
- Put AI inside `EnemyCombatFSM`
- Put AI inside `EnemyController`
- Separate perception and decision logic

### Decision
Use `EnemyPerception` + `EnemyBrain` + `EnemyController`.

### Reason
Keeps sensing, decision-making, and execution independently evolvable.

### Tradeoffs
- Adds one more mediator component
- Requires clear ownership rules

### Future Improvements
- Utility AI scoring
- Better intent evaluation cadence
- Group coordination

## MotionGraph Pipeline

### Problem
Attacks, dodges, and hit reactions need designer-tunable displacement without tying motion entirely to animator root motion.

### Alternatives
- Hardcoded translation per state
- Animator root motion only
- Curve-driven movement assets

### Decision
Use `MotionGraph` ScriptableObjects sampled through `MotionGraphSampler`.

### Reason
Gives reusable, tuneable, data-authored motion across combat systems.

### Tradeoffs
- Requires disciplined asset setup
- Runtime math must stay consistent with animation timing

### Future Improvements
- Validation tooling for missing/invalid graphs
- Debug visualization of sampled motion

## Hitbox / Hurtbox Context Model

### Problem
The project needs defender-aware combat outcomes such as dodge/parry/block rather than attacker-authoritative direct damage.

### Alternatives
- Hitbox directly applies damage
- Hitbox sends a lightweight context and defender resolves outcome

### Decision
Use `Hitbox` -> `AttackContext` -> `IAttackReciever`.

### Reason
Defender-side resolution supports richer combat rules and cleaner state ownership.

### Tradeoffs
- More systems participate in one hit
- Easier to misconfigure ownership if inspector references are wrong

### Future Improvements
- Harden hitbox ownership assignment
- Add validation/debug views for attack direction and active frames

## Player Health

### Problem
Enemy hits needed to affect a player-owned HP system rather than only triggering reaction states.

### Alternatives
- Store health inside `CombatFSM`
- Create a separate `PlayerHealth` implementing `IDamageable`

### Decision
Use separate `PlayerHealth`.

### Reason
Keeps numeric health concerns separate from combat-state concerns.

### Tradeoffs
- Requires an extra component reference
- Damage and reaction pipelines must stay synchronized

### Future Improvements
- UI events
- death handling
- healing sources

## 2026-07-04 - Defensive Resolution Pass

### Feature
Added defender-side dodge/parry branching, enemy parry stun entry, and separated hit reaction lock duration from hit reaction movement timing.

### Problem
Hit reactions previously used one duration value for both total stun time and reaction movement, and defensive outcomes were not yet branching cleanly inside incoming-hit resolution.

### Alternatives
- Keep one reaction timer for everything
- Resolve parry success only on the player and leave enemy reaction external
- Split reaction timing and let the defender push the attacker into stun on parry success

### Decision
Use defender-side hit resolution for dodge/parry outcomes, add an explicit enemy `EnterParryStun(...)` path, and author a separate reaction-force timing value in `HitReactionData`.

### Reason
This keeps the defender authoritative over incoming-hit outcomes while allowing hit reaction movement to end before the full stun lock does.

### Tradeoffs
- `HitReactionData` authoring is now easier to misconfigure
- Parry behavior still needs more polish and validation coverage

### Future Improvements
- Add gizmos or debug UI for dodge/parry timing windows
- Validate reaction timing data in editor/runtime
- Add direction-aware enemy parry-stun variants

## 2026-07-09 - Combat Director Pass

### Feature
Added a first-pass `CombatDirector` singleton to gate concurrent enemy attacks in multi-enemy encounters.

### Decision
Keep group attack permission outside `EnemyCombatFSM` and use the director as the shared coordination layer.

### Current Limitation
Denied attackers currently stop rather than reposition or circle.

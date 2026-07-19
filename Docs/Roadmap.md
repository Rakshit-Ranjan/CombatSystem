# Roadmap

## Completed Systems
- Player locomotion controller
- Input buffering
- Player attack combo pipeline
- MotionGraph authoring and sampling
- Player and enemy hitbox / hurtbox pipeline
- Enemy perception / brain / controller / locomotion split
- Enemy windup and attack execution
- CombatDirector focus enemy and orbit-slot repositioning
- Player and enemy hit reaction selection by hurtbox + direction
- Basic player and enemy health components
- Terrain / grass utility tooling

## Current Milestone
Polish CombatDirector V2 so multi-enemy encounters preserve a readable duel-like focus.

## Next Milestone
Defensive combat validation:
- dodge i-frame rules
- parry validation and enemy-side parry stun polish
- perfect parry / perfect dodge windows
- stable one-hit-per-swing lifecycle
- focus release, handoff timing, and slot fairness on top of the current combat director

## Future Features
- Enemy combo chains and attack variety
- Better player death / enemy death handling
- Block / guard rules
- Poise / stagger systems
- More AI behaviors (group behavior, director heuristics, better repositioning)
- Stance or enemy-type-specific combat responses

## Known Technical Debt
- Some combat state exits still depend on animation-event correctness
- Duration math assumes valid clips/data in some paths
- `Hitbox` ownership is inspector-driven and easy to misconfigure
- Reaction force timing now depends on valid `HitReactionData` authoring

## Known Bugs
- Hitbox target dedupe is still being tuned
- Defensive rules are not fully enforced before damage is applied
- Some combat flows can be brittle if data assets are missing or zero-length

## Stretch Goals
- Combat UI / debug overlays
- Utility AI
- Better animation feedback on parry/dodge success
- Expanded terrain generation and environmental interaction systems

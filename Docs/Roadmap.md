# Roadmap

## Completed Systems
- Player locomotion controller
- Input buffering
- Player attack combo pipeline
- MotionGraph authoring and sampling
- Player and enemy hitbox / hurtbox pipeline
- Enemy perception / brain / controller / locomotion split
- Enemy windup and attack execution
- Player and enemy hit reaction selection by hurtbox + direction
- Basic player and enemy health components
- Terrain / grass utility tooling

## Current Milestone
Close the core 1v1 melee loop so one enemy and one player can:
- attack
- dodge / parry
- take damage once per swing
- react and recover reliably

## Next Milestone
Defensive combat validation:
- dodge i-frame rules
- parry validation and enemy-side parry stun polish
- perfect parry / perfect dodge windows
- stable one-hit-per-swing lifecycle

## Future Features
- Enemy combo chains and attack variety
- Better player death / enemy death handling
- Block / guard rules
- Poise / stagger systems
- More AI behaviors (circling, group behavior, director logic)
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

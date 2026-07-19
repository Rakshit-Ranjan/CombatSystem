# Feature Checklist

## Combat
- [x] Player attack combo system
- [x] MotionGraph-driven attacks
- [x] MotionGraph-driven dodges
- [x] MotionGraph-driven hit reactions
- [x] Hitbox / Hurtbox pipeline
- [x] Enemy attack windup
- [x] Enemy single-attack execution
- [x] Player health component
- [x] Enemy health component
- [x] Enemy parry-stun response path
- [x] Separate hit reaction duration vs movement timing
- [x] Stable one-hit-per-swing lifecycle
- [ ] Full dodge damage validation
- [ ] Full parry damage validation
- [ ] Perfect parry
- [ ] Perfect dodge
- [ ] Block / guard system
- [ ] Enemy combo attacks
- [ ] Poise / stagger rules

## AI
- [x] Perception
- [x] Brain
- [x] Controller
- [x] NavMesh locomotion
- [x] Combat ownership handoff
- [x] Director system
- [x] First-pass circling behavior
- [x] Focus enemy selection
- [x] Orbit-style slot repositioning
- [ ] Attack selection variety
- [ ] Group AI
- [ ] Slot fairness / anti-reshuffle polish

## Movement
- [x] CharacterController player movement
- [x] Camera-relative movement
- [x] Directional dodge basis
- [x] Motion-driven attack movement
- [ ] Climbing
- [ ] Sprint / walk polish in combat transitions

## Reactions
- [x] Hurtbox body regions
- [x] Direction-based hit reaction lookup
- [x] Player stunned state
- [x] Enemy stunned state
- [ ] Robust interruption rules
- [ ] Death reactions

## Systems / Tools
- [x] Input buffer
- [x] AttackChain assets
- [x] MotionGraphSampler
- [x] Terrain generator
- [x] Terrain texture sync utility
- [x] Grass interaction manager
- [x] Grass mesh generator
- [ ] Combat debug tooling
- [ ] Data validation tooling

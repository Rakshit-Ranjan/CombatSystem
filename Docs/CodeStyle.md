# Code Style

## Naming Conventions
- Classes, structs, interfaces, enums: PascalCase
- Interfaces: `I...`
- Serialized private fields: camelCase
- Public properties: PascalCase
- Animation/state enums: PascalCase members

## Folder Organization
- Put runtime combat data assets in `Scriptables`
- Put shared primitives and enums in `Shared` / `Structs` / `Interfaces`
- Put player-only runtime behavior in `Player`
- Put enemy-only runtime behavior in `Enemy`
- Put rendering / terrain support in `Terrain`

## Serialization Rules
- Prefer `[SerializeField] private` for designer-facing runtime values.
- Expose read-only public properties when code needs access.
- Use ScriptableObjects for reusable data, not scene-bound mutable state.

## Access Modifier Guidelines
- Default to `private`.
- Use `public` for explicit API surface or Unity inspector/state exposure that is intentionally public.
- Prefer properties over mutable public fields when exposing runtime state.

## Component Responsibilities
- One component should own one domain.
- Combat FSMs own combat execution.
- Health components own HP only.
- Hitboxes own collision detection only.
- Controllers mediate between systems instead of duplicating subsystem logic.

## When to Use ScriptableObjects
Use ScriptableObjects for:
- attacks
- attack chains
- motion graphs
- dodge tuning
- parry tuning
- hit reaction tuning

Do not use ScriptableObjects for:
- per-instance live state
- transient combat state timers

## State Machine Conventions
- State enums live near the owning FSM.
- Each state should have a dedicated handler method.
- Transition helpers should reset timing cleanly.
- Avoid hidden state changes from unrelated systems.

## MotionGraph Conventions
- Curves are cumulative, not per-frame.
- Sampled output should be converted to frame deltas through `MotionGraphSampler`.
- MotionGraph is the source of truth for combat displacement.

## Interface Conventions
- `IDamageable` receives resolved `DamageData`.
- `IAttackReciever` receives unresolved `AttackContext`.
- Prefer small, purpose-specific interfaces.

## Error Handling Philosophy
- Guard null or empty data where gameplay would otherwise hard-fail.
- Fail loudly in editor/dev via logs when authoring data is invalid.
- Keep runtime ownership assumptions explicit.

## Performance Guidelines
- Prefer cached component references in `Awake`.
- Use evaluation intervals for AI reasoning where real-time evaluation is unnecessary.
- Keep collision-to-resolution data lightweight.
- Use data assets rather than repeated runtime allocations where possible.

## Documentation Rule
- Update files in `Docs` when architecture or feature status changes.
- Do not create throwaway docs for one-off features.

# Multi-Enemy Plan

## Goal
Add support for 2-4 enemies around the player without making combat unreadable or unfair.

This plan is designed to fit the current architecture:
- `EnemyPerception` senses the player
- `EnemyBrain` chooses coarse intent
- `EnemyController` routes movement vs combat ownership
- `EnemyLocomotion` handles chase / reposition movement
- `EnemyCombatFSM` owns attack execution

The main rule is:
keep group logic **outside** `EnemyCombatFSM`.

`EnemyCombatFSM` should still answer:
- am I attacking?
- can I attack now?
- am I stunned / committed?

Group coordination should answer:
- which enemy may attack now?
- where should waiting enemies stand?
- who should circle, chase, or hold?

## High-Level Build Order

1. Multiple enemies can all detect and target the same player.
2. Add a shared combat director so not every enemy attacks at once.
3. Add ring slots around the player so enemies spread out.
4. Add circling behavior for non-attacking enemies.
5. Add fairness rules and polish.

Do not start with circling first.
Circling only becomes meaningful once enemies are coordinated around shared space.

## Phase 1: Make Multiple Enemies Stable

### Objective
Spawn 2-4 enemies and make sure they can all:
- see the player
- chase the player
- stop overlapping too badly
- not break when one attacks and another is nearby

### What should already work
- each enemy has its own `EnemyPerception`
- each enemy has its own `EnemyBrain`
- each enemy has its own `EnemyController`
- each enemy has its own `EnemyCombatFSM`

### What to check before continuing
- two enemies can chase the player at the same time
- getting stunned on one enemy does not affect another
- hitboxes still only hit intended targets
- pathing does not explode when enemies bunch together

### Likely small fixes
- tune `NavMeshAgent.radius`
- tune stopping distance
- tune attack range vs chase distance
- make sure enemies do not constantly toggle between `CHASE` and `ATTACK`

## Phase 2: Add a Combat Director

### Objective
Prevent all enemies from attacking at once.

This is the most important multi-enemy system.
Without it, the fight will feel unfair even if movement is working.

### New system
Create a scene-level manager, for example:
- `EnemyCombatDirector`

Responsibilities:
- track active enemies near the player
- decide how many enemies may attack at once
- assign attack permission
- optionally prioritize the closest or best-positioned enemy

### First simple version
The first version can be extremely small.

It only needs:
- a list of registered enemies
- `maxConcurrentAttackers`
- a way for an enemy to ask: "may I attack now?"

### Recommended first rule
- only `1` enemy may actively attack at a time
- all others may chase or reposition but not start attack windup

This gives you fairness immediately.

### API shape
The director can expose methods like:
- `RegisterEnemy(EnemyController enemy)`
- `UnregisterEnemy(EnemyController enemy)`
- `CanAttack(EnemyController enemy)`
- `NotifyAttackStarted(EnemyController enemy)`
- `NotifyAttackEnded(EnemyController enemy)`

### Integration point
Keep `EnemyBrain` simple.

Do not teach `EnemyBrain` group tactics yet.
Instead:
- `EnemyBrain` can still say `ATTACK`
- `EnemyController` or `EnemyCombatFSM` asks the director whether attacking is allowed

Best first fit for your current structure:
- `EnemyBrain` keeps producing `ATTACK`
- `EnemyController` decides:
  - if attack is allowed -> let combat proceed
  - if attack is not allowed -> keep repositioning instead of hard stop

## Phase 3: Add Ring Slots Around the Player

### Objective
Give waiting enemies intentional places to stand.

Instead of every enemy running directly at the player, assign them positions around a circle.

### New concept
Each enemy gets a desired slot around the player:
- front-left
- front-right
- left
- right
- rear-left
- rear-right

You do not need a perfect formation system at first.

### Simple implementation
The director can generate slot positions using:
- player position
- player forward
- a ring radius
- evenly spaced angles

For example:
- 0 degrees
- 60 degrees
- 120 degrees
- 180 degrees

### Enemy behavior with slots
- attacking enemy tries to move into an attack slot near the front
- non-attacking enemies move toward their assigned slot
- if close enough to their slot, they either hold position or lightly circle

### Why this matters
This solves:
- body pileups
- enemies standing in one line
- unreadable crowding

## Phase 4: Add Circling

### Objective
Make non-attacking enemies feel active instead of frozen.

Circling should be a **waiting behavior**, not the main behavior.

### Rule of thumb
- one enemy attacks
- one or more enemies circle / reposition
- distant enemies chase to join the ring

### Where circling should live
Put circling in locomotion/controller behavior, not in combat FSM.

Good fit:
- `EnemyController` decides that a non-attacking enemy should `Circle`
- `EnemyLocomotion` executes the movement

### First simple circling implementation
Pick a tangent direction around the player:
- clockwise
- counter-clockwise

Compute:
- direction to player
- perpendicular/tangent vector on XZ plane
- move partly tangent, partly inward/outward to maintain ring radius

The result is:
- enemies slide around the player instead of stopping in place

### Recommended control rule
Circle only when:
- enemy can see the player
- enemy wants to attack
- enemy is not the chosen attacker
- enemy is already close to ring distance

If enemy is too far away:
- chase slot first

## Phase 5: Add Fairness Rules

### Objective
Make multi-enemy combat readable instead of chaotic.

### Rules to add
- attack token cooldown after an enemy finishes attacking
- short delay before a new enemy can attack
- prefer enemies already near the front of the player
- avoid back-to-back attacks from the exact same enemy unless intended

### Good first fairness settings
- `maxConcurrentAttackers = 1`
- attack handoff delay: `0.2` to `0.5` seconds
- post-attack personal cooldown: existing `attackTimer`
- maximum active engagement ring: 3-4 nearby enemies

### Why this works
The player can read:
- who is threatening
- who is waiting
- when the next attack is likely coming

## Recommended Architecture Changes

### New files
- `EnemyCombatDirector.cs`
- optionally `EnemyGroupSlot.cs` or a small helper struct for slot data

### Existing files to touch
- `EnemyController.cs`
- `EnemyLocomotion.cs`
- possibly `EnemyBrain.cs`

### Files to avoid overloading
- `EnemyCombatFSM.cs`

Keep `EnemyCombatFSM` focused on:
- windup
- attack execution
- combo continuation
- stun / parry stun

Do not turn it into a group AI manager.

## Detailed Responsibilities

### `EnemyCombatDirector`
- owns enemy registration
- owns attack permission
- owns slot assignment
- owns shared fairness rules

### `EnemyBrain`
Still only decides:
- `IDLE`
- `CHASE`
- `ATTACK`

No need yet for:
- flank intent
- support intent
- retreat intent

Those can come later.

### `EnemyController`
Should become the main coordinator between:
- brain intent
- director permission
- locomotion mode
- combat execution

It can decide between:
- `Stop`
- `ChasePlayer`
- `MoveToSlot`
- `Circle`
- `AllowCombat`

### `EnemyLocomotion`
Should support:
- move toward player
- move toward a slot position
- circle around player
- stop

### `EnemyCombatFSM`
Should expose enough state for the director/controller:
- is attacking
- is stunned
- blocks locomotion
- can begin an attack

If needed later, add a simple property like:
- `public bool IsBusy`

## First Implementation Pass

### Step 1
Create `EnemyCombatDirector` in the scene.

### Step 2
Have each enemy register/unregister itself.

### Step 3
Director tracks nearby enemies and chooses one active attacker.

### Step 4
Update `EnemyController` logic:
- if combat blocks locomotion -> stop
- if brain says `CHASE` -> chase player
- if brain says `ATTACK` and director allows attack -> stop and let combat execute
- if brain says `ATTACK` and director denies attack -> move to slot or circle

### Step 5
Add slot generation around the player.

### Step 6
When denied attack permission:
- move to assigned slot if not in position
- circle if already near slot/ring

### Step 7
When attack starts/ends:
- notify director

## Playtest Checklist

### With 2 enemies
- only one attacks at a time
- second enemy stays active instead of freezing
- both enemies remain readable

### With 3 enemies
- no severe pileup in front of player
- enemies redistribute around the player
- attack turns feel fair

### During interruptions
- stunned attacker releases its attack slot
- another enemy does not attack instantly in the same frame unless intended
- circling enemies do not walk through the player

### During player movement
- enemies keep reforming around the player
- slots do not lag too far behind
- navigation still works on turns and corners

## Common Failure Modes

### All enemies attack anyway
Cause:
- attack permission checked too late

Fix:
- check director permission before entering windup or before starting attack

### Enemies freeze when denied attack
Cause:
- denied attackers are told to stop instead of reposition

Fix:
- give denied attackers a fallback locomotion behavior

### Enemies clump in front
Cause:
- no slot assignment

Fix:
- move denied attackers to ring positions

### Circling looks jittery
Cause:
- slot target changes every frame too aggressively

Fix:
- update slots at a controlled cadence or smooth target movement

### Combat becomes unfair
Cause:
- too many simultaneous threats

Fix:
- keep one attacker at first
- add handoff delay
- keep other enemies in visible waiting motion

## Recommended Milestone Order

### Milestone 1
Two enemies, one attacker token, no circling yet.

Success condition:
- second enemy waits or repositions while first attacks

### Milestone 2
Two to three enemies with slot positions.

Success condition:
- enemies spread around player instead of lining up

### Milestone 3
Add circling for denied attackers.

Success condition:
- waiting enemies feel alive and intentional

### Milestone 4
Tune fairness, cooldowns, and attack handoff.

Success condition:
- encounter feels readable and challenging, not spammy

## Strong Recommendation
Do **not** try to ship all of this in one pass.

Build it like this:
1. one active attacker token
2. slot positioning
3. circling
4. fairness polish

That path gives you useful results after each step and reduces rewrite risk.

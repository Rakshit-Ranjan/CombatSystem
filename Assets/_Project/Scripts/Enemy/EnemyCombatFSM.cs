using System;
using Unity.VisualScripting;
using UnityEngine;


public class EnemyCombatFSM : MonoBehaviour, IAttackReciever {
    
    [SerializeField]private EnemyBrain brain;
    [SerializeField]private EnemyController enemyController;
    [SerializeField]private EnemyHealth health;
    [SerializeField]private EnemyLocomotion locomotion;
    [SerializeField]private HurtboxReactionMap[] hurtboxReactionMaps;
    [SerializeField]private CombatState combatState;
    [SerializeField]private float stateTimer, stunnedStateTimer, stunnedStateOffset;
    [SerializeField]private float timeBetweenAttack, attackTimer, attackFacingThreshold; // every n seconds do an attack
    //ATTACK STATE VARIABLES
    [SerializeField] private AttackData currentAttack;
    [SerializeField]private AttackChain lightAttackChain;
    [SerializeField] private Hitbox weaponHitbox;

    private CharacterController controller;
    private MotionGraphSampler stunnedSampler;
    private MotionGraphSampler attackSampler;
    private Animator animator;
    private Vector3 HitForward, HitRight, HitUp;
    public bool BlocksLocomotion;

    void Awake() {
        health = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        enemyController = GetComponent<EnemyController>();
        locomotion = GetComponent<EnemyLocomotion>();
        brain = GetComponent<EnemyBrain>();
        weaponHitbox = GetComponentInChildren<Hitbox>();
        combatState = CombatState.IDLE;
        stunnedSampler = new MotionGraphSampler();
        attackSampler = new MotionGraphSampler();
    }

    void Update() {
        BlocksLocomotion = combatState == CombatState.STUNNED || combatState == CombatState.ATTACKING || combatState == CombatState.WINDUP;
        stateTimer += Time.deltaTime;
        if(attackTimer > 0) 
            attackTimer -= Time.deltaTime;
        switch(combatState) {
            case CombatState.IDLE:
                HandleIdleState();
                break;
            case CombatState.STUNNED:
                HandleStunnedState();
                break;
            case CombatState.WINDUP:
                HandleWindupState();
                break;
            case CombatState.ATTACKING:
                HandleAttackingState();
                break;
        }
    }


    /// <summary>
    /// handling states
    /// </summary>
    private void HandleIdleState() {
        //first check if enemy intent is in attack
        //attack every 3 seconds
        if(attackTimer <= 0f && brain.CurrentIntent == EnemyIntent.ATTACK) {
            TransitionTo(CombatState.WINDUP);
        }
    }

    private void HandleWindupState() {
        //ABORTING ATTACK
        if(brain.CurrentIntent != EnemyIntent.ATTACK) {
            BlocksLocomotion = false;
            TransitionTo(CombatState.IDLE);
            return;
        }
        //ROTATE TOWARDS PLAYER
        Vector3 toPlayer = enemyController.playerT.position - transform.position;
        toPlayer.y = 0;
        if(toPlayer.magnitude < 0.001f) return;
        Quaternion lookRotation = Quaternion.LookRotation(toPlayer.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            lookRotation,
            locomotion.rotationSpeed * Time.deltaTime
        );


        //STARTING ATTACK
        if(Vector3.Angle(transform.forward, toPlayer.normalized) < attackFacingThreshold) {
            StartAttack();
        }

    }

    private void HandleAttackingState() {

        if(currentAttack == null) {
            TransitionTo(CombatState.IDLE);
            return;
        }

        float normalizedTime = stateTimer/(currentAttack.GetDuration());
        (Vector3 localDelta, float deltaYaw) = attackSampler.Sample(normalizedTime);
        transform.Rotate(0f, deltaYaw, 0f);
        Vector3 worldDelta = transform.forward * localDelta.z + 
                             transform.right * localDelta.x + 
                             transform.up * localDelta.y;
        controller.Move(worldDelta);

        if(stateTimer > currentAttack.GetDuration()) {
            attackTimer = timeBetweenAttack;
            attackSampler.Reset();
            currentAttack = null;
            BlocksLocomotion = false;
            TransitionTo(CombatState.IDLE);
        }

    }

    private void HandleStunnedState() {
        float normalizedTime = stateTimer/(stunnedStateTimer - stunnedStateOffset);
        (Vector3 localDelta, float deltaYaw) = stunnedSampler.Sample(normalizedTime);
        Vector3 worldDelta = HitForward * localDelta.z + HitRight * localDelta.x + HitUp * localDelta.y;
        controller.Move(worldDelta);
        
        if(stateTimer >= stunnedStateTimer) {
            stunnedSampler.Reset();
            TransitionTo(CombatState.IDLE);
        }
    }

    public void TransitionTo(CombatState state) {
        stateTimer = 0f;
        combatState = state;
    }

    /// <summary>
    /// handling incoming attacks
    /// create dmgdata --> hitrxtdata
    /// </summary>
    /// <param name="ctx"></param>
    public void OnIncomingAttack(AttackContext ctx) {
        DamageData data = new DamageData {
            attacker = ctx.attacker,
            damage = ctx.attackData.damage,
            poiseDamage = ctx.attackData.damage
        };
        float angleOfAttack = Vector3.SignedAngle(transform.forward, ctx.attackDirection, Vector3.up);
        HitDirectionType directionType;
        if(angleOfAttack >= -45f && angleOfAttack <= 45f) {
            directionType = HitDirectionType.BACK;
        } else if(angleOfAttack > 45 && angleOfAttack <= 135f) {
            directionType = HitDirectionType.LEFT;
        } else if(angleOfAttack >= -135f && angleOfAttack < -45f) {
            directionType = HitDirectionType.RIGHT;
        } else {
            directionType = HitDirectionType.FORWARD;
        }
        HitReactionData reaction = GetHitReaction(ctx.hurtboxType, directionType);
        stunnedSampler.Begin(reaction.hitReactionGraph);
        stunnedStateTimer = reaction.hitReactionDuraion + stunnedStateOffset;
        (HitForward, HitUp, HitRight) = (ctx.attackDirection, Vector3.up, Vector3.Cross(Vector3.up, ctx.attackDirection).normalized);
        TransitionTo(CombatState.STUNNED);
        if(reaction != null) {
            PlayHitReaction(reaction);
        }
        /*
            Add angled based hit animation here
            0-180 the enemy is being hit on from its left
            -180-0 the enemy is being hit on from its right
         */
        health.TakeDamage(data);
        print("got attack");
    }

    public void PlayHitReaction(HitReactionData data) {
        animator.Play(data.clip.name);
    }

    public void StartAttack() {
        if(lightAttackChain == null || lightAttackChain.Attacks.Length <= 0) return;

        currentAttack = lightAttackChain.GetRandomAttack();
        if(currentAttack.attackClip != null) {
            animator.Play(currentAttack.attackName);
        }
        if (currentAttack.motionGraph != null) {
            attackSampler.Begin(currentAttack.motionGraph);
        }
        weaponHitbox.SetAttackData(currentAttack);
        TransitionTo(CombatState.ATTACKING);
    }
   
    private HitReactionData GetHitReaction(HurtboxType type, HitDirectionType directionType) {
        
        foreach(var map in hurtboxReactionMaps) {
            
            if(map.hurtboxType == type && map.hitDirectionType == directionType)
                return map.data;

        }

        return hurtboxReactionMaps[0].data;

    }

}

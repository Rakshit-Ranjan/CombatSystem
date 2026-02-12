using System;
using UnityEngine;


public class EnemyCombatFSM : MonoBehaviour, IAttackReciever {
    
    [SerializeField] 
    private EnemyHealth health;
    [SerializeField]
    private HurtboxReactionMap[] hurtboxReactionMaps;

    public bool CanAttack {get; private set;}

    private MotionGraphSampler sampler;

    [SerializeField]
    private CombatState combatState;

    [SerializeField]
    private float stateTimer, stunnedStateTimer;

    private CharacterController controller;

    private Vector3 HitForward, HitRight, HitUp;

    private Animator animator;

    void Awake() {
        health = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        combatState = CombatState.IDLE;
        sampler = new MotionGraphSampler();
    }

    void Update() {
        stateTimer += Time.deltaTime;
        switch(combatState) {
            case CombatState.IDLE:
                HandleIdleState();
                break;
            case CombatState.STUNNED:
                HandleStunnedState();
                break;
        }
    }

    private void HandleIdleState() {
        
    }

    private void HandleStunnedState() {
        float normalizedTime = stateTimer/stunnedStateTimer;
        (Vector3 localDelta, float deltaYaw) = sampler.Sample(normalizedTime);
        Vector3 worldDelta = HitForward * localDelta.z + HitRight * localDelta.x + HitUp * localDelta.y;
        controller.Move(worldDelta);

        if(stateTimer >= stunnedStateTimer) {
            sampler.Reset();
            TransitionTo(CombatState.IDLE);
        }
    }

    public void TransitionTo(CombatState state) {
        stateTimer = 0f;
        combatState = state;
    }

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
        Debug.Log(directionType);
        HitReactionData reaction = GetHitReaction(ctx.hurtboxType, directionType);
        sampler.Begin(reaction.hitReactionGraph);
        stunnedStateTimer = reaction.hitReactionDuraion;
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
    }

    public void PlayHitReaction(HitReactionData data) {
        animator.Play(data.clip.name);
    }

    private HitReactionData GetHitReaction(HurtboxType type, HitDirectionType directionType) {
        
        foreach(var map in hurtboxReactionMaps) {
            
            if(map.hurtboxType == type && map.hitDirectionType == directionType)
                return map.data;

        }

        return hurtboxReactionMaps[0].data;

    }

    public void TryStartAttack() {
        Debug.Log("Trying to attack");     
    }
}

using Unity.VisualScripting;
using UnityEngine;


public class EnemyCombatFSM : MonoBehaviour, IAttackReciever {
    
    [SerializeField] 
    private EnemyHealth health;
    [SerializeField]
    private HurtboxReactionMap[] hurtboxReactionMaps;

    [SerializeField]
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

    private void HandleIdleState() => Debug.Log("Idling");

    private void HandleStunnedState() {
        float normalizedTime = stateTimer/stunnedStateTimer;
        (Vector3 localDelta, float deltaYaw) = sampler.Sample(normalizedTime);
        Vector3 worldDelta = HitForward * localDelta.z + HitRight * localDelta.x + HitUp * localDelta.y;
        controller.Move(worldDelta);

        if(stateTimer > stunnedStateTimer) {
            sampler.Reset();
            TransitionTo(CombatState.IDLE);
        }
    }

    public void TransitionTo(CombatState state) {
        stateTimer = 0f;
        combatState = state;
    }

    public void OnIncomingAttack(AttackContext ctx) {
        Debug.Log($"{ctx.attacker} hit the {ctx.hurtboxType} of Enemy");
        DamageData data = new DamageData {
            attacker = ctx.attacker,
            damage = ctx.attackData.damage,
            poiseDamage = ctx.attackData.damage
        };
        HitReactionData reaction = GetHitReaction(ctx.hurtboxType);
        sampler.Begin(reaction.hitReactionGraph);
        stunnedStateTimer = reaction.hitReactionDuraion;
        (HitForward, HitUp, HitRight) = (ctx.attackDirection, Vector3.up, Vector3.Cross(Vector3.up, ctx.attackDirection).normalized);
        TransitionTo(CombatState.STUNNED);
        if(reaction != null) {
            PlayHitReaction(reaction);
            Debug.Log("Play hit reaction");
        }
        
        health.TakeDamage(data);
    }

    public void PlayHitReaction(HitReactionData data) {
        animator.Play("Body Hit");
    }

    private HitReactionData GetHitReaction(HurtboxType type) {
        
        foreach(var map in hurtboxReactionMaps) {
            
            if(map.hurtboxType == type)
                return map.data;

        }

        return null;

    }

}

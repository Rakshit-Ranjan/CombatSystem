using System;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;


public class EnemyCombatFSM : MonoBehaviour, IAttackReciever {

    [SerializeField] private EnemyBrain brain;
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyPerception perception;
    [SerializeField] private EnemyHealth health;
    [SerializeField] private EnemyLocomotion locomotion;
    [SerializeField] private HurtboxReactionMap[] hurtboxReactionMaps;
    [SerializeField] private CombatState combatState;
    [SerializeField] private float stateTimer, stunnedStateTimer, stunnedStateMovingTimer; //stunned state moving timer dictates how long the player moves in the stunned state and is calculated using hitreaction.hitreactionforce
    [SerializeField] private float timeBetweenAttack, attackTimer, attackFacingThreshold; // every n seconds do an attack
    //ATTACK STATE VARIABLES
    [SerializeField] private AttackData currentAttack;
    [SerializeField] private AttackChain lightAttackChain;
    [SerializeField] private int comboIndex;
    [SerializeField] private bool hasQueuedCombo;
    [SerializeField] private Hitbox weaponHitbox;

    private CharacterController controller;
    private MotionGraphSampler stunnedSampler;
    private MotionGraphSampler attackSampler;
    private Animator animator;
    private Vector3 HitForward, HitRight, HitUp;
    public bool BlocksLocomotion;

    //DEBUG PROPERTIES
    public CombatState CurrentState => combatState;
    public float StateTimer => stateTimer;
    public float StunnedStateTimer => stunnedStateTimer;
    public float StunnedStateMovingTimer => stunnedStateMovingTimer;
    public float AttackTimer => attackTimer;
    public bool IsBlockingLocomotion => BlocksLocomotion;
    public bool IsInAttackRange => perception.IsInAttackRange;
    public int ComboIndex => comboIndex;


    void Awake() {
        health = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        enemyController = GetComponent<EnemyController>();
        locomotion = GetComponent<EnemyLocomotion>();
        perception = GetComponent<EnemyPerception>();
        brain = GetComponent<EnemyBrain>();
        weaponHitbox = GetComponentInChildren<Hitbox>();
        combatState = CombatState.IDLE;
        stunnedSampler = new MotionGraphSampler();
        attackSampler = new MotionGraphSampler();
    }

    void Update() {
        BlocksLocomotion = combatState == CombatState.STUNNED || combatState == CombatState.ATTACKING || combatState == CombatState.WINDUP || combatState == CombatState.DEAD;
        stateTimer += Time.deltaTime;
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
        switch (combatState) {
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
            case CombatState.CIRCLING:
                HandleCirclingState();
                break;
            case CombatState.DEAD:
                HandleDeadState();
                break;
        }
    }


    /// <summary>
    /// handling states
    /// </summary>
    private void HandleIdleState() {
        //first check if enemy intent is in attack
        //attack every 3 seconds
        if (brain.CurrentIntent != EnemyIntent.ENGAGE) return;


        if (CombatDirector.Instance.CanAttack(enemyController)) {
            if (perception.IsInAttackRange) {
                if (attackTimer <= 0f) {
                    TransitionTo(CombatState.WINDUP);
                }
            } else {
                return;
            }
        }
        else {
            ResetAttackState();
            TransitionTo(CombatState.CIRCLING);
        }
    }

    private void HandleWindupState() {
        //ABORTING ATTACK
        if (brain.CurrentIntent != EnemyIntent.ENGAGE) {
            BlocksLocomotion = false;
            TransitionTo(CombatState.IDLE);
            return;
        }
        //ROTATE TOWARDS PLAYER
        Vector3 toPlayer = enemyController.playerT.position - transform.position;
        toPlayer.y = 0;
        if (toPlayer.magnitude < 0.001f) return;
        Quaternion lookRotation = Quaternion.LookRotation(toPlayer.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            locomotion.rotationSpeed * Time.deltaTime
        );


        //STARTING ATTACK
        if (Vector3.Angle(transform.forward, toPlayer.normalized) < attackFacingThreshold) {
            if (currentAttack == null) {
                currentAttack = lightAttackChain.GetStarterAttack();
            }
            StartAttack();
        }

    }

    private void HandleAttackingState() {

        if (currentAttack == null) {
            TransitionTo(CombatState.IDLE);
            return;
        }

        float normalizedTime = stateTimer / (currentAttack.GetDuration());
        (Vector3 localDelta, float deltaYaw) = attackSampler.Sample(normalizedTime);
        transform.Rotate(0f, deltaYaw, 0f);
        Vector3 worldDelta = transform.forward * localDelta.z +
                             transform.right * localDelta.x +
                             transform.up * localDelta.y;
        controller.Move(worldDelta);

        if (stateTimer > currentAttack.GetDuration()) {

            if (hasQueuedCombo) {
                ContinueCombo();
            }
            else {
                ResetAttackState();
                TransitionTo(CombatState.IDLE);
            }
        }

    }

    private void HandleStunnedState() {
        float normalizedTime = stateTimer / (stunnedStateMovingTimer);
        (Vector3 localDelta, float deltaYaw) = stunnedSampler.Sample(normalizedTime);
        Vector3 worldDelta = HitForward * localDelta.z + HitRight * localDelta.x + HitUp * localDelta.y;
        controller.Move(worldDelta);

        if (stateTimer >= stunnedStateTimer) {
            stunnedSampler.Reset();
            TransitionTo(CombatState.IDLE);
        }
    }

    private void HandleCirclingState() {
        if (brain.CurrentIntent != EnemyIntent.ENGAGE) {
            ResetAttackState();
            TransitionTo(CombatState.IDLE);
            return;
        }
        Vector3 toPlayer = enemyController.playerT.position - transform.position;

        locomotion.FaceDirection(toPlayer);
        // if (CombatDirector.Instance.CanAttack(enemyController)) {
        //     if (attackTimer <= 0f) {
        //         CombatDirector.Instance.NotifyAttackStarted(enemyController);
        //         TransitionTo(CombatState.WINDUP);
        //         return;
        //     }
        // }

    }

    private void HandleDeadState() {
        animator.SetTrigger("Death");
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
        print(data.damage);
        float angleOfAttack = Vector3.SignedAngle(transform.forward, ctx.attackDirection, Vector3.up);
        HitDirectionType directionType;
        if (angleOfAttack >= -45f && angleOfAttack <= 45f) {
            directionType = HitDirectionType.BACK;
        }
        else if (angleOfAttack > 45 && angleOfAttack <= 135f) {
            directionType = HitDirectionType.LEFT;
        }
        else if (angleOfAttack >= -135f && angleOfAttack < -45f) {
            directionType = HitDirectionType.RIGHT;
        }
        else {
            directionType = HitDirectionType.FORWARD;
        }
        bool willDie = health.WillDie(data);
        data.killedTarget = willDie;
        if(willDie) {
            data.hitPoint = ctx.attackHitPoint;
            CombatFeedbackManager.Instance.PlayFatalHitFeedback(ctx, data);
            if(enemyController == CombatDirector.Instance.GetFocusEnemy()) 
            if(CombatDirector.Instance.HasFocus(enemyController))
                CombatDirector.Instance.ClearFocusEnemy(enemyController);
            CombatDirector.Instance.UnregisterEnemy(enemyController);
            TransitionTo(CombatState.DEAD);
            
        } else {
            HitReactionData reaction = GetHitReaction(ctx.hurtboxType, directionType);
            stunnedSampler.Begin(reaction.hitReactionGraph);
            stunnedStateTimer = reaction.hitReactionDuraion;
            stunnedStateMovingTimer = reaction.hitReactionForce;
            (HitForward, HitUp, HitRight) = (ctx.attackDirection, Vector3.up, Vector3.Cross(Vector3.up, ctx.attackDirection).normalized);

            if (!CombatDirector.Instance.HasFocus(enemyController)) {
                CombatDirector.Instance.SetFocusEnemy(enemyController);
            }
            data.hitPoint = ctx.attackHitPoint;
            CombatFeedbackManager.Instance.PlayHitFeedback(ctx, data);

            ResetAttackState();
            TransitionTo(CombatState.STUNNED);
            if (reaction != null) {
                PlayHitReaction(reaction);
            }
        }
        health.TakeDamage(data);
    }

    private void ContinueCombo() {
        if (currentAttack == null || currentAttack.motionGraph == null) {
            TransitionTo(CombatState.IDLE);
            return;
        }

        AttackData nextAttack = lightAttackChain?.GetNextAttack(comboIndex);
        comboIndex++;
        if (nextAttack == null) {
            ResetAttackState();
            TransitionTo(CombatState.IDLE);
            return;
        }

        currentAttack = nextAttack;
        StartAttack();
        TransitionTo(CombatState.ATTACKING);
    }

    public void PlayHitReaction(HitReactionData data) {
        animator.Play(data.clip.name);
    }

    public void StartAttack() {
        if (currentAttack == null) return;

        if (currentAttack.attackClip != null) {
            animator.Play(currentAttack.attackName);
        }
        if (currentAttack.motionGraph != null) {
            attackSampler.Begin(currentAttack.motionGraph);
        }
        weaponHitbox.SetAttackData(currentAttack);
        hasQueuedCombo = lightAttackChain.GetNextAttack_Enemy(comboIndex) != null;
        TransitionTo(CombatState.ATTACKING);
    }

    public void EnterParryStun(Transform source, AttackContext ctx, HitDirectionType directionType) {
        print("Enter Stunned state");
        weaponHitbox.DisableHitbox();
        attackSampler.Reset();
        currentAttack = null;
        Vector3 stunDir = (transform.position - source.position).normalized;
        stunDir.y = 0f;
        HitForward = stunDir;
        HitUp = Vector3.up;
        HitRight = Vector3.Cross(Vector3.up, stunDir).normalized;
        HitReactionData reaction = GetHitReaction(ctx.hurtboxType, directionType);
        stunnedSampler.Begin(reaction.hitReactionGraph);
        stunnedStateTimer = reaction.hitReactionDuraion;
        stunnedStateMovingTimer = reaction.hitReactionForce;
        attackTimer = timeBetweenAttack;
        CombatDirector.Instance.ClearFocusEnemy(enemyController);
        PlayHitReaction(reaction);
        TransitionTo(CombatState.STUNNED);
    }

    public void EnableWeaponHitbox() {
        weaponHitbox.EnableHitbox();
    }

    public void DisableWeaponHitbox() {
        weaponHitbox.DisableHitbox();
    }

    private void ResetAttackState() {
        weaponHitbox.DisableHitbox();
        weaponHitbox.SetAttackData(null);

        attackSampler.Reset();

        currentAttack = null;
        comboIndex = 0;
        hasQueuedCombo = false;

        attackTimer = timeBetweenAttack;
        BlocksLocomotion = false;
    }

    private HitReactionData GetHitReaction(HurtboxType type, HitDirectionType directionType) {

        foreach (var map in hurtboxReactionMaps) {

            if (map.hurtboxType == type && map.hitDirectionType == directionType)
                return map.data;

        }

        return hurtboxReactionMaps[0].data;

    }

}

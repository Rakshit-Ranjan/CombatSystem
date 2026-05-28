using UnityEngine;

public enum CombatState {
    IDLE,
    WINDUP,
    ATTACKING,
    BLOCKING,
    DODGING,
    PARRYING,
    STUNNED
}


public class CombatFSM : MonoBehaviour, IAttackReciever {

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private InputBuffer inputBuffer;
    [SerializeField] private PlayerLocomotionController locomotion;

    [Header("Attack Chains")]
    [SerializeField] private AttackChain lightAttackChain;
    [SerializeField] private AttackChain heavyAttackChain;

    // State
    [SerializeField] private CombatState currentState = CombatState.IDLE;
    [SerializeField] private float stateTimer;
    [SerializeField] private float parryTimer;
    [SerializeField] private float dodgeTimer;
    [SerializeField] private float dodgeCooldown;
    [SerializeField] private int comboIndex;
    [SerializeField] private bool hasQueuedCombo;
    [SerializeField] private AttackData currentAttack;
    [SerializeField] private AttackChain currentAttackChain;
    [SerializeField] private ParryData parryData;
    [SerializeField] private DodgeData dodgeData;
    [SerializeField] private Hitbox weaponHitbox;
    private Vector3 worldDodgeDir;
    private Vector3 localDodgeDir;
    private Vector3 DodgeForward, DodgeUp, DodgeRight;
    private MotionGraphSampler attackSampler;
    private MotionGraphSampler dodgeSampler;

    private int attackHash;
    private int comboIndexHash;

    // Public properties
    public CombatState CurrentState => currentState;
    public bool IsAttacking => currentState == CombatState.ATTACKING;
    public bool CanRecieveInput => currentState == CombatState.IDLE;


    void Awake() {
        if (animator == null) animator = GetComponent<Animator>();
        if (locomotion == null) locomotion = GetComponent<PlayerLocomotionController>();
        if (inputBuffer == null) inputBuffer = GetComponent<InputBuffer>();
        attackSampler = new MotionGraphSampler();
        dodgeSampler = new MotionGraphSampler();
        attackHash = Animator.StringToHash("Attack");
        comboIndexHash = Animator.StringToHash("ComboIndex");
        weaponHitbox = GetComponentInChildren<Hitbox>();
    }

    private void Update() {
        stateTimer += Time.deltaTime;
        HandleParryInput();
        HandleDodgingInput();
        switch (currentState) {
            case CombatState.IDLE:
                HandleIdleState();
                break;
            case CombatState.ATTACKING:
                HandleAttackingState();
                break;
            case CombatState.PARRYING:
                HandleParryingState();
                break;
            case CombatState.DODGING:
                HandleDodgingState();
                break;
                // Additional states like BLOCKING, DODGING, STUNNED can be handled here
        }
    }

    /* State Handlers */
    private void HandleIdleState() {
        if (inputBuffer.TryConsumeInput(out InputActionType inputType)) {
            switch (inputType) {
                case InputActionType.LIGHT_ATTACK:
                    if (lightAttackChain != null) {
                        AttackData starterAttack = lightAttackChain.GetStarterAttack();
                        if (starterAttack != null) {
                            StartAttack(starterAttack, lightAttackChain);
                        }
                    }
                    break;
                case InputActionType.HEAVY_ATTACK:
                    if (heavyAttackChain != null) {
                        AttackData starterAttack = heavyAttackChain.GetStarterAttack();
                        if (starterAttack != null) {
                            StartAttack(starterAttack, heavyAttackChain);
                        }
                    }
                    break;

                    // Handle other input types like BLOCK, DODGE, etc.
            }
        }
    }

    private void HandleAttackingState() {
        if (currentAttack == null) {
            TransitionToIdle();
            return;
        }

        //check for combo window
        if (currentAttack.IsInComboWindow(stateTimer)) {
            if (!hasQueuedCombo) // if combo is not queued yet, try getting a combo
            {
                //check for combo input
                if (inputBuffer.PeekNextInput(out InputActionType inputType)) {
                    switch (inputType) {
                        case InputActionType.LIGHT_ATTACK:
                            AttackData nextAttack = lightAttackChain.GetNextAttack(comboIndex);
                            currentAttackChain = lightAttackChain;
                            if (nextAttack != null) {
                                hasQueuedCombo = true;
                                inputBuffer.TryConsumeInput(out _); //consume the input
                            }
                            break;
                        case InputActionType.HEAVY_ATTACK:
                            //Get next attack
                            nextAttack = heavyAttackChain.GetNextAttack(comboIndex);
                            currentAttackChain = heavyAttackChain;
                            if (nextAttack != null) {
                                hasQueuedCombo = true;
                                inputBuffer.TryConsumeInput(out _); //consume the input
                            }
                            break;
                    }
                }
            }
        }

        // Apply attack movement through motion graph
        float normalizedTime = stateTimer / currentAttack.GetDuration();
        if (attackSampler != null) {
            (Vector3 localDelta, float deltaYaw) = attackSampler.Sample(normalizedTime);
            Vector3 worldDelta =
                transform.forward * localDelta.z +
                transform.right * localDelta.x +
                transform.up * localDelta.y;
            locomotion.ApplyAttackMovement(worldDelta, deltaYaw);
        }


        //check for end of attack
        if (stateTimer >= currentAttack.GetDuration()) {
            if (hasQueuedCombo) {
                ContinueCombo();
            }
            else {
                TransitionToIdle();
            }

        }

    }

    private void HandleParryingState() {
        // Parrying logic can be implemented here
        parryTimer += Time.deltaTime;
        float endTime = parryData.activeTime + parryData.recoveryTime + parryData.startupTime;
        if (parryTimer >= endTime) {
            TransitionToIdle();
        }
    }

    private void HandleDodgingState() {
        dodgeTimer += Time.deltaTime;
        float endTime = dodgeData.duration;
        // if (dodgeTimer > endTime) {
        //     TransitionToIdle();
        // }
        float normalizedTime = dodgeTimer / dodgeData.duration;
        (Vector3 localDelta, float yawDelta) = dodgeSampler.Sample(normalizedTime);
        Vector3 worldDelta = DodgeForward * localDelta.z + DodgeRight * localDelta.x + DodgeUp * localDelta.y;
        GetComponent<CharacterController>().Move(worldDelta);
    }
    
    private void HandleParryInput() {


        if (!inputBuffer.PeekNextInput(out InputActionType input))
            return;

        if (input != InputActionType.PARRY)
            return;

        if (currentState == CombatState.IDLE || currentState == CombatState.ATTACKING && parryData.canParryDuringAttack) {
            inputBuffer.TryConsumeInput(out _);
            StartParry();
        }

    }

    private void HandleDodgingInput() {
        if (currentState == CombatState.DODGING)
            return;

        if (!inputBuffer.PeekNextInput(out InputActionType input))
            return;

        if (input != InputActionType.DODGE)
            return;

        if (currentState == CombatState.IDLE) {
            StartDodge();
        }
    }

    /* Helper Methods */
    public void ExitDodgingState() {
        TransitionToIdle();
    }
    private void StartAttack(AttackData attackData, AttackChain attackChain) {
        currentAttack = attackData;
        currentAttackChain = attackChain;

        TransitionTo(CombatState.ATTACKING);

        locomotion.SetCombatMode(true);
        locomotion.LockMovement();
        weaponHitbox.SetAttackData(currentAttack);
        if (attackData.motionGraph != null) {
            attackSampler.Begin(attackData.motionGraph);
        }

        if (attackData.attackClip != null) {
            animator.Play(attackData.attackName, 1, 0f);
        }
    }

    private void StartParry() {
        TransitionTo(CombatState.PARRYING);
        parryTimer = 0f;
        locomotion.SetCombatMode(true);
        locomotion.LockMovement();
        // Play parry animation if available
        animator.Play("Parry", 1, 0f);
    }

    private void StartDodge() {
        TransitionTo(CombatState.DODGING);
        dodgeTimer = 0f;
        dodgeSampler.Begin(dodgeData.dodgeGraph);
        (worldDodgeDir, localDodgeDir) = locomotion.GetDodgeDirection();
        (DodgeForward, DodgeUp, DodgeRight) = (worldDodgeDir, Vector3.up, Vector3.Cross(Vector3.up, worldDodgeDir).normalized);
        locomotion.SetCombatMode(true);
        locomotion.LockMovement();
        animator.SetFloat("DodgeX", localDodgeDir.x);
        animator.SetFloat("DodgeZ", localDodgeDir.z);
        animator.Play("Dodge");
    }


    /*
        get next attack
        last combo attack displacement
        play the next attack
     */
    private void ContinueCombo() {
        if (currentAttack == null || currentAttack.motionGraph == null) {
            TransitionToIdle();
            return;
        }

        AttackData nextAttack = currentAttackChain?.GetNextAttack(comboIndex);
        comboIndex++;
        if (nextAttack == null) {
            TransitionToIdle();
            return;
        }
        // get combo displacement accumulation
        float lastAttackDistance =
        currentAttack.motionGraph.forward.Evaluate(1f) *
        currentAttack.motionGraph.distanceMultiplier;
        Vector3 comboTransitionDelta = transform.forward * lastAttackDistance;
        // Apply it ONCE
        GetComponent<CharacterController>().Move(comboTransitionDelta);

        currentAttack = nextAttack;
        hasQueuedCombo = false;
        locomotion.LockMovement();
        TransitionTo(CombatState.ATTACKING);
        attackSampler.Begin(currentAttack.motionGraph);
        weaponHitbox.SetAttackData(currentAttack);
        if (currentAttack.attackClip != null) {
            animator.Play(currentAttack.attackName, 1, 0f);
        }
    }

    private void TransitionTo(CombatState newState, float? customDuration = null) {
        currentState = newState;
        stateTimer = 0f;
        if (customDuration.HasValue) {
            stateTimer = customDuration.Value;
        }
    }

    private void TransitionToIdle() {
        currentState = CombatState.IDLE;
        comboIndex = 0;
        hasQueuedCombo = false;
        currentAttack = null;
        currentAttackChain = null;
        weaponHitbox.SetAttackData(null);
        locomotion.UnlockMovement();
        locomotion.SetCombatMode(false);
        attackSampler?.Reset();
        dodgeSampler?.Reset();

    }

    public void OnIncomingAttack(AttackContext ctx) {
        
    }

}

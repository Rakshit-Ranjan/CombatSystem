using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyLocomotion : MonoBehaviour {

    [Header("Components")]

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private EnemyCombatFSM combat;
    [SerializeField] private EnemyPerception perception;
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private Animator animator;

    public float Speed { get; private set; }

    [Header("Settings")]
    public float moveSpeed;
    public float circlingSpeed;
    public float rotationSpeed;
    public float gravityY;
    [SerializeField] private float slotArriveDistance = 0.8f;
    [SerializeField] private float slotAngleArriveThreshold = 8f;
    [SerializeField] private float radialCorrectionStrength = 0.75f;
    [SerializeField] private float focusAvoidArc = 40f;

    private Vector3 engagedMoveDirection;


    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
        enemyController = GetComponent<EnemyController>();
        perception = GetComponent<EnemyPerception>();
        combat = GetComponent<EnemyCombatFSM>();
        animator = GetComponent<Animator>();
    }

    void Start() {
        agent.updatePosition = false;
        agent.updateRotation = false;
    }
    public void Move(Vector3 direction) {

        characterController.Move(Speed * Time.deltaTime * direction.normalized);
        characterController.Move(gravityY * Time.deltaTime * Vector3.up);
    }

    public void Stop() {
        agent.ResetPath();
        engagedMoveDirection = Vector3.zero;
        Speed = 0f;

        animator.SetFloat("Speed", 0f, 0.05f, Time.deltaTime);
        animator.SetFloat("Horizontal", 0f, 0.05f, Time.deltaTime);
        animator.SetFloat("Vertical", 0f, 0.05f, Time.deltaTime);
    }

    public void SetTarget(Transform t) {
        agent.SetDestination(t.position);
    }

    public void SetTarget(Vector3 target) {
        agent.SetDestination(target);
    }

    public void FaceDirection(Vector3 direction) {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

    }

    public void HandleLocomotion() {
        float desiredVelocity = agent.desiredVelocity.magnitude;

        SetVelocity(moveSpeed);

        if (desiredVelocity > 0.01f) {
            Move(agent.desiredVelocity);
            agent.nextPosition = transform.position;
            FaceDirection(agent.desiredVelocity);
        }

        float normalizedSpeed = Mathf.Clamp01(desiredVelocity / moveSpeed);

        animator.SetFloat("Speed", normalizedSpeed, 0.05f, Time.deltaTime);
        animator.SetFloat("Horizontal", 0f, 0.05f, Time.deltaTime);
        animator.SetFloat("Vertical", 0f, 0.05f, Time.deltaTime);
    }

    public void MoveToPlayer(Transform t) {
        SetTarget(t);

        Vector3 desired = agent.desiredVelocity;
        desired.y = 0f;

        if (desired.sqrMagnitude < 0.001f) {
            Stop();
            return;
        }

        SetVelocity(circlingSpeed);

        engagedMoveDirection = desired.normalized;
        Move(engagedMoveDirection);
        agent.nextPosition = transform.position;

        FaceDirection(t.position - transform.position);

        Vector3 localVel = transform.InverseTransformDirection(engagedMoveDirection * Speed);

        animator.SetFloat("Speed", 0f, 0.05f, Time.deltaTime);
        animator.SetFloat("Horizontal", localVel.x, 0.05f, Time.deltaTime);
        animator.SetFloat("Vertical", localVel.z, 0.05f, Time.deltaTime);
    }

    public void HandleLocomotionWhileEngaged() {
        engagedMoveDirection = Vector3.zero;

        switch (combat.CurrentState) {
            case CombatState.IDLE:
                HandleFocusedApproachOrOrbit();
                break;

            case CombatState.CIRCLING:
                OrbitToAssignedSlot();
                break;

            case CombatState.ATTACKING:
            case CombatState.WINDUP:
            case CombatState.STUNNED:
                Stop();
                break;
        }

        SetVelocity(circlingSpeed);

        Vector3 localVel = transform.InverseTransformDirection(engagedMoveDirection * Speed);

        animator.SetFloat("Speed", 0f, 0.05f, Time.deltaTime);
        animator.SetFloat("Horizontal", localVel.x, 0.05f, Time.deltaTime);
        animator.SetFloat("Vertical", localVel.z, 0.05f, Time.deltaTime);
    }

    public void ToCombatAnimation(bool c) {
        animator.SetBool("IsInCombat", c);
    }

    public void SetVelocity(float speed) {
        Speed = speed;
    }

    private void HandleFocusedApproachOrOrbit() {
        CombatDirector director = CombatDirector.Instance;

        if (director == null || enemyController.playerT == null) {
            Stop();
            return;
        }

        if (!director.HasFocus(enemyController)) {
            OrbitToAssignedSlot();
            return;
        }

        Vector3 toPlayer = enemyController.playerT.position - transform.position;
        toPlayer.y = 0f;

        FaceDirection(toPlayer);

        if (perception.IsInAttackRange) {
            Stop();
            return;
        }

        SetTarget(enemyController.playerT);

        Vector3 desired = agent.desiredVelocity;
        desired.y = 0f;

        if (desired.sqrMagnitude < 0.001f) {
            desired = agent.steeringTarget - transform.position;
            desired.y = 0f;
        }

        if (desired.sqrMagnitude < 0.001f) {
            desired = toPlayer;
            desired.y = 0f;
        }

        if (desired.sqrMagnitude < 0.001f) {
            engagedMoveDirection = Vector3.zero;
            return;
        }

        SetVelocity(circlingSpeed);

        engagedMoveDirection = desired.normalized;
        Move(engagedMoveDirection);
        agent.nextPosition = transform.position;
    }


    private void OrbitToAssignedSlot() {
        CombatDirector director = CombatDirector.Instance;
        Transform player = enemyController.playerT;

        if (director == null || player == null) {
            Stop();
            engagedMoveDirection = Vector3.zero;
            return;
        }

        if (!director.TryGetAssignedSlotAngle(enemyController, out float targetAngle)) {
            Stop();
            engagedMoveDirection = Vector3.zero;
            return;
        }

        Vector3 toEnemy = transform.position - player.position;
        toEnemy.y = 0f;

        if (toEnemy.sqrMagnitude < 0.001f) {
            toEnemy = -player.forward;
            toEnemy.y = 0f;
        }

        Vector3 radialDir = toEnemy.normalized;
        float currentRadius = toEnemy.magnitude;
        float currentAngle = NormalizeAngle(Vector3.SignedAngle(director.SlotBasisForward, radialDir, Vector3.up));
        float angleDelta = Mathf.DeltaAngle(currentAngle, targetAngle);
        float radiusError = director.CirclingRadius - currentRadius;

        FaceDirection(player.position - transform.position);

        if (Mathf.Abs(angleDelta) <= slotAngleArriveThreshold &&
            Mathf.Abs(radiusError) <= slotArriveDistance) {
            Stop();
            engagedMoveDirection = Vector3.zero;
            return;
        }

        float orbitDirection = angleDelta >= 0f ? 1f : -1f;
        if (WouldCrossFocusLane(currentAngle, targetAngle, orbitDirection, director, player)) {
            orbitDirection *= -1f;
        }

        Vector3 clockwiseTangent = Vector3.Cross(Vector3.up, radialDir).normalized;
        Vector3 counterClockwiseTangent = -clockwiseTangent;
        Vector3 tangentDir = orbitDirection > 0f ? clockwiseTangent : counterClockwiseTangent;
        Vector3 radialCorrection = radialDir * radiusError * radialCorrectionStrength;
        Vector3 moveDirection = tangentDir + radialCorrection;

        if (moveDirection.sqrMagnitude < 0.001f) {
            moveDirection = tangentDir;
        }

        agent.ResetPath();
        SetVelocity(circlingSpeed);
        engagedMoveDirection = moveDirection.normalized;
        Move(engagedMoveDirection);
        agent.nextPosition = transform.position;
    }

    private bool WouldCrossFocusLane(
        float currentAngle,
        float targetAngle,
        float orbitDirection,
        CombatDirector director,
        Transform player
    ) {
        EnemyController focusEnemy = director.GetFocusEnemy();
        if (focusEnemy == null || focusEnemy == enemyController)
            return false;

        Vector3 toFocus = focusEnemy.transform.position - player.position;
        toFocus.y = 0f;

        if (toFocus.sqrMagnitude < 0.001f)
            return false;

        float focusAngle = NormalizeAngle(Vector3.SignedAngle(director.SlotBasisForward, toFocus.normalized, Vector3.up));

        if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, focusAngle)) <= focusAvoidArc)
            return true;

        if (Mathf.Abs(Mathf.DeltaAngle(targetAngle, focusAngle)) <= focusAvoidArc)
            return true;

        return IsAngleBetweenAlongDirection(currentAngle, targetAngle, focusAngle, orbitDirection);
    }

    private bool IsAngleBetweenAlongDirection(float start, float end, float test, float direction) {
        start = NormalizeAngle(start);
        end = NormalizeAngle(end);
        test = NormalizeAngle(test);

        if (direction >= 0f) {
            float total = ClockwiseDistance(start, end);
            float toTest = ClockwiseDistance(start, test);
            return toTest > 0f && toTest < total;
        }

        float counterTotal = ClockwiseDistance(end, start);
        float counterToTest = ClockwiseDistance(test, start);
        return counterToTest > 0f && counterToTest < counterTotal;
    }

    private float ClockwiseDistance(float from, float to) {
        return Mathf.Repeat(to - from + 360f, 360f);
    }

    private float NormalizeAngle(float angle) {
        return Mathf.Repeat(angle + 360f, 360f);
    }

}

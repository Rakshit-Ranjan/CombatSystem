using UnityEngine;

public class EnemyPerception : MonoBehaviour {

    [Header("Perception Settings")]
    [SerializeField] private Transform playerTransform;
    public bool CanSeePlayer;
    public float DistToPlayer { get; private set; }
    public bool IsInAttackRange { get; private set; }
    public bool IsInEngagementRange { get; private set; }


    public float viewDistance;

    public DistanceRange attackRange;

    public float engagementRange;
    public float disengagementRadius;

    public LayerMask viewMask;

    public Transform EnemyEyeTransform;

    void Awake() {
        playerTransform = FindAnyObjectByType<PlayerLocomotionController>().transform;
    }

    void Update() {
        HandlePerception();
    }

    private void HandlePerception() {
        DistToPlayer = Vector3.Distance(playerTransform.position, transform.position);
        IsInAttackRange = DistToPlayer < attackRange.max;
        if (DistToPlayer > viewDistance) {
            CanSeePlayer = false;
            return;
        }

        if (!IsInEngagementRange) {
            if (DistToPlayer <= engagementRange)
                IsInEngagementRange = true;
        }
        else {
            if (DistToPlayer >= disengagementRadius)
                IsInEngagementRange = false;
        }

        Vector3 dir = (playerTransform.position - transform.position).normalized;
        if (Physics.Raycast(new Ray(EnemyEyeTransform.position, dir), out RaycastHit hitInfo, DistToPlayer, viewMask)) {
            PlayerLocomotionController player = hitInfo.collider.GetComponent<PlayerLocomotionController>();
            if (player != null)
                CanSeePlayer = true;
            else
                CanSeePlayer = false;
        }
        else {
            CanSeePlayer = false;
        }


    }

    public bool IsTooClose() => attackRange.IsTooClose(DistToPlayer);
    public bool IsTooFar() => attackRange.IsTooFar(DistToPlayer);

}
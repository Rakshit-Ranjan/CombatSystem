using UnityEngine;

public class EnemyPerception : MonoBehaviour {
    
    [Header("Perception Settings")]
    [SerializeField] private Transform playerTransform;
    public bool CanSeePlayer {get; private set;}
    public float DistToPlayer {get; private set;}
    public bool IsInAttackRange {get; private set;}
    public bool IsInEngagementRange {get; private set;}


    public float viewDistance;

    public float attackRange;

    public float engagementRadius;
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
        if(DistToPlayer <= attackRange) {
            IsInAttackRange = true;
        } else {
            IsInAttackRange = false;
        }
        if(DistToPlayer > viewDistance) {
            CanSeePlayer = false;
            return;
        }

        if(!IsInEngagementRange) {
            if(DistToPlayer <= engagementRadius) 
                IsInEngagementRange = true;
        } else {
            if(DistToPlayer >= disengagementRadius)
                IsInEngagementRange = false;
        }

        Vector3 dir = (playerTransform.position - transform.position).normalized;
        if(Physics.Raycast(new Ray(EnemyEyeTransform.position, dir), out RaycastHit hitInfo, DistToPlayer, viewMask)) {
            PlayerLocomotionController player = hitInfo.collider.GetComponent<PlayerLocomotionController>();
            if(player != null)
                CanSeePlayer = true;
            else 
                CanSeePlayer = false;
        } else {
            CanSeePlayer = false;
        }

    
    }

}
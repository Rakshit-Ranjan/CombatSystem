using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyLocomotion : MonoBehaviour {
    
    [Header("Components")]
    
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    public Transform player;
    public float moveSpeed;
    public float rotationSpeed;
    public float gravityY;


    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<CharacterController>();
        player = FindAnyObjectByType<PlayerLocomotionController>().transform;
    }

    void Start() {
        agent.updatePosition = false;
        agent.updateRotation = false;
    }
    public void Move(Vector3 direction) {
        
        controller.Move(moveSpeed * Time.deltaTime * direction.normalized);
        controller.Move(gravityY * Time.deltaTime * Vector3.up);
    }

    public void Stop() {
        agent.ResetPath();
    }

    public void SetTarget(Transform t) {
        agent.SetDestination(t.position);
    }

    public void FaceDirection(Vector3 direction) {
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

    }

    public void HandleLocomotion() {
        if(agent.desiredVelocity.magnitude > 0.01f) {
            Move(agent.desiredVelocity);
            agent.nextPosition = transform.position;
            FaceDirection(agent.desiredVelocity);
        }
    }

}

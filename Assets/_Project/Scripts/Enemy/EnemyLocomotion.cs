using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyLocomotion : MonoBehaviour {
    
    [Header("Components")]
    
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private EnemyCombatFSM combat;
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    public float moveSpeed;
    public float rotationSpeed;
    public float gravityY;


    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
        enemyController = GetComponent<EnemyController>();
        combat = GetComponent<EnemyCombatFSM>();
        animator = GetComponent<Animator>();
    }

    void Start() {
        agent.updatePosition = false;
        agent.updateRotation = false;
    }
    public void Move(Vector3 direction) {
        
        characterController.Move(moveSpeed * Time.deltaTime * direction.normalized);
        characterController.Move(gravityY * Time.deltaTime * Vector3.up);
    }

    public void Stop() {
        agent.ResetPath();
        animator.SetFloat("Speed", 0f, 0.05f, Time.deltaTime);
    }

    public void SetTarget(Transform t) {
        agent.SetDestination(t.position);
    }

    public void SetTarget(Vector3 target) {
        agent.SetDestination(target);
    }

    public void FaceDirection(Vector3 direction) {
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

    }

    public void HandleLocomotion() {
        float desiredVelocity = agent.desiredVelocity.magnitude;

        if(desiredVelocity > 0.01f) {
            Move(agent.desiredVelocity);
            agent.nextPosition = transform.position;
            FaceDirection(agent.desiredVelocity);
        }
        float normalizedSpeed = Mathf.Clamp01(desiredVelocity / moveSpeed);
        animator.SetFloat("Speed", normalizedSpeed, 0.05f, Time.deltaTime);
    }

    public void HandleLocomotionWhileAttacking() {
        
        switch (combat.CurrentState) {
            
            case CombatState.ATTACKING:
                Stop();
                break;

            case CombatState.CIRCLING:
                Vector3 targetPos = CombatDirector.Instance.GetSlotPosition(enemyController);
                
                SetTarget(targetPos);
                HandleLocomotion();
                break;

        }

    }



}

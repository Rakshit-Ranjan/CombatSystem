using System;
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
    public float moveSpeed;
    public float rotationSpeed;
    public float gravityY;


    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
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
        animator.SetFloat("Speed", 0f, 0.05f, Time.deltaTime);
    }

    public void SetTarget(Transform t) {
        agent.SetDestination(t.position);
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


}

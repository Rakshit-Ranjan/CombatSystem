using UnityEngine;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(EnemyCombatFSM))]
[RequireComponent(typeof(EnemyLocomotion))]
public class EnemyController : MonoBehaviour {

    [SerializeField]
    private EnemyBrain brain;
    [SerializeField]
    private EnemyCombatFSM combat;
    [SerializeField]
    private EnemyLocomotion locomotion;

    public Transform playerT;

    void Awake() {
        (brain, combat, locomotion) = (GetComponent<EnemyBrain>(), GetComponent<EnemyCombatFSM>(), GetComponent<EnemyLocomotion>());
        playerT = FindAnyObjectByType<PlayerLocomotionController>().transform;
    }

    void Update() {
        HandleStates();
    }

    void OnEnable() {
        if (CombatDirector.Instance != null) {
            CombatDirector.Instance.RegisterEnemy(this);
        }
    }

    void OnDisable() {
        if (CombatDirector.Instance != null) {
            CombatDirector.Instance.UnregisterEnemy(this);
        }
    }

    private void HandleStates() {

        if (combat.BlocksLocomotion) {
            locomotion.Stop();
            return;
        }

        switch (brain.CurrentIntent) {

            case EnemyIntent.IDLE:
                locomotion.Stop();
                break;
            case EnemyIntent.CHASE:
                locomotion.SetTarget(playerT);
                locomotion.HandleLocomotion();
                break;
            case EnemyIntent.ATTACK:
                locomotion.HandleLocomotionWhileAttacking();
                break;
            default:
                break;

        }

    }


}
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
    [SerializeField]
    private EnemyPerception perception;

    public bool IsInCombatGroup { get; private set; }

    public Transform playerT;

    void Awake() {
        (brain, combat, locomotion, perception) = (
                                                GetComponent<EnemyBrain>(),
                                                GetComponent<EnemyCombatFSM>(),
                                                GetComponent<EnemyLocomotion>(),
                                                GetComponent<EnemyPerception>()
                                                );
        playerT = FindAnyObjectByType<PlayerLocomotionController>().transform;
    }

    void Update() {
        UpdateCombatGroupMembership();

        if(IsInCombatGroup && CombatDirector.Instance != null) {
            CombatDirector.Instance.TryRefreshSlotBasis(playerT);
        }

        HandleStates();
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

    private void UpdateCombatGroupMembership() {

        if (!IsInCombatGroup) {
            if (perception.CanSeePlayer && perception.IsInEngagementRange) {
                IsInCombatGroup = true;
                if(CombatDirector.Instance != null) 
                    CombatDirector.Instance.RegisterEnemy(this);
                locomotion.ToCombatAnimation(true);
            }
        } else {
            if (!perception.CanSeePlayer || !perception.IsInEngagementRange) {
                IsInCombatGroup = false;
                locomotion.ToCombatAnimation(false);
                if(CombatDirector.Instance != null) 
                    CombatDirector.Instance.UnregisterEnemy(this);
            }
        }
    }


}
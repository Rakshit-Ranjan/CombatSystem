using Unity.AppUI.Core;
using UnityEngine;

[RequireComponent(typeof(EnemyPerception))]
public class EnemyBrain : MonoBehaviour {

    [SerializeField]
    private EnemyPerception perception;
    private EnemyController controller;
    private CombatDirector director;
    private float evalTimer;

    public EnemyIntent CurrentIntent;

    public float stateEvalInterval;

    void Awake() {
        perception = GetComponent<EnemyPerception>();
        controller = GetComponent<EnemyController>();
        CurrentIntent = EnemyIntent.IDLE;
        director = CombatDirector.Instance;
    }

    void Update() {
        evalTimer += Time.deltaTime;
        if (evalTimer < stateEvalInterval) return;
        evalTimer = 0f;
        EvaluateIntent();
    }

    private void EvaluateIntent() {
        EnemyIntent newIntent;

        if (!perception.CanSeePlayer) {
            newIntent = EnemyIntent.IDLE;
        }
        else if (!perception.IsInEngagementRange) {
            newIntent = EnemyIntent.CHASE;
        }
        else {
            newIntent = EnemyIntent.ENGAGE;
        }

        if (CurrentIntent != newIntent) {
            CurrentIntent = newIntent;
        }
    }

}

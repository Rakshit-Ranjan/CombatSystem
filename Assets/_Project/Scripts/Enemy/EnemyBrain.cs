using UnityEngine;

[RequireComponent(typeof(EnemyPerception))]
public class EnemyBrain : MonoBehaviour {

    [SerializeField] 
    private EnemyPerception perception;
    private float evalTimer;

    public EnemyIntent CurrentIntent;

    public float stateEvalInterval;

    void Awake() {
        perception = GetComponent<EnemyPerception>();
        CurrentIntent = EnemyIntent.IDLE;
    }

    void Update() {
        evalTimer += Time.deltaTime;
        if(evalTimer < stateEvalInterval ) return;
        evalTimer = 0f;
        EvaluateIntent();
    }

    private void EvaluateIntent() {
        
        if(!perception.CanSeePlayer) {
            CurrentIntent = EnemyIntent.IDLE;
            return;
        }

        if(!perception.IsInEngagementRange) {
            CurrentIntent = EnemyIntent.CHASE;
            return;
        }
        CurrentIntent = EnemyIntent.ATTACK;
    }

}

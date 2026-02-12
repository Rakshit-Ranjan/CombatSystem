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
    }

    void Update() {
        evalTimer += Time.deltaTime;
        if(evalTimer < stateEvalInterval ) return;
        evalTimer = 0f;
        EvaluateIntent();
    }

    private void EvaluateIntent() {
        EnemyIntent newIntent;
        if(!perception.CanSeePlayer) {
            newIntent = EnemyIntent.IDLE;
        } else if(perception.IsInAttackRange) {
            newIntent = EnemyIntent.ATTACK;
        } else {
            newIntent = EnemyIntent.CHASE;
        }

        if(CurrentIntent != newIntent)
            CurrentIntent = newIntent;
    }

}

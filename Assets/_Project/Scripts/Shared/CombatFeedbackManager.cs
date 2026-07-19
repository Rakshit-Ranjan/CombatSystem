using UnityEngine;

public class CombatFeedbackManager : MonoBehaviour {
    

    CombatFeedbackManager Instance;

    void Awake() {
        if(Instance != null || Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PlayHitFeedback(AttackContext context, DamageData data) {
        
    }


}
using System.Collections;
using UnityEngine;

public class CombatFeedbackManager : MonoBehaviour {
    

    public static CombatFeedbackManager Instance {get; private set;}

    void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PlayParryFeedback(AttackContext context, DamageData data) {
        if(context.feedbackData == null) {
            print("no feedback data");
            return;
        }
        PlayHitstopEffect(context.feedbackData.hitStopDuration);
        PlayParryVisualEffect(context, data);
    }

    private void PlayHitstopEffect(float duration) {
        StartCoroutine(HitstopRoutine(duration));
    }

    private void PlayParryVisualEffect(AttackContext context, DamageData data) {
        if(context.feedbackData == null) {
            print("no feedback data");
            return;
        }
        Instantiate(context.feedbackData.hitVFX, data.hitPoint, Quaternion.identity);

    }

    IEnumerator HitstopRoutine(float duration) {
        print(duration);
        float oldScale = Time.timeScale;
        float oldDelta = Time.fixedDeltaTime;
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

    }


}
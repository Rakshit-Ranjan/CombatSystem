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
        if(context.parryFeedbackData == null) {
            print("no feedback data");
            return;
        }
        PlayHitstopEffect(context.parryFeedbackData.hitStopDuration);
        PlayParryVisualEffect(context, data);
    }

    public void PlayHitFeedback(AttackContext ctx, DamageData data) {
        if(ctx.parryFeedbackData == null) return;
        PlayHitstopEffect(ctx.parryFeedbackData.hitStopDuration);
    }

    private void PlayHitstopEffect(float duration) {
        StartCoroutine(HitstopRoutine(duration));
    }

    private void PlayParryVisualEffect(AttackContext context, DamageData data) {
        if(context.parryFeedbackData == null) {
            print("no feedback data");
            return;
        }
        Instantiate(context.parryFeedbackData.hitVFX, data.hitPoint, Quaternion.LookRotation(data.hitNormal));

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
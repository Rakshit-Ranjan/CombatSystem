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
    #region  feedback functions
    public void PlayParryFeedback(AttackContext context, DamageData data) {
        if(context.parryFeedbackData == null) {
            print("no feedback data");
            return;
        }
        PlayHitstopEffect(context.parryFeedbackData.hitStopDuration);
        PlayParryVisualEffect(context, data);
    }

    public void PlayHitFeedback(AttackContext ctx, DamageData data) {
        if(ctx.hitFeedbackData == null) return;
        print("Hitting");
        PlayHitstopEffect(ctx.hitFeedbackData.hitStopDuration);
        PlayHitVFX(ctx, data);
    }

    #endregion


    #region Effects functions

    private void PlayHitstopEffect(float duration) {
        StartCoroutine(HitstopRoutine(duration));
    }

    private void PlayParryVisualEffect(AttackContext context, DamageData data) {
        if(context.parryFeedbackData == null) {
            print("no feedback data");
            return;
        }
        Instantiate(context.parryFeedbackData.hitVFX, data.parryVFXPoint, Quaternion.LookRotation(-context.attackDirection));

    }

    private void PlayHitVFX(AttackContext ctx, DamageData data) {
        if(ctx.hitFeedbackData == null) {
            return;
        }
        Instantiate(ctx.hitFeedbackData.bloodPrefab, data.hitPoint, Quaternion.LookRotation(-ctx.attackDirection), ctx.target);
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
    #endregion

}
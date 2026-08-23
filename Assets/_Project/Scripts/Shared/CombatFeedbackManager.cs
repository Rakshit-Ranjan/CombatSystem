using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CombatFeedbackManager : MonoBehaviour {
    

    public static CombatFeedbackManager Instance {get; private set;}

    [Header("Fatal Hit")]
    [SerializeField] private float fatalHitstopDuration = 0.06f;
    [SerializeField] private float fatalSlowScale = 0.15f;
    [SerializeField] private float fatalSlowDuration = 0.35f;
    [SerializeField] private CinemachineImpulseSource fatalImpulseSource;
    [SerializeField] private float fatalImpulseForce = 1.5f;

    private Coroutine timeScaleRoutine;
    private Coroutine cameraShakeRoutine;
    private float defaultFixedDeltaTime;

    void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
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

    public void PlayFatalHitFeedback(AttackContext ctx, DamageData data) {
        if(ctx.hitFeedbackData != null) {
            PlayHitVFX(ctx, data);
        }

        PlayFatalSlowMotion();

        
        if(fatalImpulseSource != null) {
            Vector3 shakeVel = -ctx.attackDirection.normalized * fatalImpulseForce;
            fatalImpulseSource.GenerateImpulseAtPositionWithVelocity(data.hitPoint, shakeVel);
        }
    }

    #endregion


    #region Effects functions

    private void PlayHitstopEffect(float duration) {
        if(timeScaleRoutine != null) {
            StopCoroutine(timeScaleRoutine);
        }
        timeScaleRoutine = StartCoroutine(HitstopRoutine(duration));
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
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
        timeScaleRoutine = null;

    }

    private void PlayFatalSlowMotion() {
        StartCoroutine(FatalSlowMotionHitRoutine());
    }

    private IEnumerator FatalSlowMotionHitRoutine() {
        
        float defaultTimeDelta = Time.fixedDeltaTime;
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;
        yield return new WaitForSecondsRealtime(fatalHitstopDuration);
        Time.timeScale = fatalSlowScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime* Time.timeScale;
        yield return new WaitForSecondsRealtime(fatalSlowDuration);
        Time.timeScale = 1;
        Time.fixedDeltaTime = defaultFixedDeltaTime;

    }

    
    #endregion

}

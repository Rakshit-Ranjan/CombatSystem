using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackData", menuName = "Attack System/AttackData")]
public class AttackData : ScriptableObject {
    [Header("Basic Info")]
    public string attackName;
    public AnimationClip attackClip;

    [Header("Movement Settings")]
    public MotionGraph motionGraph;
    public AnimationCurve movementCurve;
    public float movementMagnitude = 2f;

    [Header("Damage Settings")]
    public float damage;
    //public float staminaCost;

    [Header("Combo properties")]
    public float comboStartWindow;
    public float comboEndWindow;

    public float GetDuration() {
        if (attackClip != null) {
            return attackClip.length;
        }
        return 0f;
    }

    public bool IsInComboWindow(float currentTime) {
        float duration = GetDuration();
        float progress = currentTime / duration;
        return progress >= comboStartWindow && progress <= comboEndWindow;

    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "MotionGraph", menuName = "Attack System/MotionGraph")]
public class MotionGraph : ScriptableObject {
    [Header("Cumulative Displacement Curves")]
    [Tooltip("Forward (local Z) displacement over normalized time")]
    public AnimationCurve forward = AnimationCurve.Linear(0, 0, 1, 0);

    [Tooltip("Right (local X) displacement over normalized time")]
    public AnimationCurve right = AnimationCurve.Linear(0, 0, 1, 0);

    [Tooltip("Up (local Y) displacement over normalized time")]
    public AnimationCurve up = AnimationCurve.Linear(0, 0, 1, 0);

    [Header("Rotation (Optional)")]
    [Tooltip("Yaw rotation in degrees over normalized time")]
    public AnimationCurve yaw = AnimationCurve.Linear(0, 0, 1, 0);

    [Header("Scaling")]
    [Tooltip("Global scale multiplier for this motion")]
    public float distanceMultiplier = 1f;
}

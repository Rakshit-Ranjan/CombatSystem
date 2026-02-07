using System;
using UnityEngine;

[CreateAssetMenu(menuName ="Attack System/HitReactionData")]
public class HitReactionData : ScriptableObject {
    
    [Header("Reaction Data")]
    public AnimationClip clip;
    public MotionGraph hitReactionGraph;
    public float hitReactionDuraion;

}

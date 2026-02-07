using UnityEngine;
using System;

[CreateAssetMenu(fileName = "DodgeData", menuName = "Attack System/DodgeData")]
public class DodgeData : ScriptableObject {
    [Header("Dodge Settings")]
    public float duration = 0.5f;
    public MotionGraph dodgeGraph;

    public float iFramesStart;
    public float iFramesEnd;

}

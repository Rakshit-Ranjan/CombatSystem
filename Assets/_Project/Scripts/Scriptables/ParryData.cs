using UnityEngine;

[CreateAssetMenu(fileName = "ParryData", menuName = "Attack System/ParryData")]
public class ParryData : ScriptableObject
{
    [Header("Timings")]
    public float startupTime = 0.05f;
    public float activeTime = 0.2f;
    public float recoveryTime = 0.3f;
    [Header("Perfect Parry")]
    public float perfectParryWindow = 0.08f; // Time window for perfect parry
    public bool canParryDuringAttack = false;
}

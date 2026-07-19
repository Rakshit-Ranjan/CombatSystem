using UnityEngine;


[CreateAssetMenu(fileName ="Feedback Data", menuName ="Attack System/")]
public class CombatFeedbackData : ScriptableObject {
    
    [Header("Hit effects parameters")]
    public GameObject hitVFX;
    public AudioClip hitSOund;
    public float hitStopDuration;
    public GameObject weaponSparks;
    public GameObject bloodPrefab;

}


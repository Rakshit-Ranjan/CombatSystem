using UnityEngine;


[CreateAssetMenu(fileName ="Feedback Data", menuName ="Attack System/Combat Feedback Data")]
public class CombatFeedbackData : ScriptableObject {
    
    [Header("Hit effects parameters")]
    public GameObject hitVFX;
    public AudioClip hitSound;
    public float hitStopDuration;
    public GameObject weaponSparks;
    public GameObject bloodPrefab;

}


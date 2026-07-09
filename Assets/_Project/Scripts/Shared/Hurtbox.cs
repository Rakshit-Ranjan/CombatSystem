using UnityEngine;


public class Hurtbox : MonoBehaviour {
    
    public HurtboxType hurtboxType;
    public Transform owner;
    public CombatTeam team;
    public Transform GetOwner() => owner;

}

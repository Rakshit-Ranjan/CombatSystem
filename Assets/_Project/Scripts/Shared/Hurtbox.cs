using UnityEngine;


public class Hurtbox : MonoBehaviour {
    
    public HurtboxType hurtboxType;
    public Transform owner;

    public Transform GetOwner() => owner;

}

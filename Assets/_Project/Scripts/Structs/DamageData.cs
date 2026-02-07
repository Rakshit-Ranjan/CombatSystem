using System;
using UnityEngine;

[Serializable]
public struct DamageData {
    
    public float damage;
    public float stagger;
    public float poiseDamage;
    
    public bool isUnblockable;
    public bool isUnparryable;

    public Vector3 hitPoint;
    public Vector3 hitNormal;
    
    public Transform attacker;

}
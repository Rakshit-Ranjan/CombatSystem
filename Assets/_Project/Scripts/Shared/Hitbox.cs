using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox: MonoBehaviour {
    
    [Header("Owner")]
    [SerializeField] private Transform owner;

    [Header("Runtime")]
    [SerializeField] private AttackData currentAttack;
    [SerializeField] private Collider hitboxCollider;

    private HashSet<IAttackReciever> hitTargets = new();

    void Awake() {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
    }

    public void SetAttackData(AttackData data) {
        currentAttack = data;
    }

    //use in animtion event
    public void EnableHitbox() {
        hitTargets.Clear();
        hitboxCollider.enabled = true;
    }

    public void DisableHitbox() {
        hitboxCollider.enabled = false;
    }


    void OnTriggerEnter(Collider other) {
        
        //dont hit when not attacking
        if(currentAttack == null) 
            return;
        
        if(other.transform == owner) return; // dont hit self

        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        IAttackReciever reciever = hurtbox?.GetOwner().GetComponent<IAttackReciever>();
        if(reciever == null) 
            return;

        if(hitTargets.Contains(reciever)) 
            return;
        hitTargets.Add(reciever);
        AttackContext ctx = new AttackContext {
            attacker = owner,
            attackData = currentAttack,
            attackDirection = owner.forward,
            target = other.transform,
            attackOrigin = owner.position,
            hurtboxType = hurtbox.hurtboxType,
            timeToImpact=0f
        };

        reciever.OnIncomingAttack(ctx);
        hitTargets.Clear();
    }


}
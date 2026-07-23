using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox: MonoBehaviour {
    
    [Header("Owner")]
    [SerializeField] private Transform owner;
    [SerializeField] private CombatTeam team;
    public Transform hitVFXSpawnPoint;
    [Header("Runtime")]
    [SerializeField] private AttackData currentAttack;
    [SerializeField] private Collider hitboxCollider;

    private HashSet<IAttackReciever> hitTargets = new();

    public bool IsActive => hitboxCollider != null && hitboxCollider.enabled;
    public Transform Owner => owner;
    public AttackData CurrentAttack => currentAttack;
    public int HitTargetCount => hitTargets.Count;

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
        if(hurtbox == null) return;
        if(hurtbox.team == team) return;


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
            attackHitPoint = other.bounds.ClosestPoint(transform.position),
            
            attackOrigin = owner.position,
            hurtboxType = hurtbox.hurtboxType,
            timeToImpact=0f,
            parryFeedbackData = currentAttack.parryFeedbackData,
            hitFeedbackData = currentAttack.hitFeedbackData
        };

        reciever.OnIncomingAttack(ctx);
        // hitTargets.Clear(); // REMOVE WHEN YOU PROPERLY SET ANIMATION EVENTS
    }


}

public enum CombatTeam {
    PLAYER, ENEMY
}

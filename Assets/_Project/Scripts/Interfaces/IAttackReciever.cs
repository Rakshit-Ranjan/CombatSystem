using System;
using UnityEngine;

public interface IAttackReciever {

    void OnIncomingAttack(AttackContext ctx);

}
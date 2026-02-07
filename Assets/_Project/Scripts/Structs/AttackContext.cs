using System;
using UnityEngine;

[Serializable]
public struct AttackContext {

    public Transform attacker;
    public Transform target;

    public AttackData attackData;
    public MotionGraph hitGraph;

    public Vector3 attackOrigin;
    public Vector3 attackDirection;

    public HurtboxType hurtboxType;

    public float timeToImpact;

}
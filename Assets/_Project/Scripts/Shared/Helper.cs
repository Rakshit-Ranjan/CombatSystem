using System;
using UnityEngine;

public enum HurtboxType {
    HEAD, LEGS, ARMS, BODY
}

public enum InputActionType {
    LIGHT_ATTACK,
    HEAVY_ATTACK,
    PARRY,
    DODGE
}


public enum HitDirectionType {
    FORWARD, BACK, LEFT, RIGHT
}

public enum EnemyIntent {
    IDLE, CHASE, ENGAGE
} 

public enum CombatState {
    IDLE,
    WINDUP,
    ATTACKING,
    CIRCLING,
    BLOCKING,
    DODGING,
    PARRYING,
    DEAD,
    STUNNED
}

public enum ParryPhase {
    NONE,
    STARTUP,
    ACTIVE,
    RECOVERY
}

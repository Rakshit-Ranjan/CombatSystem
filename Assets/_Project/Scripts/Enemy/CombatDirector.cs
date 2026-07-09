using System.Collections.Generic;
using UnityEngine;


public class CombatDirector : MonoBehaviour {
    

    public List<EnemyController> enemies = new();

    public static CombatDirector Instance {get; private set;}

    public int maxAttackersAtOnce;
    [SerializeField] private int attackers;
    [SerializeField] private float circlingRadius;

    void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool CanAttack(EnemyController enemy) {

        if(attackers < maxAttackersAtOnce) {
            return true;
        } else {
            return false;
        }
    }

    public void NotifyAttackStarted(EnemyController enemy) {
        attackers++;
    }
    public void NotifyAttackEnded(EnemyController enemy) {
        if(attackers <= 0) return;
        attackers--;
    }

    public void RegisterEnemy(EnemyController enemy) {
        if(enemies.Contains(enemy)) return;

        enemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyController enemy) {
        if(enemies.Contains(enemy))
            enemies.Remove(enemy);
    }

    public Vector3 GetSlotPosition(EnemyController enemy) {
        if(enemies == null || enemies.Count == 0) return enemy.playerT.position;
        
        int index = enemies.IndexOf(enemy) + 1;
        float stepAngle = 360f/enemies.Count;

        Vector3 dir = Quaternion.Euler(0, stepAngle * index, 0) * enemy.playerT.forward;
        Vector3 target = enemy.playerT.position + (dir * circlingRadius);
        return target;

    }


}

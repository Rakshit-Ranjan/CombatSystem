using System.Collections.Generic;
using UnityEngine;


public class CombatDirector : MonoBehaviour {


    public List<EnemyController> enemies = new();

    public static CombatDirector Instance { get; private set; }

    public int maxAttackersAtOnce;
    private Vector3 slotBasisForward = Vector3.forward;
    [SerializeField] private int attackers;
    [SerializeField] private float circlingRadius;


    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool CanAttack(EnemyController enemy) {

        if (attackers < maxAttackersAtOnce) {
            return true;
        }
        else {
            return false;
        }
    }

    public void NotifyAttackStarted(EnemyController enemy) {
        attackers++;
    }
    public void NotifyAttackEnded(EnemyController enemy) {
        if (attackers <= 0) return;
        attackers--;
    }

    public void RegisterEnemy(EnemyController enemy) {
        if (enemies.Contains(enemy)) return;

        enemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyController enemy) {
        if (enemies.Contains(enemy))
            enemies.Remove(enemy);
    }

    public Vector3 GetSlotPosition(EnemyController enemy) {
        if (enemies == null || enemies.Count == 0)
            return enemy.playerT.position;

        int index = enemies.IndexOf(enemy);
        if (index < 0)
            return enemy.playerT.position;

        float stepAngle = 360f / enemies.Count;
        Vector3 dir = Quaternion.Euler(0f, stepAngle * index, 0f) * slotBasisForward;
        return enemy.playerT.position + dir * circlingRadius;

    }

    public void RefreshSlotBasis(Transform player) {
        Vector3 forward = player.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.001f)
            slotBasisForward = forward.normalized;
    }


}

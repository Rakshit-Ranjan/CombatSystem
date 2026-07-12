using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CombatDirector : MonoBehaviour {


    public List<EnemyController> enemies = new();
    Dictionary<EnemyController, float> assignedAngles = new();

    public static CombatDirector Instance { get; private set; }

    public int maxAttackersAtOnce;
    private Vector3 slotBasisForward = Vector3.forward;
    [SerializeField] private int attackers;
    [SerializeField] private float circlingRadius;
    float[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };


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
        Debug.Log($"Registering {enemy.name}");
        if (enemies.Contains(enemy) || enemy == null) return;
        enemies.Add(enemy);
        if(!assignedAngles.ContainsKey(enemy)) {
            AssignFreeSlot(enemy);
        }
    }

    public void UnregisterEnemy(EnemyController enemy) {
        Debug.Log($"Unregistering {enemy.name}");
        if(enemies.Count == 0 || enemy == null) return;
        if (enemies.Contains(enemy))
            enemies.Remove(enemy);
        if(assignedAngles.ContainsKey(enemy)) {
            assignedAngles.Remove(enemy);
        }
    }

    public Vector3 GetSlotPosition(EnemyController enemy) {
        if (enemies == null || enemies.Count == 0)
            return enemy.playerT.position;

        if(enemy.playerT == null) return Vector3.zero;
        if(!assignedAngles.ContainsKey(enemy)) return enemy.playerT.position;

        float closestSlotAngle = assignedAngles[enemy];

        Vector3 dir = Quaternion.Euler(0f, closestSlotAngle, 0f) * slotBasisForward;
        return enemy.playerT.position + dir * circlingRadius;

    }

    public void RefreshSlotBasis(Transform player) {
        Vector3 forward = player.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.001f)
            slotBasisForward = forward.normalized;
    }

    public void AssignFreeSlot(EnemyController enemy) {
        if (enemies == null || enemies.Count == 0)
            return;

        Vector3 toEnemy = enemy.transform.position - enemy.playerT.position;
        toEnemy.y = 0;
        toEnemy.Normalize();

        if(toEnemy.sqrMagnitude < 0.001f) return;

        float angleToEnemy = Vector3.SignedAngle(slotBasisForward, toEnemy, Vector3.up);
        if(angleToEnemy < 0f)
            angleToEnemy += 360f;
        
        float[] sortedAngles = angles
                                .OrderBy(i => Mathf.Abs(Mathf.DeltaAngle(i, angleToEnemy)))
                                .ToArray();

        foreach(float angle in sortedAngles) {
            if(!assignedAngles.ContainsValue(angle)) {
                assignedAngles[enemy] = angle;
                return;
            }
        }

        assignedAngles[enemy] = sortedAngles[0];

    }

    


}

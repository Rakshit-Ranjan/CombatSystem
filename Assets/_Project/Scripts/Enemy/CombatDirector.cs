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
    [SerializeField] private float refreshCooldown = 6f;
    [SerializeField] private float yawRefreshThreshold = 35f;
    [SerializeField] private EnemyController focusEnemy;
    [SerializeField] private float focusLockUntilTime;
    [SerializeField] private float stunFocusReleaseDelay;
    [SerializeField] private float maxFocusDistance;

    private float lastRefreshTime;
    private Vector3 lastBasisForward = Vector3.forward;
    float[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };


    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool CanAttack(EnemyController enemy) {
        
        if(focusEnemy == null) {
            SetFocusEnemy(enemy);
            return true;
        } else {
            return focusEnemy == enemy;
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
        if (enemies.Contains(enemy) || enemy == null) return;
        enemies.Add(enemy);
        if (!assignedAngles.ContainsKey(enemy)) {
            AssignFreeSlot(enemy);
        }
    }

    public void SetFocusEnemy(EnemyController enemy) {
        if(enemy == null) return;
        focusEnemy = enemy;
    }

    public void ClearFocusEnemy(EnemyController enemy) {
        if(focusEnemy != enemy) return;
        focusEnemy = null;
    }

    public void OnEnemyHitByPlayer(EnemyController enemy) {
        if(enemy==null) return;
        if(focusEnemy != enemy)
            focusEnemy = enemy;
    }

    public EnemyController GetFocusEnemy() => focusEnemy;

    public bool HasFocus(EnemyController enemy) {
        if(enemy == null) return false;
        return focusEnemy == enemy;
    }

    public void UnregisterEnemy(EnemyController enemy) {
        if (enemies.Count == 0 || enemy == null) return;
        if (enemies.Contains(enemy))
            enemies.Remove(enemy);
        if (assignedAngles.ContainsKey(enemy)) {
            assignedAngles.Remove(enemy);
        }
    }

    public Vector3 GetSlotPosition(EnemyController enemy) {
        if (enemies == null || enemies.Count == 0)
            return enemy.playerT.position;

        if (enemy.playerT == null) return Vector3.zero;
        if (!assignedAngles.ContainsKey(enemy)) return enemy.playerT.position;

        float closestSlotAngle = assignedAngles[enemy];

        Vector3 dir = Quaternion.Euler(0f, closestSlotAngle, 0f) * slotBasisForward;
        return enemy.playerT.position + dir * circlingRadius;

    }

    public void RefreshSlotBasis(Transform player) {
        if(player ==null ) return;

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

        if (toEnemy.sqrMagnitude < 0.001f) return;

        float angleToEnemy = Vector3.SignedAngle(slotBasisForward, toEnemy, Vector3.up);
        if (angleToEnemy < 0f)
            angleToEnemy += 360f;

        float[] sortedAngles = angles
                                .OrderBy(i => Mathf.Abs(Mathf.DeltaAngle(i, angleToEnemy)))
                                .ToArray();

        foreach (float angle in sortedAngles) {
            if (!assignedAngles.ContainsValue(angle)) {
                assignedAngles[enemy] = angle;
                return;
            }
        }

        assignedAngles[enemy] = sortedAngles[0];

    }


    public void TryRefreshSlotBasis(Transform player) {
        if (player == null)
            return;

        if (Time.time - lastRefreshTime < refreshCooldown)
            return;

        Vector3 currentForward = player.forward;
        currentForward.y = 0f;

        if (currentForward.sqrMagnitude < 0.0001f)
            return;

        currentForward.Normalize();

        float yawDelta = Vector3.Angle(lastBasisForward, currentForward);
        if (yawDelta < yawRefreshThreshold)
            return;

        foreach (EnemyController enemy in enemies) {
            if (enemy == null)
                continue;

            EnemyCombatFSM combat = enemy.GetComponent<EnemyCombatFSM>();
            if (combat == null)
                continue;

            if (combat.CurrentState == CombatState.ATTACKING ||
                combat.CurrentState == CombatState.WINDUP) {
                return;
            }
        }

        RefreshSlotBasis(player);
        lastBasisForward = slotBasisForward;
        lastRefreshTime = Time.time;
        print("Changing Basis");
    }



}

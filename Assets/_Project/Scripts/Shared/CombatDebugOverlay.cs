using UnityEngine;

public class CombatDebugOverlay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatFSM playerCombat;
    [SerializeField] private EnemyCombatFSM enemyCombat;
    [SerializeField] private Hitbox playerHitbox;
    [SerializeField] private Hitbox enemyHitbox;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    private InputSystem_Actions inputActions;

    void Awake() {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable() {
        inputActions.Enable();
        inputActions.UI.Debug.performed += ctx => showDebug = !showDebug;
    }

    void OnDisable() {
        inputActions.Disable();
        inputActions.UI.Debug.performed -= ctx => showDebug = !showDebug;
    }
    private void OnGUI()
    {
        if (!showDebug)
            return;

        GUILayout.BeginArea(new Rect(20f, 20f, 380f, 720f), GUI.skin.box);

        GUILayout.Label("PLAYER");
        if (playerCombat != null)
        {
            GUILayout.Label($"State: {playerCombat.CurrentState}");
            GUILayout.Label($"State Timer: {playerCombat.StateTimer:F2}");
            GUILayout.Label($"Parry Timer: {playerCombat.ParryTimer:F2}");
            GUILayout.Label($"Parry Phase: {playerCombat.CurrentParryPhase}");
            GUILayout.Label($"Dodge Timer: {playerCombat.DodgeTimer:F2}");
            GUILayout.Label($"Dodge IFrames: {playerCombat.IsDodgeIFramesActive}");
            GUILayout.Label($"Stunned Timer: {playerCombat.StunnedTimer:F2}");
            GUILayout.Label($"Stunned Move Timer: {playerCombat.StunnedMovingTimer:F2}");
        }
        else
        {
            GUILayout.Label("Player CombatFSM not assigned");
        }

        GUILayout.Space(8f);

        if (playerHitbox != null)
        {
            GUILayout.Label($"Player Hitbox Active: {playerHitbox.IsActive}");
            GUILayout.Label($"Player Current Attack: {(playerHitbox.CurrentAttack != null ? playerHitbox.CurrentAttack.name : "None")}");
            GUILayout.Label($"Player Hit Targets: {playerHitbox.HitTargetCount}");
        }
        else
        {
            GUILayout.Label("Player Hitbox not assigned");
        }

        GUILayout.Space(16f);

        GUILayout.Label("ENEMY");
        if (enemyCombat != null)
        {
            GUILayout.Label($"Combat State: {enemyCombat.CurrentState}");
            GUILayout.Label($"State Timer: {enemyCombat.StateTimer:F2}");
            GUILayout.Label($"Attack Cooldown Timer: {enemyCombat.AttackTimer:F2}");
            GUILayout.Label($"Stunned Timer: {enemyCombat.StunnedStateTimer:F2}");
            GUILayout.Label($"Stunned Move Timer: {enemyCombat.StunnedStateMovingTimer:F2}");
            GUILayout.Label($"Blocks Locomotion: {enemyCombat.IsBlockingLocomotion}");
            GUILayout.Label($"Combo Index: {enemyCombat.ComboIndex}");
        }
        else
        {
            GUILayout.Label("Enemy CombatFSM not assigned");
        }

        GUILayout.Space(8f);

        if (enemyHitbox != null)
        {
            GUILayout.Label($"Enemy Hitbox Active: {enemyHitbox.IsActive}");
            GUILayout.Label($"Enemy Current Attack: {(enemyHitbox.CurrentAttack != null ? enemyHitbox.CurrentAttack.name : "None")}");
            GUILayout.Label($"Enemy Hit Targets: {enemyHitbox.HitTargetCount}");
        }
        else
        {
            GUILayout.Label("Enemy Hitbox not assigned");
        }

        GUILayout.EndArea();
    }
}
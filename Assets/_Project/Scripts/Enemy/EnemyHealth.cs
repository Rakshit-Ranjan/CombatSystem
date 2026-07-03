using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable {
    
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;
    public float HealthNormalized => maxHealth > 0f ? currentHealth / maxHealth : 0f;

    private void Awake() {
        currentHealth = maxHealth;
    }

    public void TakeDamage(DamageData data) {
        if (IsDead)
            return;

        currentHealth -= data.damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"Enemy took {data.damage} damage from {data.attacker?.name}. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f) {
            Die();
        }
    }

    public void Heal(float amount) {
        if (IsDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    private void Die() {
        Debug.Log("Player died");
    }

}

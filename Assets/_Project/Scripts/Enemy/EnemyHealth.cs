using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable {

    public float health;

    void Start() {

    }


    void Update() {

    }

    public void TakeDamage(DamageData data) {
        health -= data.damage;
    } 

}

using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable {

    public float health;

    void Start() {

    }


    void Update() {

    }

    public void TakeDamage(DamageData data) {
        health -= data.damage;
        Debug.Log($"{transform.name} got hit by {data.attacker}. \n Health is now {health}");
    } 

}

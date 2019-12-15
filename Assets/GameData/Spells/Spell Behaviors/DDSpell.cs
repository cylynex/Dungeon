using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDSpell : MonoBehaviour {

    [Header("Attributes")]
    public Spell currentSpell;
    public string spellName;
    public float spellMoveSpeed;
    public float spellExplosionRadius;
    public int spellDamage;
    public GameObject spellImpactEffect;

    [Header("Internal Only")]
    public GameObject targetGO;
    public Transform target;

    void Start() {
        spellName = currentSpell.spellName;
        spellMoveSpeed = currentSpell.spellMoveSpeed;
        spellExplosionRadius = currentSpell.spellExplosionRadius;

        // Add any bonuses or whatever and figure out how much damage this spell will do.
        spellDamage = CalculateSpellDamage();

        spellImpactEffect = currentSpell.spellImpactEffect;

        targetGO = Player.target;
        target = targetGO.transform;
        Debug.Log("my target as a missile is " + targetGO.name);
        Seek(target);

    }


    // Calculate Spell Damage
    int CalculateSpellDamage() {
        spellDamage = currentSpell.spellDamage + Player.character.spellDamageBonus;
        return spellDamage;
    }

    void Update() {
        if (target == null) {
            Destroy(gameObject);
            return;
        }

        // Make it mooooove
        Vector3 direction = target.position - transform.position;
        float distanceThisFrame = spellMoveSpeed * Time.deltaTime;
        if (direction.magnitude <= distanceThisFrame) {
            // HIT!
            HitTarget();
            return;
        }

        // Didn't hit, move along
        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);

    }


    public void Seek(Transform _target) {
        Debug.Log("setting target to " + _target);
        target = _target;
    }


    // Hit TODO add hooks for dmg bonuses or shit
    void HitTarget() {
        GameObject effectInstance = (GameObject)Instantiate(spellImpactEffect, transform.position, transform.rotation);
        Destroy(effectInstance, 0.5f);
        Destroy(gameObject);

        // Explosion
        if (spellExplosionRadius > 0f) {
            Explode();
        } else {
            Damage(target);
            DungeonController.instance.UpdateActionLog("<color=#0000ee>You hit mob for " +spellDamage+"</color>");
            //TODO move this
            //targetGO.GetComponent<MonsterBase>().UpdateTargetHPBar();
            DungeonController.instance.UpdateTargetHPBar();
        }
    }

    // AE Damage
    void Explode() {
        Collider[] colliders = Physics.OverlapSphere(transform.position, spellExplosionRadius);
        foreach (Collider collider in colliders) {
            if (collider.tag == "Enemy") {
                // Its an enemy - destroy it
                Damage(collider.transform);
            } else {
                // Not an enemy
            }
        }

    }

    void Damage(Transform enemy) {
        MonsterBase e = enemy.GetComponent<MonsterBase>();
        e.TakeDamage(spellDamage);
    }


    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spellExplosionRadius);
    }
}

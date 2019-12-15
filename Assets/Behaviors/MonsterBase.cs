using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterBase : MonoBehaviour {

    public Monster monsterObject;
    public GameObject spawnPoint;
    public Image hpBar;
    [SerializeField]
    public float hitPoints;
    float aggroRange;

    public GameObject target = null;

    DungeonController dungeonController;

    // Drops
    Item dropItem;
    bool dropComplete = false;

    // Timers
    float attackTimer;

	// Use this for initialization
	void Start () {

        dungeonController = GameObject.FindGameObjectWithTag("DungeonController").GetComponent<DungeonController>();

        // Set Image
        GetComponent<SpriteRenderer>().sprite = monsterObject.monsterImage;

        // Set Scale
        transform.localScale = monsterObject.monsterScale;

        // Add collider
        BoxCollider2D bc = gameObject.AddComponent<BoxCollider2D>();
        //BoxCollider bc = gameObject.AddComponent<BoxCollider>();

        // Establish HitPoints
        hitPoints = monsterObject.hitPoints;
        UpdateHPBar();

        // Aggro range
        aggroRange = monsterObject.aggroRange;

        // Attack Stuff
        attackTimer = monsterObject.attackTime;

	}


    // Update-Ho
    void Update() {
        if (target == null) { CheckAggro(); }
        if (target != null) { Attack(); }
    }


    // Check aggro radius
    void CheckAggro() {
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, monsterObject.aggroRange);

        // Loop through and find a target.  Once target is set you can stop doing this.
        foreach (Collider2D objs in objectsInRange) {

            if (objs.tag == "Player") {
                target = objs.gameObject;
            }
        }
    }


    private void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("hit me");
    }


    // The baseline "attack".  It should incorporate melee vs range and handle hooks from subclass.  IN fact it should probably be
    // in the subclass not here, but i didnt write it yet so this will do for now.  TODO all that bullshit i just said.
    void Attack() {
        // Determine if we are in attack range
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > monsterObject.range) {
            // Not close enough - get closer
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, monsterObject.speed * Time.deltaTime);
        } else {
            // Close enough - attack.
            ExecuteAttack();
        }

        // TODO add another else here for leash range, add to monster class as variable

    }


    // The actual attack
    void ExecuteAttack() {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0) {
            target.gameObject.GetComponent<Player>().TakeDamage(monsterObject.damage);
            attackTimer = monsterObject.attackTime;
        }
    }


    // Take Damage
    public void TakeDamage(float amount) {
        hitPoints -= amount;
        if (hitPoints <= 0) {
            hitPoints = 0;
            Die();
        }
        UpdateHPBar();

        // Target back the player if mob took damage and wasn't aggro prior.
        if (target == null) {
            target = GameObject.FindGameObjectWithTag("Player");
        }
    }


    // Update the HP Bar
    public void UpdateHPBar() {
        // Get % hp remaining
        float percentHitPoints = (hitPoints / monsterObject.hitPoints);
        hpBar.fillAmount = percentHitPoints;
    }


    // Monster has died.  Report in to dungeon controller to be counted and eliminate him.
    void Die() {
        Debug.Log("monster has died.  Notify dungeon controller");
        dungeonController.MobHasDied();

        // Gold Drop
        int goldDropped = Random.Range(monsterObject.goldLow, monsterObject.goldHigh);
        dungeonController.AddGoldToLoot(goldDropped);

        // Loot Drop.
        int dropWeight = Random.Range(1, 100);

        // Figure out the drop
        if (!dropComplete) {
            for (int i = 0; i < monsterObject.lootTable.dropChance.Length; i++) {

                // Only do this again if we haven't found the drop yet.
                if (dropComplete == false) {
                    if (dropWeight < monsterObject.lootTable.dropChance[i]) {
                        dropItem = monsterObject.lootTable.drops[i];
                        dropComplete = true;
                    }
                }
            }
        }

        // Add the loot if its not nothing
        if (dropItem.itemName != "Nothing") {
            dungeonController.AddLoot(dropItem, 1); // TODO dynamic quantity esp for like crafting shit is probably a good idea.
        } else {
            Debug.Log("Nothing dropped");
        }

        // Add xp to the pool AND to the player TODO figure out which system we want to go with.
        dungeonController.lootXP += monsterObject.xpPerKill;
        dungeonController.GivePlayerExperience(monsterObject.xpPerKill);

        // Log stuff
        dungeonController.UpdateActionLog("<color=#ff9900>You have slain "+monsterObject.monsterName+"</color>");
        dungeonController.UpdateActionLog("<color=#fff002>You have gained experience! (" + monsterObject.xpPerKill + ")</color>");

        Debug.Log("now destroy it");
        Destroy(gameObject);
    }


    // gizmos to see range for Stuff // TODO move to somewhere else
    void OnDrawGizmosSelected() {
        // Aggro Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        // Attack Range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, monsterObject.range);
    }
	
}

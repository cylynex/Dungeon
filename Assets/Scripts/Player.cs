using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour {

    public static Character character;

    [Header("Defense")]
    public static float hitPoints;

    // TODO add modifiers for attacktimer from both spells and items
    [SerializeField]
    float attackTimer;
    Vector2 targetPos;
    bool isMoving;

    GameObject gameManager;

    // Statistics Stuff  TODO move to its own class 
    public static int deaths;

    // Targeting stuff
    public static GameObject target;
    public Transform selectedTarget;
    RaycastHit hit;
    Ray2D ray;

    // Zone Stuff - TODO this should probably be moved to the static GM makes more sense
    public static Zone currentZone;
    public static Dungeon currentDungeon;
    public static Camp currentCamp;
    public DungeonController dungeonController;

    // Casting Bars TODO move to barController class
    public Image castBar;
    public Text castBarText;


    void Awake() {
        // Bar stuff TODO move to barcontroller
        castBar = GameObject.FindGameObjectWithTag("castBar").GetComponent<Image>();
        castBarText = GameObject.FindGameObjectWithTag("castBarText").GetComponent<Text>();
    }


    void Start() {

        // Init Attack timer for dungeon
        if (Player.character.weaponPrimaryEquipped) {
            attackTimer = Player.character.attackTime / 10;
        } else {
            attackTimer = character.attackTime / 10;
        }

        // Init HP - TODO this will need to be augmented with gear and stats that change the HP to be used properly.  Lots more math.  Awesome.
        hitPoints = character.modifiedHitPoints;

        // Init target.
        targetPos = transform.position;

        // Connect with dungeon controller
        dungeonController = GameObject.FindGameObjectWithTag("DungeonController").GetComponent<DungeonController>();

        // Game Manager
        gameManager = GameObject.FindGameObjectWithTag("GameController");

        // Bar Updates
        BarController.instance.UpdateHPBar();
        BarController.instance.UpdateXPBar();

    }

    void Update() {
        CheckTargetInput();
        CheckMoveInput();
        if (isMoving) {
            Move();
            ResetAttackTimer();
        }
        if (selectedTarget == null) {
            Debug.Log("No target");
            ResetAttackTimer();
            //GetTarget();
        } else {
            Debug.Log("target found to attack");
            Attack();
        }
    }


    // Reset attack timer if moving, cause you cant cast while moving!
    void ResetAttackTimer() {
        if (Player.character.weaponPrimaryEquipped) {
            attackTimer = (Player.character.weaponDelay / 10);
        } else {
            attackTimer = character.attackTime / 10;
        }
        castBar.fillAmount = 0;
    }


    // Click
    void CheckTargetInput() {

        target = null;

        if (Input.GetMouseButtonDown(0)) {

            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector3.zero);
            if (hit.collider != null) {
                Debug.Log("its a hit");
                if (hit.collider.tag == "Enemy") {
                    selectedTarget = hit.transform;
                    target = hit.transform.gameObject;
					Debug.Log("target: " + target.GetComponent<MonsterBase>().monsterObject.monsterName);
                    DeselectTarget();
                    SelectTarget();
                } else {
                    Debug.Log("jack shit");
                }
            } 

            //if (!gotTarget) DeselectTarget(); //If we missed everything, deselect

        }
    }


    // Select the target
    private void SelectTarget() {
        selectedTarget.GetComponent<SpriteRenderer>().color = Color.cyan;
        DungeonController.instance.SetTargetBar(selectedTarget.gameObject);
    }


    // Deselect
    private void DeselectTarget() {
        if (target != null) {
            Debug.Log("starting deselect part");
            selectedTarget.GetComponent<SpriteRenderer>().color = Color.red;
            target = null;
        } else {
            Debug.Log("couldnt find anythinig to deselect");
        }
    }


    // Move
    void CheckMoveInput() {
        if (Input.GetMouseButtonDown(1)) {
            if (!EventSystem.current.IsPointerOverGameObject()) {
                Vector2 clickedPos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
                targetPos = clickedPos;
                isMoving = true;
            }
        }
    }


    // Gain Experience
    public void AddExperience(float amount) {
        character.currentExperience += amount;
        if (character.currentExperience >= character.expForNextLevel) {
            character.currentExperience = character.expForNextLevel;
        }

        // Check for level up
        if (character.currentExperience > character.expForNextLevel) {
            Debug.Log("level up SHOULD HAPPEN HERE");



        }

        BarController.instance.UpdateXPBar();

    }


    // Move
    void Move() {
        if ((Vector2)transform.position != targetPos) {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, character.speed * Time.deltaTime);
        } else {
            isMoving = false;
            targetPos = transform.position;
        }
    }


    // Check for target mob in radius TODO allow for manual target selection in the case of multiple mobs in range.  Raycast 4tw
    void GetTarget() {
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, character.attackRange);

        // Loop through and find a target.  Once target is set you can stop doing this.
        foreach (Collider2D objs in objectsInRange) {

            if (objs.tag == "Enemy") {
                target = objs.gameObject;
                DungeonController.instance.SetTargetBar(target);
            }
        }

        if (target != null) {
            dungeonController.UpdateActionLog("<color=#00ff00>Targeted mob: " + target.name + "</color>");
        }

    }


    // Baseline attack
    void Attack() {
        // Determine if we are in attack range
        float distance = Vector3.Distance(transform.position, selectedTarget.position);
        if (distance > character.attackRange) {
            // Not close enough - get closer
            Debug.Log("not close enough");
        } else {
            // Close enough - attack.
            Debug.Log("close enough - attack");
            ExecuteAttack();
        }
    }


    // The actual attack TODO dynamic damage
    void ExecuteAttack() {
        attackTimer -= Time.deltaTime;
        float percentCast = attackTimer / character.attackTime;
        castBar.fillAmount = percentCast * 10;

        if (attackTimer <= 0) {

            if (selectedTarget != null) {
                switch (character.baseClass) {
                    case "Warrior": Debug.Log("warrior: melee attack"); MeleeAttack(); break;
                    case "Wizard": CastOffensiveSpell(); break;
                    case "Cleric": ClericAI(); break;
                }
            }

            attackTimer = character.attackTime / 10;
            DungeonController.instance.UpdateTargetHPBar();

        }
    }


    // Player hit something
    void OnCollisionEnter2D(Collision2D collision) {
        targetPos = transform.position;
    }



    // Cast Nuke Spell (for caster classes)
    // TODO select a random spell or something from their spell list instead of using hardcoded spell slot 0
    void CastOffensiveSpell() {
        Debug.Log("going to cast a spell: " + GameManager.spellbooks[Player.character.characterName].spell[0].spellName);
        // Pick a random spell from the list of prepared spells
        int maxSpellSlot = GameManager.preparedSpells.Count;
        int spellSlot = Random.Range(0, maxSpellSlot);

        //GameObject thisSpell = (GameObject)Instantiate(GameManager.spellBook[0].spellPrefab, transform.position, Quaternion.identity);
        GameObject thisSpell = (GameObject)Instantiate(GameManager.spellbooks[Player.character.characterName].spell[0].spellPrefab, transform.position, Quaternion.identity);
    }


    // Cast Heal Spell (for healers)
    // TODO select a random spell or something from their spell list instead of using hardcoded spell slot 0
    // TODO instantiate the heal effect
    // TODO differentiate between healing self and healing other, even tho there is no "other" yet
    void CastHealingSpell() {
        Debug.Log("going to cast a spell: " + GameManager.spellbooks[Player.character.characterName].spell[0].spellName);
        //GameObject thisSpell = (GameObject)Instantiate(GameManager.spellbooks[Player.character.characterName].spell[0].spellPrefab, transform.position, Quaternion.identity);
        int amountToHeal = GameManager.spellbooks[Player.character.characterName].spell[0].amountToHeal;
        hitPoints += amountToHeal;
        BarController.instance.UpdateHPBar();
        dungeonController.UpdateActionLog("<color=#eeeeff>Healed yourself for " + amountToHeal + "</color>");
    }


    // Melee Attack
    void MeleeAttack() {
        Debug.Log("MELEE: should hit for : " + character.meleeDamage);
        selectedTarget.gameObject.GetComponent<MonsterBase>().TakeDamage(character.meleeDamage);
        dungeonController.UpdateActionLog("<color=#0000ee>You hit the mob for " + character.meleeDamage + " damage</color> <color=#ff0000>(" + character.damage + ")</color>");
    }


    // Cleric Stuff
    void ClericAI() {
        float hppercent = hitPoints / character.modifiedHitPoints;
        if (hppercent <= .5f) {
            Debug.Log("I'm under 50% hp i should heal myself");
            dungeonController.UpdateActionLog("<color=#3333ff>Healing myself!</color>");
            CastHealingSpell();
        } else {
            Debug.Log("I'm over 50% hp i can melee");
            MeleeAttack();
        }
    }


    // Take Damage
    public void TakeDamage(int amount) {
        hitPoints -= amount;
        if (hitPoints < 0) { hitPoints = 0; }
        BarController.instance.UpdateHPBar();

        dungeonController.UpdateActionLog("<color=#ff0000>Mob hit you for " + amount + " damage</color>");

        if (hitPoints <= 0) {
            // Player died.
            dungeonController.Lose();
            Statistics.playerStatistics["Player Deaths"] += 1;
            // Save the game
            gameManager.GetComponent<Save>().SaveGame();
            Destroy(gameObject);

        }
    }


    // gizmos to see range for Stuff // TODO move to somewhere else
    void OnDrawGizmosSelected() {
        // Attack Range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, character.attackRange);
    }
}

using UnityEngine;
using System.Collections;

public class Zombie : MonoBehaviour {

	public float speed;
	public float rotationSpeed;

	public GameObject healthbar_bg;
	public GameObject healthbar_fg;
	public GameObject feet;
	public GameObject midSection;
	public GameObject head;
	public GameObject midSection_attack;
	public GameObject head_attack;
	public float healthbar_y_offset = 0.75f;

	private GameObject player;
	private GameObject target;

	private float maxHealth = 100;
	private float health = 100;
	private float damageScalar = 0.3f;
	private int attackFrames = 30;
	private int attackFramesCounter = 0;
	private bool attacking = false;
	private float attackCooldown = 1; //s
	private float attackCooldownTimer = 0;

	private int numHits = 0;

	private Rigidbody2D body;

	// Use this for initialization
	void Start () {
		player = GameObject.Find ("Player");
		body = GetComponent<Rigidbody2D> ();
		handleMovement();
		handleHealthBarUpdates();
	}
	
	// Update is called once per frame
	void Update () {
		if (GameMaster.isPlayState ()) {
			handleMovement();
			handleAttack ();
			handleHealthBarUpdates();
		}
	}

	private void handleMovement(){
		if (health <= 0) {
			kill ();
		}
		
		if (numHits > 1) {
			target = player;
		} else {
			findNearestFriend ();
		}
		
		if (target != null) {
			Vector3 diff = target.transform.position - transform.position;
			float toAngle = Mathf.Atan2 (diff.y, diff.x) * Mathf.Rad2Deg;
			if (toAngle < 0)
				toAngle += 360;
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.AngleAxis (toAngle, Vector3.forward), rotationSpeed);
			
			body.AddForce (transform.right * speed);
		}
	}

	private void handleAttack(){
		if (attackFramesCounter > attackFrames) {
			attacking = false;
			attackFramesCounter = 0;
			midSection_attack.SetActive (false);
			head_attack.SetActive (false);
			midSection.SetActive (true);
			head.SetActive (true);
		}

		if (attacking) {
			attackFramesCounter++;
		} else {
			if (target != null){
				float distanceToTarget = Vector3.Distance (transform.position, target.transform.position);
				if (attackCooldownTimer <= 0 && distanceToTarget < 2) {
					attackCooldownTimer = 0;
					attack ();
				} else {
					attackCooldownTimer -= Time.deltaTime;
				}
			}
		}
	}

	private void attack(){
		attacking = true;
		attackCooldownTimer = attackCooldown;
		midSection_attack.SetActive (true);
		head_attack.SetActive (true);
		midSection.SetActive (false);
		head.SetActive (false);

		Collider2D[] colliders;
		Vector2 attackPoint = new Vector2 ();
		Vector3 attackPoint3 = transform.position + (transform.right * 0.75f);
		attackPoint.x = attackPoint3.x;
		attackPoint.y = attackPoint3.y;
		//transform.position = attackPoint3;
		if((colliders = Physics2D.OverlapCircleAll(attackPoint, 0.5f)).Length > 1) {
			foreach(var collider in colliders) {
				if (collider.transform.GetComponent<Player>() != null){
					Player player = (Player) collider.transform.GetComponent<Player>();
					player.induceRage (10);
				}
				if (collider.transform.GetComponent<Civilian>() != null){
					Civilian civilian = (Civilian) collider.transform.GetComponent<Civilian>();
					civilian.damage (50);
				}
			}
		}
	}
	
	private void handleHealthBarUpdates(){
		healthbar_bg.transform.rotation = Quaternion.identity;
		healthbar_fg.transform.rotation = Quaternion.identity;
		
		float percentHealth = (float)health / maxHealth;
		Vector3 scale = healthbar_fg.transform.localScale;
		scale.x = percentHealth;
		healthbar_fg.transform.localScale = scale;
		float x_offset = -0.5f + percentHealth / 2;
		
		Vector3 healthbar_bg_pos = new Vector3 (transform.position.x, transform.position.y + healthbar_y_offset, transform.position.z);
		Vector3 healthbar_fg_pos = new Vector3 (transform.position.x + x_offset, transform.position.y + healthbar_y_offset, transform.position.z - 0.1f);
		
		healthbar_bg.transform.position = healthbar_bg_pos;
		healthbar_fg.transform.position = healthbar_fg_pos;
	}
	
	private void findNearestFriend(){
		GameObject[] friends = GameObject.FindGameObjectsWithTag ("Friend") as GameObject[];
		if (friends != null && friends.Length > 0) {
			foreach (GameObject friend in friends) {
				if (target == null) {
					target = friend;
				} else {
					float currLen = Vector3.Distance (transform.position, target.transform.position);
					float newLen = Vector3.Distance (transform.position, friend.transform.position);
					if (newLen < currLen) {
						target = friend;
					}
				}
			}
		} else {
			target = null;
		}
	}

	private void kill(){
		GameMaster.createBloodSpatter(transform.position, 4);
		GameMaster.shakeCamera(0.2f);
		GameMaster.decrementEnemyCount ();
		Destroy (gameObject);
	}

	private void damage(float damage){
		health -= (damage * damageScalar);
		GameMaster.shakeCamera(damage/1000);
		if (Random.Range (0, 2) == 1 && damage > 20) {
			GameMaster.createBloodSpatter(transform.position, damage/25);
		}
	}

	void OnCollisionEnter2D (Collision2D collision) {
		if(collision.gameObject.GetComponent<ForceDamage>() != null) {
			float dmg = Mathf.Sqrt (Vector2.SqrMagnitude(collision.relativeVelocity));
			if (dmg > 10){
				damage (dmg);
				numHits++;
			}
		}
	}
}

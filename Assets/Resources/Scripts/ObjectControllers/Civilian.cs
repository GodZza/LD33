using UnityEngine;
using System.Collections;

public class Civilian : MonoBehaviour {
	
	public float speed;
	public float rotationSpeed;
	
	public GameObject healthbar_bg;
	public GameObject healthbar_fg;
	public float healthbar_y_offset = 0.75f;
	
	private GameObject nearestThreat;
	
	private float maxHealth = 100;
	private float health = 100;
	private float damageScalar = 0.3f;
	
	private Rigidbody2D body;

	private float randomAngleModifier = 0;
	
	// Use this for initialization
	void Start () {
		body = GetComponent<Rigidbody2D> ();
		handleMovement();
		handleHealthBarUpdates();
	}
	
	// Update is called once per frame
	void Update () {
		handleMovement();
		handleHealthBarUpdates();
	}

	private void handleMovement(){
		if (health <= 0) {
			kill ();
		}
		
		findNearestThreat ();
		
		if (Random.Range (0, 100) == 0) {
			randomAngleModifier = Random.Range (-120, 120);
		} else if (Random.Range (0, 100) == 0) {
			randomAngleModifier = 0;
		}
		
		if (nearestThreat != null) {
			Vector3 diff = nearestThreat.transform.position - transform.position;
			float toAngle = (Mathf.Atan2 (diff.y, diff.x) * Mathf.Rad2Deg) + 180 + randomAngleModifier;
			if (toAngle < 0)
				toAngle += 360;
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.AngleAxis (toAngle, Vector3.forward), rotationSpeed);
			
			body.AddForce (transform.right * speed);
		} else {
			float toAngle = randomAngleModifier;
			if (toAngle < 0)
				toAngle += 360;
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.AngleAxis (toAngle, Vector3.forward), rotationSpeed);
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
	
	private void kill(){
		GameMaster.createZombie (transform.position);
		GameMaster.createBloodSpatter(transform.position, 2);
		GameMaster.decrementFriendlyCount ();
		Destroy (gameObject);
	}
	
	public void damage(float damage){
		health -= (damage * damageScalar);
		if (Random.Range (0, 2) == 1 && damage > 20) {
			GameMaster.createBloodSpatter(transform.position, damage/25);
		}
	}

	private void findNearestThreat(){
		GameObject[] threats = GameObject.FindGameObjectsWithTag ("Threat") as GameObject[];
		if (threats != null && threats.Length > 0) {
			foreach (GameObject threat in threats) {
				if (nearestThreat == null) {
					nearestThreat = threat;
				} else {
					float currLen = Vector3.Distance (transform.position, nearestThreat.transform.position);
					float newLen = Vector3.Distance (transform.position, threat.transform.position);
					if (newLen < currLen) {
						nearestThreat = threat;
					}
				}
			}
		} else {
			nearestThreat = null;
		}
	}
	
	void OnCollisionEnter2D (Collision2D collision) {
		if(collision.gameObject.GetComponent<ForceDamage>() != null) {
			float dmg = Mathf.Sqrt (Vector2.SqrMagnitude(collision.relativeVelocity));
			if (dmg > 10){
				damage (dmg);
			}
		}
	}


}

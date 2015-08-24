using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	enum PlayerState {Lucid, Raging}

	public bool invincible = false;
	public float moveSpeedBase;
	public float moveSpeedScalar;
	public float rotationSpeed;
	public float attackStrengthBase;
	public float attackStrengthScalar;

	public GameObject rageOverlay;
	private RectTransform rageMeter;
	private RectTransform moraleMeter;
	
	public float attackCooldown = 0.01f; //s

	private PlayerState playerState = PlayerState.Lucid;
	private float rage = 0;
	private int rageTier = 0; //0-0, 1-20, 2-40, 3-60, 4-80
	private float morale = 100;
	private float attackCooldownTimer = 0;

	private Rigidbody2D body;
	private Rigidbody2D leftArmBody;
	private Rigidbody2D rightArmBody;

	public GameObject leftArm;
	public GameObject rightArm;

	public GameObject target;

	private float randomAngleModifier = 0;
	
	// Use this for initialization
	void Start () {
		body = GetComponent<Rigidbody2D> ();
		leftArmBody = leftArm.GetComponent<Rigidbody2D> ();
		rightArmBody = rightArm.GetComponent<Rigidbody2D> ();
		rageMeter = GameObject.Find ("RageMeter").GetComponent<RectTransform> () as RectTransform;
		moraleMeter = GameObject.Find("MoraleMeter").GetComponent<RectTransform> () as RectTransform;
	}
	
	// Update is called once per frame
	void Update () {
		morale -= 0.02f;
		if (morale <= 0)
			kill ();
		if (playerState == PlayerState.Lucid || !GameMaster.isPlayState ()) {
			rageOverlay.SetActive (false);
			handleMovement ();
			handleAttack ();
			handleRageUpdates ();

			if (rage > 20 && GameMaster.isPlayState ()){
				if (Random.Range (0, 20000) < rage){
					playerState = PlayerState.Raging;
				}
			}
		} else if (playerState == PlayerState.Raging) {
			rageOverlay.SetActive (true);
			findNearestFriend();
			handleMovementRage();
			handleAttackRage();
			handleRageUpdates ();

			if (rage > 20){
				if (Random.Range (0, 150) == 0){
					playerState = PlayerState.Lucid;
				}
			} else {
				playerState = PlayerState.Lucid;
			}
		}
		moraleMeter.sizeDelta = new Vector2 ((morale/100) * 320, moraleMeter.sizeDelta.y);
	}

	private void handleAttackRage(){
		if (target != null) {
			float distanceToTarget = Vector3.Distance (transform.position, target.transform.position);
			if (Random.Range (0, 20) == 0 && distanceToTarget < 4) {
				attack (Random.Range (0, 2));
			}
		}
	}

	private void handleMovementRage(){
		if (target != null) {

			if (Random.Range (0, 80) == 0) {
				randomAngleModifier = Random.Range (-60, 60);
			} else if (Random.Range (0, 80) == 0) {
				randomAngleModifier = 0;
			}

			Vector3 diff = target.transform.position - transform.position;
			float toAngle = Mathf.Atan2 (diff.y, diff.x) * Mathf.Rad2Deg + randomAngleModifier;
			if (toAngle < 0)
				toAngle += 360;

			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.AngleAxis (toAngle, Vector3.forward), rotationSpeed);
			
			body.AddForce (transform.right * (moveSpeedBase + (moveSpeedScalar * (rageTier + 1))));
		} else {
			if (Random.Range (0, 80) == 0 || randomAngleModifier == 0) {
				randomAngleModifier = Random.Range (-180, 180);
			}
			float toAngle = randomAngleModifier;
			if (toAngle < 0)
				toAngle += 360;
			
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.AngleAxis (toAngle, Vector3.forward), rotationSpeed);
			
			body.AddForce (transform.right * (moveSpeedBase + (moveSpeedScalar * (rageTier + 1))));
		}
	}

	private void handleMovement(){
		if (Input.GetKey ("d")) {
			body.AddForce(Vector2.right * (moveSpeedBase + (moveSpeedScalar * (rageTier + 1))));
		}
		if (Input.GetKey ("a")) {
			body.AddForce(Vector2.right * -(moveSpeedBase + (moveSpeedScalar * (rageTier + 1))));
		}
		if (Input.GetKey ("w")) {
			body.AddForce(Vector2.up * (moveSpeedBase + (moveSpeedScalar * (rageTier + 1))));
		}
		if (Input.GetKey ("s")) {
			body.AddForce(Vector2.up * -(moveSpeedBase + (moveSpeedScalar * (rageTier + 1))));
		}
		
		Vector3 worldPos = Utils.GetWorldPositionOnPlane(Input.mousePosition, 1); //perspective
		//Vector3 worldPos = Camera.main.ScreenToWorldPoint (Input.mousePosition); //ortho
		Vector3 diff = worldPos - transform.position;
		float angle = Mathf.Atan2 (diff.y, diff.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis (angle, Vector3.forward), rotationSpeed);
	}

	private void handleAttack(){
		if (attackCooldownTimer <= 0){
			attackCooldownTimer = 0;
			if (Input.GetMouseButtonDown (0)) {
				attack (0);
			}
			
			if (Input.GetMouseButtonDown (1)){
				attack (1);
			}
		} else {
			attackCooldownTimer -= Time.deltaTime;
		}
	}

	private void handleRageUpdates(){
		if (rage > 80) {
			setTier(4);
			transform.tag = "Threat";
		} else if (rage > 60) {
			setTier(3);
			transform.tag = "Threat";
		} else if (rage > 40) {
			setTier(2);
			transform.tag = "Threat";
		} else if (rage > 20) {
			setTier(1);
			transform.tag = "Friend";
		} else {
			setTier(0);
			transform.tag = "Friend";
		}
		if (rage > 0) {
			if (GameMaster.isPlayState()){
				rage-= 0.02f;
			} else {
				rage-= 0.05f;
			}
		} else {
			rage = 0;
		}
		rageMeter.sizeDelta = new Vector2 ((rage/100) * 320, rageMeter.sizeDelta.y);
	}

	private void setTier(int tier){
		//update the arm references
		if (rageTier != tier) {
			rageTier = tier;

			for (int i = 0; i <= 4; i++) {
				string tierNameTemp = "tier"+i;
				GameObject limbsTemp = transform.Find(tierNameTemp).gameObject;
				if (i == tier){
					limbsTemp.SetActive(true);
				} else {
					limbsTemp.SetActive(false);
				}
			}

			string tierName = "tier"+rageTier;
			GameObject limbs = transform.Find(tierName).gameObject;
			leftArmBody = limbs.transform.Find ("leftArm").GetComponent<Rigidbody2D>();
			rightArmBody = limbs.transform.Find ("rightArm").GetComponent<Rigidbody2D>();
		}
	}

	private void attack(int side){
		attackCooldownTimer = attackCooldown;
		if (side == 0) {
			leftArmBody.AddForce(transform.right * (attackStrengthBase + (attackStrengthScalar * (rageTier + 1))));
		} else {
			rightArmBody.AddForce(transform.right * (attackStrengthBase + (attackStrengthScalar * (rageTier + 1))));
		}
	}

	public void induceRage(float amount){
		if (playerState == PlayerState.Lucid) {
			rage += amount;
		}
		if (rage > 100) {
			rage = 100;
		}
	}

	public void addMorale(float amount){
		morale += amount;
		if (morale > 100) {
			morale = 100;
		}
	}

	private void kill(){
		Debug.Log ("Restart");
		Application.LoadLevel (0);
	}

	private void findNearestFriend(){
		GameObject[] friends = GameObject.FindGameObjectsWithTag ("Friend") as GameObject[];
		List<GameObject> friendsList = new List<GameObject> ();
		foreach (GameObject friend in friends) {
			if (friend.name != "Player"){
				friendsList.Add(friend);
			}
		}
		if (friendsList != null && friendsList.Count > 0) {
			foreach (GameObject friend in friendsList) {
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
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameMaster : MonoBehaviour {

	public enum GameState {Play, Loading, Level_Complete};
	public enum Direction {Up, Down, Left, Right};

	private int numFriendly = 0;
	private int numEnemy = 0;
	private int level = 0;

	protected static GameMaster instance;

	public List<GameObject> additives = new List<GameObject>();
	public List<GameObject> bloodSpatter = new List<GameObject> ();
	public GameObject mainCamera;
	public GameObject zombie;
	public GameObject civilian;
	public Player player;

	public Text scoreText;
	public Text scoreTextBg;

	private GameState gameState = GameState.Play;

	public GameObject oldAdditive;
	public GameObject newAdditive;

	private Elevator oldElevator;
	private Elevator newElevator;

	private Random rand = new Random();
	private float slerpIn = 0.05f;
	private float slerpOut = 0.01f;

	private Vector3 outDest;
	private Vector3 inDest;

	private int spawnDepth = 200;
	
	// Use this for initialization
	void Start () {
		instance = this;
		startLevel ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown("escape")) Application.Quit();
		if (instance.gameState == GameState.Loading) {
			updateLoading ();
		} else if (instance.gameState == GameState.Play) {
			updatePlay ();
		} else if (instance.gameState == GameState.Level_Complete) {

		}
	}

	private void updateLoading(){
		if (instance.oldAdditive != null) {
			if (instance.oldAdditive.transform.position.z < -30) {
				Destroy (instance.oldAdditive);
			}
			instance.oldAdditive.transform.position = Vector3.Lerp (instance.oldAdditive.transform.position, instance.outDest, instance.slerpOut);
		}
		instance.newAdditive.transform.position = Vector3.Lerp(instance.newAdditive.transform.position, instance.inDest, instance.slerpIn);
		if (Vector3.Distance (instance.newAdditive.transform.position, instance.inDest) < 0.2f) {
			startLevel();
			instance.newAdditive.transform.position = instance.inDest;
			instance.oldElevator.finalize(instance.newAdditive.transform);
		}
	}

	private void updatePlay(){
		if (numEnemy <= 0) {
			finishLevel ();
		}
	}

	private void finishLevel(){
		instance.newElevator.open ();
		instance.gameState = GameState.Level_Complete;
		player.addMorale (numFriendly * 10);
	}

	private void startLevel(){
		instance.numFriendly = 0;
		instance.numEnemy = 0;
		foreach (Transform child in instance.newAdditive.transform) {
			if (child.GetComponent <Zombie>() != null){
				instance.numEnemy++;
			} else if (child.GetComponent <Civilian>() != null){
				instance.numFriendly++;
			} else if (child.GetComponent <Elevator>() != null){
				instance.newElevator = (Elevator) child.GetComponent <Elevator>();
			}
		}
		instance.gameState = GameState.Play;
		instance.level++;
		setScore (level);
	}


	public static void loadNext(Elevator elevator){
		instance.oldElevator = elevator;

		instance.gameState = GameState.Loading;
		int nextIndex = Random.Range(0, instance.additives.Count);
		int nextOrientation = Random.Range (0, 4);
		int angle = 0;
		switch (nextOrientation) {
		case 4: angle = 0; break;
		case 1: angle = 90; break;
		case 2: angle = 180; break;
		case 3: angle = 270; break;
		}


		instance.oldAdditive = instance.newAdditive;
		instance.outDest = new Vector3 (instance.oldAdditive.transform.position.x, instance.oldAdditive.transform.position.y, -50);
		instance.inDest = new Vector3 (elevator.transform.position.x, elevator.transform.position.y, 0);
		Vector3 inSpawn = new Vector3 (elevator.transform.position.x, elevator.transform.position.y, instance.spawnDepth);
		instance.newAdditive = (GameObject) Instantiate (instance.additives [nextIndex], inSpawn, Quaternion.AngleAxis (angle, Vector3.forward));


		GameObject[] randoms = GameObject.FindGameObjectsWithTag("Random") as GameObject[];
		List<GameObject> randomsList = new List<GameObject> (randoms);
		instance.shuffle (randomsList);

		int numFriendlyToSpawn = instance.level/4 + Random.Range (1, 4);
		int numEnemyToSpawn = instance.level/2 + Random.Range (1, 3);

		int numFriendly = 0;
		int numEnemy = 0;

		foreach (GameObject random in randomsList) {
			if (numFriendly < numFriendlyToSpawn){
				createCivilian(random.transform.position);
				numFriendly++;
			} else if (numEnemy < numEnemyToSpawn){
				createZombie(random.transform.position);
				numEnemy++;
			}
			Destroy (random);
		}
	}

	public static bool isPlayState(){
		return (instance.gameState == GameState.Play);
	}

	public static void createBloodSpatter(Vector3 location, float scale){
		location.z = 1.99f;
		location.x += Random.Range (-1, 1);
		location.y += Random.Range (-1, 1);
		GameObject spatter = (GameObject) Instantiate (instance.bloodSpatter [Random.Range(0, instance.bloodSpatter.Count)], location, Quaternion.AngleAxis (Random.Range (0,360), Vector3.forward));
		spatter.transform.localScale = spatter.transform.localScale * scale;
		spatter.transform.parent = instance.newAdditive.transform;
		spatter.SetActive (true);
	}

	public static void createZombie(Vector3 location){
		//location.z = 0;
		GameObject zombie = (GameObject) Instantiate (instance.zombie, location, Quaternion.AngleAxis (Random.Range (0,360), Vector3.forward));
		zombie.transform.parent = instance.newAdditive.transform;
		instance.numEnemy++;
	}

	public static void createCivilian(Vector3 location){
		//location.z = 0;
		GameObject civilian = (GameObject) Instantiate (instance.civilian, location, Quaternion.AngleAxis (Random.Range (0,360), Vector3.forward));
		civilian.transform.parent = instance.newAdditive.transform;
		instance.numFriendly++;
	}

	public static void decrementEnemyCount(){
		instance.numEnemy--;
	}

	public static void decrementFriendlyCount(){
		instance.numFriendly--;
	}

	public static void shakeCamera(float amount){
		Vector3 cameraPos = instance.mainCamera.transform.position;
		cameraPos.x += Random.Range (-amount, amount);
		cameraPos.y += Random.Range (-amount, amount);
		cameraPos.z += Random.Range (-amount, amount);
		instance.mainCamera.transform.position = cameraPos;
	}

	private void shuffle<GameObject>(IList<GameObject> list)  
	{  
		System.Random rng = new System.Random ();
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			GameObject value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}

	public static void setScore(int score){
		instance.scoreText.text = ""+score;
		instance.scoreTextBg.text = ""+score;
	}
}











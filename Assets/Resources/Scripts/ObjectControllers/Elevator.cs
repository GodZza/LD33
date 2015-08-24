using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Elevator : MonoBehaviour {
	
	public List<GameObject> walls;
	public List<GameObject> lights;
	public GameObject door;
	private bool triggered = false;

	void OnTriggerEnter2D (Collider2D collision) {
		if(!triggered && collision.gameObject.GetComponent<Player>() != null) {
			triggered = true;
			foreach (GameObject wall in walls){
				wall.SetActive(true);
			}
			lightsOff();
			transform.parent = null;
			GameMaster.loadNext(this);
		}
	}

	public void open(){
		door.SetActive (false);
		lightsGreen ();
	}

	public void finalize(Transform newParent){
		foreach (GameObject wall in walls){
			wall.SetActive(false);
		}
		transform.parent = newParent;
	}

	public void lightsGreen(){
		foreach (GameObject light in lights){
			Light pointLight = light.GetComponent<Light>();
			pointLight.color = Color.green;
		}
	}

	public void lightsOff(){
		foreach (GameObject light in lights){
			light.SetActive(false);
		}
	}
}

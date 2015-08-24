using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReceiveShadows : MonoBehaviour {

	private Material defaultMaterial;

	// Use this for initialization
	void Start () {
		defaultMaterial = Resources.Load("Materials/ReceiveShadows", typeof(Material)) as Material;
		setShadows (gameObject);
	}

	private void setShadows(GameObject gameObject){

		Renderer renderer = gameObject.GetComponent<Renderer> ();
		if (renderer != null && gameObject.layer == 0){
			renderer.receiveShadows = true;
			renderer.material = defaultMaterial;
		}

		foreach (Transform trans in gameObject.transform)
		{
			setShadows ( trans.gameObject );    
		}        
	}
}

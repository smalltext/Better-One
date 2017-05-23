using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetect : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnTriggerEnter2D(Collider2D other){
		if(other.gameObject.tag == "CutscenePlayer")
			//other.gameObject.scene++;
		else if(other.gameObject.tag == "CheckpointManager"){
			//other.gameObject.??;
			other.gameObject.setActive(false);
		}
		
	}
}

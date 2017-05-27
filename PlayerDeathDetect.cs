using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathDetect : MonoBehaviour {
	
	public int deathSpeed;
	bool playerDead;
	

	// Use this for initialization
	void Start () {
		playerDead = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnCollisionEnter2D(Collision2D other){
		if(other.gameObject.tag == "ground" && other.gameObject.GetComponent<Rigidbody2D>.velocity.y >= deathSpeed){
			Destroy(other.gameObject);
			playerDead = true;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
	
	GameObject player;
	public int panSpeed;
	Vector3 lastPos;
	bool locked;

	void Start () {
		locked = false;
		player = GameObject.FindWithTag("Player");
		lastPos = transform.position;
		
	}
	
	void Update(){
		if(player != null && !locked){
			transform.position = player.transform.position;
		}
		
		//make sure to choose input key
		if(Input.GetKeyDown(KeyCode.???)){
			lockPos();
			MoveTo(???);
		}
		if(Input.GetKeyDown(KeyCode.???)){
			MoveBack();
		}
		if(Input.GetKeyDown(KeyCode.???)){
			lockPos();
		}
	}

    public void UpdatePosition()
    {
        //Manual update loop called by Player, which can be stopped
    }

    public Static void MoveTo(Vector2 place)
    {
        //Pans camera to desired position
	lastPos = transform.position;
	transform.position = Vector3.Lerp(lastPos, place.Vector3, Time.DeltaTime * panSpeed);
    }

    public Static void MoveBack()
    {
        //Pans camera back to original position
	Vector3 currentPos = transform.position;
	transform.position = Vector3.Lerp(currentPos, lastPos, Time.DeltaTime * panSpeed);
    }
	
	
	public Static void lockPos(){
		locked = true;
		transform.position = transform.position;
	}
}

public GameObject player;
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
		/*if(Input.GetKeyDown(KeyCode.Q)){
			lockPos();
			MoveTo(new Vector2(15.0f, player.transform.position.y));
		}
		if(Input.GetKeyDown(KeyCode.R)){
			MoveBack();
		}*/
		if(Input.GetKeyDown(KeyCode.Y)){
			lockPos();
		}
	}
	/*
	public void UpdatePosition()
	{
		//Manual update loop called by Player, which can be stopped
	}

	public void MoveTo(Vector2 place)
	{
		//Pans camera to desired position
		lastPos = transform.position;
		transform.position = Vector3.Lerp(lastPos, new Vector3(place.x, place.y), Time.deltaTime * panSpeed);
	}

	public void MoveBack()
	{
		//Pans camera back to original position
		Vector3 currentPos = transform.position;
		transform.position = Vector3.Lerp(currentPos, player.transform.position, Time.deltaTime * panSpeed);
	}
	*/

	public void lockPos(){
		locked = true;
		transform.position = transform.position;
	}

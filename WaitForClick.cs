using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForClick : MonoBehaviour {

    public string nextLevel;
    public string levelManagerObject;
    public SceneToggler _scenetoggler;

    public void Start()
    {
        _scenetoggler = GameObject.Find(levelManagerObject).GetComponent<SceneToggler>();
    }

    public void OnMouseUp()
    {
        _scenetoggler.Load(nextLevel);
    }
}

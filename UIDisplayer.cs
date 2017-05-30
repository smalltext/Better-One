using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDisplayer : MonoBehaviour {

    private Text renderer;
    private Slider slider;
    public Parameters _parameters;

	void Start () {
        renderer = this.transform.GetChild(0).GetComponent<Text>();
        slider = this.transform.GetChild(1).GetComponent<Slider>();

        slider.maxValue = _parameters.HangTime;
        slider.minValue = 0;

        DisplayDeaths(0);
        DisplayTime(0);
	}
	
    public void DisplayDeaths(int deaths)
    {
        renderer.text = "Total Deaths: " + deaths;
    }

    public void DisplayTime(float time)
    {
        slider.value = time;
    }
}

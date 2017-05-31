using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneToggler : MonoBehaviour {

    public string firstScene;
    private int deathCount;
    public int levelIndex { get; set; }

    private UIDisplayer _uidisplayer;

    public string[] Level { get; set; }

    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        Level = new string[3] { "Level 0", "Level 1", "Level 2" };
        Load(firstScene);
    }

    public void Update()
    {
        if (_uidisplayer)
        {
            _uidisplayer.DisplayDeaths(deathCount);
        }
    }

    private void PlayerReset()
    {
        levelIndex = 0;
        deathCount = 0;
    }

    public void AddDeath()
    {
        deathCount++;
    }

    public void NextLevel()
    {
        levelIndex++;
        NewUI(Level[levelIndex]);
        SceneManager.LoadScene(Level[levelIndex]);
    }

    public void Load(string next)
    {
        NewUI(next);
        PlayerReset();
        SceneManager.LoadScene(next);
    }

    private void NewUI(string next)
    {
        for (int i = 0; i < Level.Length; i++)
        {
            if (next == Level[i])
            {
                _uidisplayer = GameObject.Find("UI Displayer").GetComponent<UIDisplayer>();
            }
        }
    }
}

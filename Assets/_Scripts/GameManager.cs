using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Worm[] worms;
    private void Awake()
    {
        instance = this;
        
        //set target frame rate
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        GridManager.instance.GenerateGrid();
        foreach (var worm in  worms)
        {
            worm.CreateWorm();
        }
        GridManager.instance.SpawnObstacles();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

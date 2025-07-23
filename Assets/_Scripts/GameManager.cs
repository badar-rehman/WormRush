using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Worm[] worms;
    
    public GameConfigs gameConfigs;
    
    private void Awake()
    {
        instance = this;
        
        //set target frame rate
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        GridManager.instance.GenerateGrid();
        CreateWorms();
        GridManager.instance.SpawnObstacles();
    }

    private void CreateWorms()
    {
        foreach (var worm in  worms)
        {
            worm.CreateWorm();
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

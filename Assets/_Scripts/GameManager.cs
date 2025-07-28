using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Worm[] worms;
    
    public GameConfigs gameConfigs;

    public Slider speedSlider;
    public TextMeshProUGUI speedText;
    
    private void Awake()
    {
        instance = this;
        
        //set target frame rate
        Application.targetFrameRate = 60;
        
        speedSlider.value = gameConfigs.moveSpeed;
        speedText.text = "Speed: " + gameConfigs.moveSpeed.ToString();
        
        speedSlider.onValueChanged.AddListener(x =>
        {
            gameConfigs.moveSpeed = x;
            speedText.text = "Speed: " + x.ToString();
        });
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

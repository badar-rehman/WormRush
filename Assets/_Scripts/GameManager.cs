using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Worm worm;
    private void Awake()
    {
        instance = this;
        
        //set target frame rate
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        GridManager.instance.GenerateGrid();
        worm.CreateWorm();
    }
}

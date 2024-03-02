using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : AManager<GameManager>
{
    public bool isGamePaused = false;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            isGamePaused = !isGamePaused;
            Time.timeScale = isGamePaused ? 0 : 1;
        }
    }
}

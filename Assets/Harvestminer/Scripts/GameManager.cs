using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : AManager<GameManager>
{
    public bool isGamePaused = false;
    public bool isControlShip = false;

    public Transform player;
    public Transform ship;

    void Update()
    {
        if (Input.GetButtonDown("Cancel") && !isControlShip)
        {
            isGamePaused = !isGamePaused;
            Time.timeScale = isGamePaused ? 0 : 1;
            Camera.main.GetComponent<ThirdPersonOrbitCamBasic>().enabled = !isGamePaused;
        }
    }
}

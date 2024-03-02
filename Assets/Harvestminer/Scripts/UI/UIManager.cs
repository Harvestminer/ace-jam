using UnityEngine;

public class UIManager : AManager<UIManager>
{
    public Transform PauseMenu;

    void Update()
    {
        if (PauseMenu.gameObject.activeSelf != GameManager.instance.isGamePaused)
            PauseMenu.gameObject.SetActive(GameManager.instance.isGamePaused);
    }
}

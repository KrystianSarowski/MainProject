using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameoverManager : MonoBehaviour
{
    public void ChangeScene(string t_sceneName)
    {
        GameplayManager.LoadScene(t_sceneName);
    }

    public void Exit()
    {
        Application.Quit();
    }
}

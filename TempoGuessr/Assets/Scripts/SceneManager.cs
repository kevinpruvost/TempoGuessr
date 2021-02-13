using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SceneManager : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject game;
    void Start()
    {
        
    }



    public void BeginSwitchToGame()
    {
        DOTween.Restart("MainMenuExit");
    }

    public void SwitchToGame()
    {
        DOTween.Restart("GameEnter");
    }

    public void BeginSwitchToMainMenu()
    {
        DOTween.Restart("GameExit");
    }

    public void SwitchToMainMenu()
    {
        DOTween.Restart("MainMenuEnter");
    }
}

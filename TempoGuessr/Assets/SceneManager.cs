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



    public void SwitchToGame()
    {
        DOTween.Play("MainMenuExit");
    }

    public void SwitchToMainMenu()
    {
        DOTween.Play("MainMenuEnter");
    }
}

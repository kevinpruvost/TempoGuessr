using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AlmostEngine;
using DG.Tweening;

public class GuessScreen : Singleton<GuessScreen>
{
    [SerializeField] Text guessText;
    [SerializeField] Text complementText;
    void Start()
    {
        if (guessText == null || complementText == null)
            Debug.LogError("Something is missing.");
    }

    bool turnedOn = false;
    public void TurnOn(bool guessIsRight)
    {
        if (turnedOn) return;

        guessText.text = (guessIsRight) ? "Right guess !" : "Wrong guess !";
        complementText.text = "Timelines :\n\n" +
            "1. " + CardsManager.m_Instance.getDateAtIndex(0) + "\n" +
            "2. " + CardsManager.m_Instance.getDateAtIndex(1) + "\n" +
            "3. " + CardsManager.m_Instance.getDateAtIndex(2);
        DOTween.Restart("GuessEnter");
        turnedOn = true;
    }

    public void TurnOff()
    {
        if (!turnedOn) return;

        turnedOn = false;
        DOTween.Restart("GuessExit");
    }

    public void RestartGame()
    {
        DOTween.Restart("CardsExit");
        TurnOff();
    }
}

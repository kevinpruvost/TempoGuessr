using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] Text num;
    [SerializeField] Image photo;

    private int numId = 0;
    public int realNumId = 0;

    void Start()
    {
        SetNum(int.MaxValue);
    }

    public bool isGuessCorrect()
    {
        return numId == realNumId;
    }

    public void SetPhoto(Sprite newSprite)
    {
        photo.sprite = newSprite;
    }

    public void SetPhoto(Texture2D newImage)
    {
        Sprite sprite = Sprite.Create(newImage, new Rect(0.0f, 0.0f, newImage.width, newImage.height), new Vector2(0.5f, 0.5f));
        SetPhoto(sprite);
    }

    public void ZoomOnCard()
    {
        ZoomManager.m_Instance.TurnOn(photo.sprite);
    }

    public void SetNumFromManager()
    {
        CardsManager.m_Instance.SetNumToCard(this);
    }

    public void SetNum(int newNum)
    {
        numId = newNum;
        if (newNum == int.MaxValue)
            num.text = "?";
        else
            num.text = newNum.ToString();
    }

    public int GetNum()
    {
        return numId;
    }
}

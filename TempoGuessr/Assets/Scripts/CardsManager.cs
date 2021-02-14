using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlmostEngine;

public class CardsManager : Singleton<CardsManager>
{
    public List<Card> cards = new List<Card>();
    private List<int> nums = new List<int>();
    void Start()
    {
        nums.Resize(cards.Count + 1, int.MaxValue);

        foreach (var card in cards)
        {
            if (card == null)
            {
                Debug.LogError("There's a missing card in the CardsManager list.");
                return;
            }
        }
    }

    private void Restart()
    {
        nums.Clear();
        nums.Resize(cards.Count + 1, int.MaxValue);

        foreach (var card in cards)
        {
            card.SetNum(int.MaxValue);
        }
    }

    void Update()
    {
        
    }

    public string getDateAtIndex(int index)
    {
        return actualViews.dates[cards[index].realNumId - 1];
    }

    public Views actualViews = null;
    public void PickNewSet()
    {
        Restart();

        actualViews = ImageHandler.m_Instance.GetView();
        System.Random r = new System.Random();
        var randomArray = Enumerable.Range(0, cards.Count).OrderBy(x => r.Next()).ToList();
        for (int i = 0; i < cards.Count; ++i)
        {
            cards[i].SetPhoto(actualViews.images[randomArray[i]]);
            cards[i].realNumId = randomArray[i] + 1;
        }
    }

    public void Guess()
    {
        if (nums.Contains(int.MaxValue))
        {
            Debug.LogError("The fuck are you trying to do ?");
            return;
        }

        foreach (var card in cards)
        {
            if (!card.isGuessCorrect())
            {
                GuessScreen.m_Instance.TurnOn(false);
                return;
            }
        }
        GuessScreen.m_Instance.TurnOn(true);
    }

    private int actualIndex = 1;
    public void SetNumToCard(Card card)
    {
        if (!nums.Contains(int.MaxValue) || actualIndex > cards.Count)
        {
            for (int i = 0; i < nums.Count; ++i)
            {
                nums[i] = int.MaxValue;
                cards[i].SetNum(int.MaxValue);
            }
            actualIndex = 1;
        }
        for (int i = 0; i < cards.Count; ++i)
        {
            if (card == cards[i])
            {
                if (nums[i] != int.MaxValue) return;

                card.SetNum(actualIndex);
                nums[i] = actualIndex++;
                return;
            }
        }
    }
}

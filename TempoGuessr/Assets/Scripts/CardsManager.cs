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

    public void PickNewSet()
    {
        Restart();
        // TODO
    }

    public void Guess()
    {
        if (nums.Contains(int.MaxValue))
        {
            Debug.LogError("The fuck are you trying to do ?");
            return;
        }

        // TODO
    }

    private int actualIndex = 1;
    public void SetNumToCard(Card card)
    {
        if (!nums.Contains(int.MaxValue))
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
                Debug.Log("Name : " + card.name);
                if (nums[i] != int.MaxValue) return;
                Debug.Log("Name : " + card.name);

                card.SetNum(actualIndex);
                nums[i] = actualIndex++;
                return;
            }
        }
    }
}

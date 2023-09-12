using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour, IItemInterface
{
    public bool OnPickedUp(GameObject other)
    {
        GameManager.Instance.OnCoinCollected();
        return true;
    }
}

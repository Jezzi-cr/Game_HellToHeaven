using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenLeg : MonoBehaviour, IItemInterface
{
    // How much health to give the character
    public float health = 0.25f;
    
    public bool OnPickedUp(GameObject other)
    {
        var character = other.GetComponent<Character>();
        if (character)
        {
            return character.AddHealth(health) > 0f;
        }

        return false;
    }
}

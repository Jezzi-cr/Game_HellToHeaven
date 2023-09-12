using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisher : MonoBehaviour, IItemInterface
{
    public Lava lava;
    public float cooldownTime = 2f;
    
    public bool OnPickedUp(GameObject other)
    {
        lava.Extinguish(cooldownTime);
        return true;
    }
}

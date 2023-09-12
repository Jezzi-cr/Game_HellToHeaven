using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.gameObject.GetComponent<Character>())
        {
            // Notify the game manager
            GameManager.Instance.OnPlayerReachedGoal();
        }
    }
}

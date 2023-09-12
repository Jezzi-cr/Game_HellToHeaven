using System;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    public float damageRate = 0.1f;
    public float damageOnEnter = 0.1f;

    private float _lastDamageTime;

    private void OnTriggerEnter(Collider other)
    {
        if (damageOnEnter > 0f)
        {
            var damageInterface = other.GetComponent<IDamageInterface>();
            damageInterface?.ApplyDamage(damageOnEnter);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (damageRate > 0f)
        {
            var damageInterface = other.GetComponent<IDamageInterface>();
            damageInterface?.ApplyDamage(damageRate * Time.fixedDeltaTime);
        }
    }
}
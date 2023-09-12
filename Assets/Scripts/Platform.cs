using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public float fallDelay = 2f;
    public float fallAnimAmplitude = 5f;
    public float fallAnimFrequency = 10f;

    private Quaternion _startRotation;
    private float _fallRatio;
    private int _numContacts;
    private bool _hasFallen;

    private void Start()
    {
        _startRotation = transform.rotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_hasFallen && ++_numContacts == 1)
        {
            StartCoroutine(FallCoroutine());
        }
    }

    private void OnCollisionExit(Collision other)
    {
        --_numContacts;
    }

    private bool ShouldFall()
    {
        return _numContacts > 0;
    }

    private IEnumerator FallCoroutine()
    {   
        while (ShouldFall() || _fallRatio > 0f)
        {
            // Update the fall ratio
            var targetFallRatio = _numContacts > 0 ? 1f : 0f;
            var deltaFallRatio = targetFallRatio - _fallRatio;
            if (Mathf.Abs(deltaFallRatio) > 0f)
            {
                _fallRatio = Mathf.Clamp01(_fallRatio + Mathf.Sign(deltaFallRatio) / fallDelay * Time.deltaTime);
            }

            // Update the animation
            if (_fallRatio > 0.01f)
            {
                var rotZ = Mathf.Sin(fallAnimFrequency * Mathf.PI * Time.time) * fallAnimAmplitude * _fallRatio;
                transform.rotation = _startRotation * Quaternion.Euler(0f, 0f, rotZ);
            }
            
            // Activate physics once we reached 100%
            if (_fallRatio >= 1f)
            {
                GetComponent<Rigidbody>().isKinematic = false;
                _hasFallen = true;
                break;
            }

            yield return null;
        }
    }
}

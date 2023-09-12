using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Vector3 offset = new(0f, 0f, -10f);
    public float interpSpeed = 2f;

    private void Start()
    {
        // Snap to the target position initially
        transform.position = GetTargetPosition();
    }

    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, GetTargetPosition(), interpSpeed * Time.deltaTime);
    }

    private Vector3 GetTargetPosition()
    {
        var character = GameManager.Instance.GetPlayerCharacter();
        return character ? character.transform.position + offset : transform.position;
    }
}

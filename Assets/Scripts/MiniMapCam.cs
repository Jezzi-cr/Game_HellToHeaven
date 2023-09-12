using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCam : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        transform.position = new Vector3(Mathf.Clamp(target.position.x, -30, 30), target.position.y, transform.position.z);
    }
}

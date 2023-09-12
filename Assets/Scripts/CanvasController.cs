using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CanvasController : MonoBehaviour
{
    public Transform target;
    public float distanceUp;
    public float distanceForward;
    void Start()
    {
        transform.position = new Vector3(target.position.x, target.position.y + distanceUp,
            target.position.z + distanceForward);

    }
    
    void FixedUpdate()
    {
        transform.position = new Vector3(target.position.x, target.position.y + distanceUp,
            target.position.z + distanceForward);
    }
}

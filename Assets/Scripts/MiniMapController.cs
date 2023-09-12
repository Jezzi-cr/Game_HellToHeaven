using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    public bool show = false;

    private void Start()
    {
        gameObject.SetActive(show);
    }

    public void SwitchMap()
    {
        show = !show;
        gameObject.SetActive(show);
    }
}

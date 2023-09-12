using System;
using UnityEngine;

public class WorldIcon : MonoBehaviour
{
    public Sprite sprite;
    public Color color = Color.white;
    public Vector2 size = new(12f, 12f);

    private void Start()
    {
        if (MiniMap.Instance)
        {
            MiniMap.Instance.AddIcon(this);
        }
    }

    private void OnDisable()
    {
        if (MiniMap.Instance)
        {
            MiniMap.Instance.RemoveIcon(this);
        }
    }
}

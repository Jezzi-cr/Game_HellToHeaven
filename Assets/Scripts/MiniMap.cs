using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MiniMap : MonoBehaviour
{
    public static MiniMap Instance { get; private set; }

    public GameObject iconPrefab;
    public Collider mapBounds;
    public Lava lava;
    public RectTransform lavaImage;
    
    private Vector2 _mapOrigin;
    private Vector2 _mapSize;

    private RectTransform _rectTransform;
    
    private readonly List<MiniMapIcon> _icons = new();

    private void Awake()
    {
        Instance = this;
        _rectTransform = GetComponent<RectTransform>();
        
        // Calculate the map size
        var bounds = mapBounds.bounds;
        _mapOrigin = bounds.min;
        _mapSize = bounds.size;
        
        // Adjust the size so it fits the map aspect ratio
        var aspectRatio = _mapSize.y / _mapSize.x;
        var size = _rectTransform.sizeDelta;
        _rectTransform.sizeDelta = new Vector2(size.x, size.x * aspectRatio);
    }

    private void Update()
    {
        // Update all icon positions
        foreach (var icon in _icons)
        {
            var pos = ProjectOntoMap(icon.GetWorldPosition());
            icon.UpdatePosition(pos);
        }
        
        // Update the lava height
        var lavaPos = ProjectOntoMap(lava.transform.position);
        var lavaImageSize = lavaImage.sizeDelta;
        lavaImage.sizeDelta = new Vector2(lavaImageSize.x, lavaPos.y);
    }

    private Vector2 ProjectOntoMap(Vector3 pos)
    {
        var canvasSize = _rectTransform.sizeDelta;
        return (new Vector2(pos.x, pos.y) - _mapOrigin) / _mapSize * canvasSize;
    }

    public void AddIcon(WorldIcon worldIcon)
    {
        Assert.AreEqual(FindIconIndex(worldIcon), -1);

        var iconObject = Instantiate(iconPrefab);
        var icon = iconObject.GetComponent<MiniMapIcon>();
        icon.InitIcon(_rectTransform, worldIcon);
        _icons.Add(icon);
    }

    public void RemoveIcon(WorldIcon worldIcon)
    {
        var index = FindIconIndex(worldIcon);
        Assert.AreNotEqual(index, -1);
        if (_icons[index])
        {
            Destroy(_icons[index].gameObject);
        }
        _icons.RemoveAt(index);
    }
    
    private int FindIconIndex(WorldIcon worldIcon)
    {
        return _icons.FindIndex(icon => icon.GetWorldIcon() == worldIcon);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class MiniMapIcon : MonoBehaviour
{
    private WorldIcon _worldIcon;
    
    private RectTransform _rectTransform;
    private Image _image;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    public void UpdatePosition(Vector2 pos)
    {
        _rectTransform.anchoredPosition = pos;
    }

    public Vector3 GetWorldPosition()
    {
        return _worldIcon.transform.position;
    }

    public WorldIcon GetWorldIcon()
    {
        return _worldIcon;
    }
    
    public void InitIcon(Transform parent, WorldIcon worldIcon)
    {
        _worldIcon = worldIcon;
        _rectTransform.SetParent(parent, false);
        _rectTransform.sizeDelta = worldIcon.size;
        _image.color = worldIcon.color;
        if (worldIcon.sprite)
        {
            _image.sprite = worldIcon.sprite;
        }
    }
}

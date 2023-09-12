using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    private void Start()
    {
        var prefab = GameManager.Instance.GetPlayerCharacterPrefab();
        Instantiate(prefab, transform);
    }
}

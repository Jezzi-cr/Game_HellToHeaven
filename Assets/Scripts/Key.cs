using UnityEngine;

public interface IKeyInterface
{
    public void OnUnlocked();
}

public class Key : MonoBehaviour, IItemInterface
{
    public GameObject[] targets;
    
    public bool OnPickedUp(GameObject other)
    {
        foreach (var target in targets)
        {
            var keyInterface = target.GetComponent<IKeyInterface>();
            keyInterface?.OnUnlocked();
        }

        return true;
    }
}

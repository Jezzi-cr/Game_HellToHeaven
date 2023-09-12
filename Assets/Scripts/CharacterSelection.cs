using System;
using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    private GameObject _character;
    
    private void Start()
    {
        UpdateCharacter();
    }

    public void NextCharacter()
    {
        CycleCharacter(1);
    }
    
    public void PrevCharacter()
    {
        CycleCharacter(-1);
    }

    public void GoMenu()
    {
        GameManager.Instance.LoadMenu();
    }

    private void CycleCharacter(int dir)
    {
        GameManager.Instance.CyclePlayerCharacterPrefab(dir);
        UpdateCharacter();
    }
    
    private void UpdateCharacter()
    {
        // Destroy the previous character
        if (_character)
        {
            Destroy(_character);
        }
        
        var prefab = GameManager.Instance.GetPlayerCharacterPrefab();
        _character = Instantiate(prefab, transform);
        // Disable input
        _character.GetComponent<Character>().SetEnableInput(false);
    }
}

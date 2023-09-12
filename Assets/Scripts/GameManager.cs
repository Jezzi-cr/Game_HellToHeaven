using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public enum GameState
{
    None,
    Playing,
    Dead,
    Won
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public string gameScene;
    public string menuScene;
    public string deathScene;
    public string winScene;
    public string characterSelectionScene;

    public float scorePerCoin = 10f;
    public float scorePerSecond = 1f;
    public float maxScoreTime = 300f;
    
    public float fadeTime = 2f;
    public Texture2D fadeTexture;
    public Color gameFadeColor = Color.black;
    public Color deathFadeColor = Color.black;
    public Color winFadeColor = Color.white;

    public AudioClip menuMusic;
    public AudioClip characterSectionMusic;
    public AudioClip gameMusic;
    public AudioClip gameOverMusic;
    public AudioClip winMusic;

    public GameObject inGameMenuPrefab;
    public GameObject[] characterPrefabs;
    
    private GameState _gameState = GameState.None;
    private Color _fadeColor;
    private bool _isFading;
    private GameObject _inGameMenu;

    private float _gameTime;
    private int _coinsCollected;
    private float _lastHighScore;
    private bool _isNewHighScore;
    private readonly List<Character> _characters = new();

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Setup the last fade color
        _fadeColor = gameFadeColor;
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Fade in with the last fade out color
        StartFade(_fadeColor, true);
        
        // Reset the game state
        SetGameState(scene.name == gameScene ? GameState.Playing : GameState.None);
        
        // Update the music
        AudioClip music = null;
        if (scene.name == gameScene)
        {
            // Make sure the score is reset when the game scene is reentered
            ResetScore();
            music = gameMusic;
        }
        else if (scene.name == menuScene)
        {
            music = menuMusic;
        }
        else if (scene.name == characterSelectionScene)
        {
            music = characterSectionMusic;
        }
        else if (scene.name == deathScene)
        {
            music = gameOverMusic;
        }
        else if (scene.name == winScene)
        {
            music = winMusic;
        }
        AudioManager.Instance.SetAmbientAudio(music);
    }

    private void Update()
    {
        // Update the timer
        if (IsGameInProgress())
        {
            _gameTime += Time.deltaTime;
        }
        
        // Check for the menu action
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsInGameMenuOpen())
            {
                CloseInGameMenu();
            }
            else
            {
                OpenInGameMenu();
            }
        }
    }

    private void OnGUI()
    {
        // Fade out using a full screen texture
        if (_fadeColor.a > 0.001f)
        {
            GUI.color = _fadeColor;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fadeTexture);
        }
    }

    public void AddCharacter(Character character)
    {
        _characters.Add(character);
    }
    
    public void RemoveCharacter(Character character)
    {
        _characters.Remove(character);
    }

    public Character GetPlayerCharacter()
    {
        return _characters.Count > 0 ? _characters[0] : null;
    }
    
    public GameObject GetPlayerCharacterPrefab()
    {
        var prefabName = PlayerPrefs.GetString("PlayerCharacter");
        foreach (var prefab in characterPrefabs)
        {
            if (prefab.name == prefabName)
            {
                return prefab;
            }
        }
        
        return characterPrefabs.Length > 0 ? characterPrefabs[0] : null;
    }

    public void SetPlayerCharacterPrefab(GameObject prefab)
    {
        PlayerPrefs.SetString("PlayerCharacter", prefab ? prefab.name : "");
    }
    
    public void CyclePlayerCharacterPrefab(int dir)
    {
        var newIndex = 0;
        
        var prefab = GetPlayerCharacterPrefab();
        for (var i = 0; i < characterPrefabs.Length; ++i)
        {
            if (characterPrefabs[i] == prefab)
            {
                newIndex = i + dir;
                break;
            }
        }
        
        // Wrap around
        newIndex = newIndex < 0 ? characterPrefabs.Length + newIndex : newIndex;
        newIndex = newIndex % characterPrefabs.Length;
        SetPlayerCharacterPrefab(characterPrefabs[newIndex]);
    }

    public void OnPlayerDied()
    {
        if (IsGameInProgress())
        {
            SetGameState(GameState.Dead);
        }
    }
    
    public void OnPlayerReachedGoal()
    {
        if (IsGameInProgress())
        {
            SetGameState(GameState.Won);
        }
    }

    public float GetGameTime()
    {
        return _gameTime;
    }

    public void OnCoinCollected()
    {
        ++_coinsCollected;
    }

    public int GetCoinsCollected()
    {
        return _coinsCollected;
    }

    public float GetScore()
    {
        return _coinsCollected * scorePerCoin + Mathf.Clamp(maxScoreTime - _gameTime, 0f, maxScoreTime) * scorePerSecond;
    }

    public float GetLastHighScore()
    {
        return _lastHighScore;
    }

    public bool IsNewHighScore()
    {
        return _isNewHighScore;
    }

    private void ResetScore()
    {
        _gameTime = 0f;
        _coinsCollected = 0;
    }
    
    private float LoadHighScore()
    {
        return PlayerPrefs.GetFloat("HighScore", 0f);
    }

    private void SaveHighScore(float score)
    {
        PlayerPrefs.SetFloat("HighScore", score);
        PlayerPrefs.Save();
    }

    private void SetGameState(GameState newState)
    {
        if (newState != _gameState)
        {
            // Call the state exit functions
            switch (newState)
            {
                case GameState.Playing:
                    ExitPlayingState();
                    break;
            }
            
            _gameState = newState;

            // Call the state functions
            switch (newState)
            {
                case GameState.Playing:
                    EnterPlayingState();
                    break;
                case GameState.Dead:
                    EnterDeadState();
                    break;
                case GameState.Won:
                    EnterWonState();
                    break;
            }
        }
    }

    private void EnterPlayingState()
    {
        ResetScore();
    }

    private void ExitPlayingState()
    {
        CloseInGameMenu();
    }

    private void EnterDeadState()
    {
        StartFade(deathFadeColor, false, deathScene);
    }
    
    private void EnterWonState()
    {
        // Check if we have beaten the high score
        var score = GetScore();
        _lastHighScore = LoadHighScore();
        _isNewHighScore = score > _lastHighScore;
        if (_isNewHighScore)
        {
            // Save the new high score
            SaveHighScore(score);
        }
        
        StartFade(winFadeColor, false, winScene);
    }

    private bool IsGameInProgress()
    {
        return _gameState == GameState.Playing;
    }

    // Goes to the menu level
    public void LoadMenu()
    {
        StartFade(gameFadeColor, false, menuScene);
    }
    
    // Goes to the game level
    public void LoadGame()
    {
        StartFade(gameFadeColor, false, gameScene);
    }

    // Goes to the character selection level
    public void LoadCharacterSelection()
    {
        StartFade(gameFadeColor, false, characterSelectionScene);
    }

    public bool IsInGameMenuOpen()
    {
        return _inGameMenu;
    }

    public void OpenInGameMenu()
    {
        if (!_inGameMenu && IsGameInProgress() && !_isFading)
        {
            _inGameMenu = Instantiate(inGameMenuPrefab);
            OnInGameMenuOpenChanged();
        }
    }
    
    public void CloseInGameMenu()
    {
        if (_inGameMenu)
        {
            Destroy(_inGameMenu);
            _inGameMenu = null;
            OnInGameMenuOpenChanged();
        }
    }

    private void OnInGameMenuOpenChanged()
    {
        // Disable input for the character while the in game menu is open
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void QuitGame()
    {
        StartFade(gameFadeColor, false, "", true);
    }

    private void StartFade(Color fadeColor, bool fadeIn, string scene = "", bool quitGame = false)
    {
        if (!_isFading)
        {
            CloseInGameMenu();
            StartCoroutine(FadeCoroutine(fadeColor, fadeIn, scene, quitGame));
        }
    }

    private IEnumerator FadeCoroutine(Color fadeColor, bool fadeIn, string scene, bool quitGame)
    {
        _isFading = true;
        
        // Update the fade amount
        _fadeColor = fadeColor;
        var elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            var fadeAmount = Mathf.Clamp01(elapsed / fadeTime);
            fadeAmount = fadeIn ? 1f - fadeAmount : fadeAmount;
            
            _fadeColor.a = fadeAmount;
            
            // Update audio
            AudioManager.Instance.SetFadeAmount(fadeAmount);
            
            yield return null;
        }

        // Load the given scene
        if (scene.Length > 0)
        {
            SceneManager.LoadScene(scene);
        }

        _isFading = false;

        if (quitGame)
        {
            Application.Quit();
        }
    }
}

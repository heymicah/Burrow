using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Rabbit")]
    [SerializeField] private RabbitController rabbit;
    [SerializeField] private Transform rabbitSpawnPoint;

    [Header("Apples")]
    [SerializeField] private GameObject[] apples;
    [SerializeField] private int applesRequired = 3;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Foxes")]
    [SerializeField] private GameObject foxPrefab;
    [SerializeField] private Transform[] foxSpawnPoints;

    [Header("Level Environments")]
    [SerializeField] private GameObject[] levelEnvironments;

    private int currentLevel = 1;
    private int applesCollected;
    private float currentHealth;
    private List<GameObject> spawnedFoxes = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        applesCollected = 0;
        if (rabbit != null && rabbitSpawnPoint != null)
            rabbit.ResetPosition(rabbitSpawnPoint.position);
        UIManager.Instance.UpdateProgressBar(applesCollected, applesRequired);
        UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);
        UIManager.Instance.UpdateLevelLabel(currentLevel);
        ActivateLevelEnvironment();
    }

    public void CollectApple()
{
    applesCollected++;
    UIManager.Instance.UpdateProgressBar(applesCollected, applesRequired);

    if (applesCollected == 2)
        UIManager.Instance.ShowDeforestationPopup(currentLevel);

    if (applesCollected >= applesRequired)
        LevelComplete();
}

    private void LevelComplete()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (currentLevel >= levelEnvironments.Length)
        {
            // Final level — show game complete instead
            UIManager.Instance.ShowGameCompletePopup();
        }
        else
        {
            UIManager.Instance.ShowLevelCompletePopup(currentLevel);
        }
    }

    public void StartNextLevel()
    {
        currentLevel++;
        applesCollected = 0;
        currentHealth = maxHealth;

        // Re-enable apples
        foreach (var apple in apples)
            apple.SetActive(true);

        // Destroy old foxes
        foreach (var fox in spawnedFoxes)
        {
            if (fox != null)
                Destroy(fox);
        }
        spawnedFoxes.Clear();

        // Spawn foxes from level 2 onward
        if (currentLevel >= 2)
            SpawnFoxes();

        // Swap environment
        ActivateLevelEnvironment();

        // Reset rabbit
        if (rabbit != null && rabbitSpawnPoint != null)
            rabbit.ResetPosition(rabbitSpawnPoint.position);

        // Resume
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Update UI
        UIManager.Instance.HideLevelCompletePopup();
        UIManager.Instance.UpdateProgressBar(applesCollected, applesRequired);
        UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);
        UIManager.Instance.UpdateLevelLabel(currentLevel);
    }

    private void ShowGameComplete()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UIManager.Instance.ShowGameCompletePopup();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
        // In editor this won't work, so add:
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void SpawnFoxes()
    {
        int foxCount = Mathf.Min(currentLevel, foxSpawnPoints.Length);
        for (int i = 0; i < foxCount; i++)
        {
            GameObject fox = Instantiate(foxPrefab, foxSpawnPoints[i].position, foxSpawnPoints[i].rotation);
            // Assign rabbit reference to chase AI
            var chaseAI = fox.GetComponent<FoxChaseAI>();
            if (chaseAI != null)
                chaseAI.rabbit = rabbit.transform;
            spawnedFoxes.Add(fox);
        }
    }

    private void ActivateLevelEnvironment()
    {
        for (int i = 0; i < levelEnvironments.Length; i++)
            levelEnvironments[i].SetActive(i == currentLevel - 1);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0f) currentHealth = 0f;

        UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            GameOver();
    }

    private void GameOver()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

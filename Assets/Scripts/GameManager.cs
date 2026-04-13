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

        // Canvas 1: Welcome
        UIManager.Instance.ShowInfoPanel(
            "Welcome to Burrow!",
            "Did you know that We Are Losing Forests at a Rate Equivalent to 27 Soccer Fields per Minute? Forests which are vital for many functions, including producing oxygen and rapidly disappearing. Don't believe us? Here is an opportunity to see it for yourself.",
            "Throughout this game, you will need to collect apples to progress through levels. However, over time this task may become harder as resources run out.",
            "Start", () => ShowLevelIntro(1)
        );
    }

    private void ShowLevelIntro(int level)
    {
        switch (level)
        {
            case 1:
                UIManager.Instance.ShowInfoPanel(
                    "Level 1",
                    "Forest degradation refers to when forest ecosystems are weakened to a point where they lose their ability to provide goods and services to people and nature. Nearly 1.6 billion hectares of forest are currently at a high risk of degradation. (1 hectare = 100 acres).",
                    "In this first level, you will see a healthy forest. Collect 3 apples to move to the next level.",
                    "Start", () => UIManager.Instance.HideInfoPanel()
                );
                break;
            case 2:
                UIManager.Instance.ShowInfoPanel(
                    "Level 2",
                    "When trees are cut down, the carbon they store is released back into the atmosphere, mainly as carbon dioxide. Between 2015 and 2017, the global loss of tropical forests generated 10 billion tonnes of carbon dioxide – nearly 10% of the annual human CO2 emissions.",
                    "In this level, you will see the start of deforestation. Continue collecting apples to continue.",
                    "Start", () => UIManager.Instance.HideInfoPanel()
                );
                break;
            case 3:
                UIManager.Instance.ShowInfoPanel(
                    "Level 3",
                    "We lose about 10 million hectares of forests every year. If you have made it to this level, you will see that there is not much forest left. Try your best to find food and complete the game.",
                    null,
                    "Start", () => UIManager.Instance.HideInfoPanel()
                );
                break;
        }
    }

    public void CollectApple()
{
    applesCollected++;
    UIManager.Instance.UpdateProgressBar(applesCollected, applesRequired);

    if (applesCollected >= applesRequired)
        LevelComplete();
}

    private void LevelComplete()
    {
        if (currentLevel >= 3)
        {
            // Canvas 5: Game over
            UIManager.Instance.ShowInfoPanel(
                "The End",
                "As you can see, there is not much forest left. Food runs out and people move in. Did you know that 25% of western drugs used to treat disease come from the rainforest? Forests are not just homes for animals, or nature retreats. These are ecosystems producing biodiversity for animal species, removing carbon emissions from the atmosphere and turning it into oxygen.",
                "Forests are vital to our survival as a species. Save forests, save life. Visit earth.org and ourworldindata.org to learn more about these stats.",
                "Play Again", () => RestartGame(),
                "Quit", () => QuitGame()
            );
        }
        else
        {
            StartNextLevel();
        }
    }

    public void StartNextLevel()
    {
        currentLevel++;
        applesCollected = 0;
        currentHealth = maxHealth;

        foreach (var apple in apples)
            apple.SetActive(true);

        foreach (var fox in spawnedFoxes)
            if (fox != null) Destroy(fox);
        spawnedFoxes.Clear();

        if (currentLevel >= 2)
            SpawnFoxes();

        ActivateLevelEnvironment();

        if (rabbit != null && rabbitSpawnPoint != null)
            rabbit.ResetPosition(rabbitSpawnPoint.position);

        UIManager.Instance.UpdateProgressBar(applesCollected, applesRequired);
        UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);
        UIManager.Instance.UpdateLevelLabel(currentLevel);

        // Canvas 3 or 4: Level intro (game stays paused, HideInfoPanel resumes it)
        ShowLevelIntro(currentLevel);
    }

    // private void ShowGameComplete()
    // {
    //     Time.timeScale = 0f;
    //     Cursor.lockState = CursorLockMode.None;
    //     Cursor.visible = true;
    //     UIManager.Instance.ShowGameCompletePopup();
    // }

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

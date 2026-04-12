using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Health Bar")]
    [SerializeField] private RectTransform healthBarFill;
    private float healthBarMaxWidth;

    [Header("Progress Bar")]
    [SerializeField] private RectTransform progressBarFill;
    [SerializeField] private TextMeshProUGUI progressText;
    private float progressBarMaxWidth;

    [Header("Level Label")]
    [SerializeField] private TextMeshProUGUI levelLabel;

    [Header("Level Complete Popup")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI levelCompleteText;
    [SerializeField] private Button nextLevelButton;

    [Header("Game Complete Popup")]
    [SerializeField] private GameObject gameCompletePanel;
    [SerializeField] private TMP_Text gameCompleteText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button quitButton;

    [Header("Deforestation Popup")]
[SerializeField] private GameObject deforestationPopupPanel;
[SerializeField] private TextMeshProUGUI deforestationText;
[SerializeField] private Button deforestationCloseButton;

    public void ShowDeforestationPopup(int level)
{
    string[] facts = {
        "Every year, 10 million hectares of forest are lost to deforestation — that's the size of Iceland disappearing annually.",
        "Deforestation destroys the homes of over 80% of the world's land-based animals, plants, and insects.",
        "It can take over 100 years for a forest to fully recover after deforestation. "It can take over 100 years for a forest to fully recover after deforestation. You can help by reducing paper waste, supporting sustainable products, and by demanding that your governments only source forest commodities in a way that ensures the protection of nature.""
    };

    deforestationText.text = facts[level - 1];
    deforestationPopupPanel.SetActive(true);
    Time.timeScale = 0f;
}

public void HideDeforestationPopup()
{
    deforestationPopupPanel.SetActive(false);
    Time.timeScale = 1f;
}

    public void ShowGameCompletePopup()
    {
        gameCompletePanel.SetActive(true);
    }

    public void HideGameCompletePopup()
    {
        gameCompletePanel.SetActive(false);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (healthBarFill != null)
            healthBarMaxWidth = healthBarFill.sizeDelta.x;

        if (progressBarFill != null)
            progressBarMaxWidth = progressBarFill.sizeDelta.x;

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(() => GameManager.Instance.StartNextLevel());

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        if (deforestationCloseButton != null)
            deforestationCloseButton.onClick.AddListener(() => HideDeforestationPopup());
    }

    public void UpdateProgressBar(int collected, int total)
    {
        if (progressBarFill != null)
        {
            float ratio = (float)collected / total;
            Vector2 size = progressBarFill.sizeDelta;
            size.x = progressBarMaxWidth * ratio;
            progressBarFill.sizeDelta = size;
        }

        if (progressText != null)
            progressText.text = $"{collected} / {total} Apples";
    }

    public void UpdateHealthBar(float current, float max)
    {
        if (healthBarFill != null)
        {
            float ratio = current / max;
            Vector2 size = healthBarFill.sizeDelta;
            size.x = healthBarMaxWidth * ratio;
            healthBarFill.sizeDelta = size;
        }
    }

    public void UpdateLevelLabel(int level)
    {
        if (levelLabel != null)
            levelLabel.text = $"Level {level}";
    }

    public void ShowLevelCompletePopup(int level)
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        if (levelCompleteText != null)
            levelCompleteText.text = $"Level {level} Complete!";
    }

    public void HideLevelCompletePopup()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }
}

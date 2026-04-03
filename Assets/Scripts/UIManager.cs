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

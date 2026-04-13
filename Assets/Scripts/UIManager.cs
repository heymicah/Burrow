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

    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI infoTitle;
    [SerializeField] private TextMeshProUGUI infoPara1;
    [SerializeField] private TextMeshProUGUI infoPara2;
    [SerializeField] private Button infoPrimaryButton;
    [SerializeField] private TextMeshProUGUI infoPrimaryButtonLabel;
    [SerializeField] private Button infoSecondaryButton;
    [SerializeField] private TextMeshProUGUI infoSecondaryButtonLabel;

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
    }

    // Called by GameManager to show the right canvas at the right time
    public void ShowInfoPanel(string title, string para1, string para2,
                               string primaryLabel, System.Action primaryAction,
                               string secondaryLabel = null, System.Action secondaryAction = null)
    {
        infoTitle.text = title;
        infoPara1.text = para1;

        bool hasPara2 = !string.IsNullOrEmpty(para2);
        infoPara2.gameObject.SetActive(hasPara2);
        if (hasPara2) infoPara2.text = para2;

        // Primary button (Start / Next Level)
        infoPrimaryButton.onClick.RemoveAllListeners();
        infoPrimaryButton.onClick.AddListener(() => primaryAction());
        infoPrimaryButtonLabel.text = primaryLabel;
        infoPrimaryButton.gameObject.SetActive(true);

        // Secondary button (Quit — only on game over)
        bool hasSecondary = secondaryAction != null;
        infoSecondaryButton.gameObject.SetActive(hasSecondary);
        if (hasSecondary)
        {
            infoSecondaryButton.onClick.RemoveAllListeners();
            infoSecondaryButton.onClick.AddListener(() => secondaryAction());
            infoSecondaryButtonLabel.text = secondaryLabel;
        }

        infoPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideInfoPanel()
    {
        infoPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
}
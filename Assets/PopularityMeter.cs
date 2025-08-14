using UnityEngine;
using UnityEngine.UI;

public class PopularityMeter : MonoBehaviour
{
    public static PopularityMeter Instance { get; private set; }

    [Header("UI")]
    public Slider meter;            // drag your Slider here
    public Text percentText;        // (optional) drag a Text here
    public CanvasGroup canvasGroup; // (optional) for fade/show; can be null

    private int totalBoxes;
    private int completedBoxes;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        SprayBox.OnAnySprayCompleted += HandleSprayCompleted;
    }

    void OnDisable()
    {
        SprayBox.OnAnySprayCompleted -= HandleSprayCompleted;
    }

    void Start()
    {
        // Count all boxes by tag
        totalBoxes = CountAllSprayBoxes();
        completedBoxes = CountCompletedAtStart();

        SetVisible(totalBoxes > 0);
        UpdateUI(true);
    }

    void HandleSprayCompleted(SprayBox box)
    {
        completedBoxes = Mathf.Clamp(completedBoxes + 1, 0, totalBoxes);
        UpdateUI();
    }

    int CountAllSprayBoxes()
    {
        var all = GameObject.FindGameObjectsWithTag("SprayBox");
        return all?.Length ?? 0;
    }

    int CountCompletedAtStart()
    {
        int c = 0;
        var boxes = FindObjectsOfType<SprayBox>(true);
        foreach (var b in boxes)
            if (b.IsCompleted) c++;
        return c;
    }

    void UpdateUI(bool immediate = false)
    {
        float fill = (totalBoxes > 0) ? (float)completedBoxes / totalBoxes : 0f;

        if (meter != null)
        {
            meter.minValue = 0f;
            meter.maxValue = 1f;
            meter.value = fill;
        }

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(fill * 100f) + "%";
    }

    void SetVisible(bool vis)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = vis ? 1f : 0f;
            canvasGroup.interactable = vis;
            canvasGroup.blocksRaycasts = false;
        }
        else if (meter != null)
        {
            meter.gameObject.SetActive(vis);
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One of these sits in every scene. It lights the lantern row, dims the
/// back/forward circles at the ends of the book, and wires the two buttons.
/// </summary>
public class ScreenController : MonoBehaviour
{
    [Header("Page")]
    public int pageIndex;                 // 0 = Home ... 5 = Credits

    [Header("Lantern row")]
    public Image[] lanterns = new Image[6];
    public Image[] rings = new Image[6];  // sage ring behind the current lantern
    public Sprite lanternOff;
    public Sprite lanternOn;

    [Header("Navigation")]
    public Button backButton;
    public Button nextButton;
    public Image backIcon;
    public Image nextIcon;
    public Color dimmed = new Color(1f, 1f, 1f, 0.35f);

    void Start()
    {
        ProgressManager.Visit(pageIndex);
        int lit = Mathf.Clamp(ProgressManager.Lit, 0, 6);

        for (int i = 0; i < lanterns.Length; i++)
        {
            if (lanterns[i] != null)
                lanterns[i].sprite = i < lit ? lanternOn : lanternOff;
            if (rings[i] != null)
                rings[i].enabled = i == pageIndex;
        }

        bool canBack = pageIndex > 0;
        bool canNext = pageIndex < 5;

        if (backButton != null)
        {
            backButton.interactable = canBack;
            backButton.onClick.AddListener(Back);
        }
        if (nextButton != null)
        {
            nextButton.interactable = canNext;
            nextButton.onClick.AddListener(Next);
        }
        if (backIcon != null) backIcon.color = canBack ? Color.white : dimmed;
        if (nextIcon != null) nextIcon.color = canNext ? Color.white : dimmed;
    }

    public void Next()
    {
        if (pageIndex < 5) SceneManager.LoadScene(pageIndex + 1);
    }

    public void Back()
    {
        if (pageIndex > 0) SceneManager.LoadScene(pageIndex - 1);
    }

    public void JumpTo(int index)
    {
        SceneManager.LoadScene(Mathf.Clamp(index, 0, 5));
    }

    public void ReadAgain()
    {
        ProgressManager.ResetProgress();
        SceneManager.LoadScene(0);
    }
}

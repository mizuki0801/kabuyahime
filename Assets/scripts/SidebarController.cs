using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SidebarController : MonoBehaviour
{
    public Button homeButton;
    public Button investButton;
    public Button learnButton;
    public Button gachaButton;
    public Button userButton;

    // 選択されていないボタン
    public Color normalColor = new Color32(135, 70, 190, 15);

    // 現在いるページ
    public Color selectedColor = new Color32(135, 70, 190, 235);

    void Start()
    {
        UpdateSidebar();
    }

    void UpdateSidebar()
    {
        // まず全部薄くする
        homeButton.image.color = normalColor;
        investButton.image.color = normalColor;
        learnButton.image.color = normalColor;
        gachaButton.image.color = normalColor;
        userButton.image.color = normalColor;

        // 今いるSceneだけ濃くする
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Home":
                homeButton.image.color = selectedColor;
                break;

            case "InvestScene":
                investButton.image.color = selectedColor;
                break;

            case "LearnScene":
                learnButton.image.color = selectedColor;
                break;

            case "GachaScene":
                gachaButton.image.color = selectedColor;
                break;

            case "UserScene":
                userButton.image.color = selectedColor;
                break;
        }
    }
}
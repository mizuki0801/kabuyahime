using UnityEngine;

public class StockDetailTabController : MonoBehaviour
{
    [Header("表示エリア")]
    public GameObject chartArea;
    public GameObject companyDescriptionPanel;

    [Header("期間ボタン")]
    public GameObject oneDayButton;
    public GameObject oneWeekButton;
    public GameObject oneMonthButton;
    public GameObject threeMonthsButton;
    public GameObject oneYearButton;
    public GameObject periodSelectedImage;

    [Header("企業説明・チャート タブ")]
    public RectTransform detailTabSelectedImage;
    public RectTransform companyTabButton;
    public RectTransform chartTabButton;


    // =========================
    // チャートを表示
    // =========================
    public void ShowChart()
    {
        // チャートを表示
        chartArea.SetActive(true);

        // 企業説明を非表示
        companyDescriptionPanel.SetActive(false);

        // 期間ボタンを表示
        SetPeriodButtons(true);

        // 紫画像を「チャート」の位置へ移動
        MoveTabSelectedImage(chartTabButton);
    }


    // =========================
    // 企業説明を表示
    // =========================
    public void ShowCompanyDescription()
    {
        // チャートを非表示
        chartArea.SetActive(false);

        // 企業説明を表示
        companyDescriptionPanel.SetActive(true);

        // 期間ボタンを非表示
        SetPeriodButtons(false);

        // 紫画像を「企業説明」の位置へ移動
        MoveTabSelectedImage(companyTabButton);
    }


    // =========================
    // 期間ボタンの表示・非表示
    // =========================
    private void SetPeriodButtons(bool isActive)
    {
        oneDayButton.SetActive(isActive);
        oneWeekButton.SetActive(isActive);
        oneMonthButton.SetActive(isActive);
        threeMonthsButton.SetActive(isActive);
        oneYearButton.SetActive(isActive);
        periodSelectedImage.SetActive(isActive);
    }


    // =========================
    // 選択中タブの紫画像を移動
    // =========================
    private void MoveTabSelectedImage(RectTransform targetButton)
    {
        if (detailTabSelectedImage == null || targetButton == null)
        {
            return;
        }

        detailTabSelectedImage.position = targetButton.position;
    }
}
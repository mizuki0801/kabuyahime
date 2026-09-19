using UnityEngine;

public class PeriodButtonController : MonoBehaviour
{
    // 選択中に表示する紫画像
    public RectTransform periodSelectedImage;

    // 各期間ボタン
    public RectTransform oneDayButton;
    public RectTransform oneWeekButton;
    public RectTransform oneMonthButton;
    public RectTransform threeMonthsButton;
    public RectTransform oneYearButton;


    // 1日
    public void SelectOneDay()
    {
        MoveSelectedImage(oneDayButton);
    }

    // 1週間
    public void SelectOneWeek()
    {
        MoveSelectedImage(oneWeekButton);
    }

    // 1か月
    public void SelectOneMonth()
    {
        MoveSelectedImage(oneMonthButton);
    }

    // 3か月
    public void SelectThreeMonths()
    {
        MoveSelectedImage(threeMonthsButton);
    }

    // 1年
    public void SelectOneYear()
    {
        MoveSelectedImage(oneYearButton);
    }


    // 紫画像を選択したボタンの位置へ移動
    private void MoveSelectedImage(RectTransform targetButton)
    {
        if (periodSelectedImage == null || targetButton == null)
        {
            return;
        }

        periodSelectedImage.position = targetButton.position;
    }
}
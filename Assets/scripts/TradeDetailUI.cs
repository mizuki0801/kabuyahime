
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeDetailUI : MonoBehaviour
{
    [Header("実現損益")]
    [SerializeField] private Image profitPanel;
    [SerializeField] private TMP_Text realizedProfitValueText;

    [Header("背景色設定")]
    [SerializeField] private Color profitBackgroundColor =
        new Color32(170, 225, 180, 255);

    [SerializeField] private Color lossBackgroundColor =
        new Color32(245, 165, 165, 255);

    [SerializeField] private Color zeroBackgroundColor =
        new Color32(210, 210, 210, 255);

    [Header("文字色設定")]
    [SerializeField] private Color profitTextColor =
        new Color32(22, 128, 60, 255);

    [SerializeField] private Color lossTextColor =
        new Color32(200, 50, 50, 255);

    [SerializeField] private Color zeroTextColor =
        new Color32(85, 85, 85, 255);

    public void SetProfit(double realizedProfit)
    {
        realizedProfitValueText.text =
            $"{realizedProfit:+#,##0;-#,##0;0}円";

        if (realizedProfit > 0)
        {
            profitPanel.color = profitBackgroundColor;
            realizedProfitValueText.color = profitTextColor;
        }
        else if (realizedProfit < 0)
        {
            profitPanel.color = lossBackgroundColor;
            realizedProfitValueText.color = lossTextColor;
        }
        else
        {
            profitPanel.color = zeroBackgroundColor;
            realizedProfitValueText.color = zeroTextColor;
        }
    }
}

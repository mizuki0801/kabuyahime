
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeHistoryItemUI : MonoBehaviour
{
    public TMP_Text tradeTypeText;
    public TMP_Text stockNameText;
    public TMP_Text tradeDateText;
    public TMP_Text tradeResultText;

    public Button button;

    public void Setup(
        string type,
        string stockName,
        string dateTime,
        int quantity,
        double realizedProfit)
    {
        bool isBuy = type == "buy";

        tradeTypeText.text = isBuy ? "購入" : "売却";
        stockNameText.text = stockName;
        tradeDateText.text = $"{dateTime} ・ {quantity}株";

        if (isBuy)
        {
            tradeResultText.text = "";
        }
        else
        {
            tradeResultText.text = $"{realizedProfit:+#,##0;-#,##0;0}円";
            tradeResultText.color =
                realizedProfit >= 0
                ? new Color32(34, 139, 34, 255)
                : new Color32(200, 50, 50, 255);
        }
    }
}

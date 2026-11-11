using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellQuantityController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text quantityText;
    public Button minusButton;
    public Button plusButton;
    public Button sellAllButton;
    public TMP_Text estimatedAmountText;
    public TMP_Text profitLossText;
    public Button confirmSellButton;

    [Header("Profit / Loss Colors")]
    public Color profitColor = new Color32(46, 180, 100, 255);
    public Color lossColor = new Color32(220, 65, 65, 255);
    public Color neutralColor = Color.white;

    [Header("Test Stock Data (fallback display only)")]
    public int holdingQuantity = 100;
    public int sellQuantity = 10;
    public int currentPrice = 3500;
    public float averagePurchasePrice = 3472f;

    [Header("Data References")]
    public TradeController tradeController;
    public string selectedTicker = "7203.T";

    private bool hasHolding;
    private bool hasApiPrice;

    private void Start()
    {
        if (tradeController == null)
            tradeController = FindFirstObjectByType<TradeController>();

        if (minusButton != null) minusButton.onClick.AddListener(DecreaseQuantity);
        if (plusButton != null) plusButton.onClick.AddListener(IncreaseQuantity);
        if (sellAllButton != null) sellAllButton.onClick.AddListener(SellAll);
        // ConfirmSellButton is already wired through Inspector On Click().
        // Do not register another listener here or the sale may execute twice.

        RefreshData();
    }

    private void Update()
    {
        // API株価が届いた時に表示を更新する。JSONは毎フレーム読まない。
        if (tradeController == null) return;
        int latestPrice;
        bool ready = tradeController.TryGetCurrentStockPrice(selectedTicker, out latestPrice);
        if (ready != hasApiPrice || (ready && latestPrice != currentPrice))
        {
            hasApiPrice = ready;
            if (ready) currentPrice = latestPrice;
            UpdateDisplay();
        }
    }

    public void RefreshData()
    {
        hasHolding = false;
        hasApiPrice = false;
        if (tradeController != null)
        {
            string stockName;
            int count;
            float average;
            if (tradeController.TryGetHolding(selectedTicker, out stockName, out count, out average))
            {
                hasHolding = true;
                holdingQuantity = count;
                averagePurchasePrice = average;
            }
            else
            {
                holdingQuantity = 0;
            }

            int apiPrice;
            hasApiPrice = tradeController.TryGetCurrentStockPrice(selectedTicker, out apiPrice);
            if (hasApiPrice) currentPrice = apiPrice;
        }
        sellQuantity = Mathf.Clamp(sellQuantity, holdingQuantity > 0 ? 1 : 0, Mathf.Max(0, holdingQuantity));
        UpdateDisplay();
    }

    public void IncreaseQuantity()
    {
        sellQuantity = Mathf.Min(holdingQuantity, sellQuantity + 1);
        UpdateDisplay();
    }

    public void DecreaseQuantity()
    {
        sellQuantity = Mathf.Max(holdingQuantity > 0 ? 1 : 0, sellQuantity - 1);
        UpdateDisplay();
    }

    public void SellAll()
    {
        sellQuantity = holdingQuantity;
        UpdateDisplay();
    }

    public void ConfirmSell()
    {
        if (tradeController == null || !hasHolding || !hasApiPrice ||
            sellQuantity <= 0 || sellQuantity > holdingQuantity)
        {
            Debug.LogWarning("【売却不可】保有株・数量・API株価を確認してください");
            return;
        }

        if (tradeController.SellStock(selectedTicker, sellQuantity))
        {
            tradeController.RefreshCoins();
            RefreshData();
        }
    }

    private void UpdateDisplay()
    {
        if (quantityText != null) quantityText.text = sellQuantity.ToString("N0") + "株";
        if (minusButton != null) minusButton.interactable = sellQuantity > 1;
        if (plusButton != null) plusButton.interactable = sellQuantity < holdingQuantity;
        if (sellAllButton != null) sellAllButton.interactable = holdingQuantity > 0;

        if (estimatedAmountText != null)
            estimatedAmountText.text = hasApiPrice ?
                ((long)currentPrice * sellQuantity).ToString("N0") + "円" : "株価取得待ち";

        if (profitLossText != null)
        {
            if (hasApiPrice && hasHolding && sellQuantity > 0)
            {
                double profit = (currentPrice - (double)averagePurchasePrice) * sellQuantity;
                profitLossText.text = (profit > 0 ? "+" : "") + profit.ToString("N0") + "円";

                if (profit > 0)
                    profitLossText.color = profitColor;
                else if (profit < 0)
                    profitLossText.color = lossColor;
                else
                    profitLossText.color = neutralColor;
            }
            else
            {
                profitLossText.text = "--";
                profitLossText.color = neutralColor;
            }
        }

        if (confirmSellButton != null)
            confirmSellButton.interactable = tradeController != null && hasHolding && hasApiPrice &&
                sellQuantity > 0 && sellQuantity <= holdingQuantity;
    }
}

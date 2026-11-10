
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellQuantityController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button sellAllButton;

    [SerializeField] private TMP_Text estimatedAmountText;
    [SerializeField] private TMP_Text profitLossText;

    [Header("Test Stock Data")]
    [SerializeField] private int holdingQuantity = 100;
    [SerializeField] private int sellQuantity = 10;
    [SerializeField] private float currentPrice = 3500f;
    [SerializeField] private float averagePurchasePrice = 3472f;

    [Header("Data References")]
    [SerializeField] private TradeController tradeController;
    [SerializeField] private string selectedTicker = "7203.T";

    private void Start()
    {
        minusButton.onClick.AddListener(Decrease);
        plusButton.onClick.AddListener(Increase);
        sellAllButton.onClick.AddListener(SellAll);

        holdingQuantity = Mathf.Max(0, holdingQuantity);
        sellQuantity = holdingQuantity > 0
            ? Mathf.Clamp(sellQuantity, 1, holdingQuantity)
            : 0;

        LoadSelectedHolding();
        UpdateUI();
    }

    private void Update()
    {
        if (tradeController == null)
            return;

        if (tradeController.TryGetCurrentStockPrice(
            selectedTicker,
            out int apiPrice))
        {
            if (!Mathf.Approximately(currentPrice, apiPrice))
            {
                currentPrice = apiPrice;
                UpdateUI();

                Debug.Log(
                    $"【売却株価更新】{selectedTicker} / {currentPrice:N0}円"
                );
            }
        }
    }


    private void Decrease()
    {
        sellQuantity = Mathf.Max(
            holdingQuantity > 0 ? 1 : 0,
            sellQuantity - 1
        );
        UpdateUI();
    }

    private void Increase()
    {
        sellQuantity = Mathf.Min(
            holdingQuantity,
            sellQuantity + 1
        );
        UpdateUI();
    }

    private void SellAll()
    {
        sellQuantity = holdingQuantity;
        UpdateUI();
    }

    private void UpdateUI()
    {
        double estimatedAmount = currentPrice * sellQuantity;

        double profitLoss =
            (currentPrice - averagePurchasePrice) * sellQuantity;

        if (quantityText != null)
            quantityText.text = $"{sellQuantity}株";

        if (estimatedAmountText != null)
            estimatedAmountText.text =
                $"売却予定額：{estimatedAmount:N0}円";

        if (profitLossText != null)
        {
            string sign = profitLoss > 0 ? "+" : "";

            profitLossText.text =
                $"実現損益：{sign}{profitLoss:N0}円";

            profitLossText.color = profitLoss > 0
                ? new Color(0.1f, 0.55f, 0.3f)
                : profitLoss < 0
                    ? new Color(0.8f, 0.2f, 0.2f)
                    : Color.gray;
        }
    }

    private void LoadSelectedHolding()
    {
        if (tradeController == null)
        {
            Debug.LogError("【売却】TradeControllerが未設定です");
            return;
        }

        if (tradeController.TryGetHolding(
            selectedTicker,
            out string stockName,
            out int quantity,
            out float averagePrice))
        {
            holdingQuantity = quantity;
            averagePurchasePrice = averagePrice;

            sellQuantity = Mathf.Min(10, holdingQuantity);

            Debug.Log(
                $"【保有株取得成功】{stockName} / " +
                $"{holdingQuantity}株 / 平均取得単価:{averagePurchasePrice:N2}円"
            );
        }
        else
        {
            Debug.LogWarning(
                $"【売却】保有株が見つかりません: {selectedTicker}"
            );
        }
    }
}

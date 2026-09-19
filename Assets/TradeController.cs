using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeController : MonoBehaviour
{
    // テキスト
    public TMP_Text quantityText;
    public TMP_Text estimatedPriceText;
    public TMP_Text buyingPowerText;

    public TMP_Text selectedStockNameText;
    public TMP_Text selectedStockPriceText;

    // チャート表示
    public Image priceChartImage;

    // 各銘柄のチャート画像
    public Sprite toyotaChart;
    public Sprite sonyChart;
    public Sprite nintendoChart;
    public Sprite mufgChart;
    public Sprite keyenceChart;

    // 初期値
    private int quantity = 100;
    private int stockPrice = 3245;
    private int buyingPower = 1024500;

    // ＋
    public void IncreaseQuantity()
    {
        quantity += 10;
        UpdateQuantityText();
    }

    // −
    public void DecreaseQuantity()
    {
        quantity -= 10;

        if (quantity < 10)
        {
            quantity = 10;
        }

        UpdateQuantityText();
    }

    // +10株
    public void Add10()
    {
        quantity += 10;
        UpdateQuantityText();
    }

    // +50株
    public void Add50()
    {
        quantity += 50;
        UpdateQuantityText();
    }

    // +100株
    public void Add100()
    {
        quantity += 100;
        UpdateQuantityText();
    }

    // 最大
    public void SetMax()
    {
        quantity = 1000;
        UpdateQuantityText();
    }

    // 購入
    public void BuyStock()
    {
        int totalPrice = stockPrice * quantity;

        if (totalPrice <= buyingPower)
        {
            buyingPower -= totalPrice;

            buyingPowerText.text =
                "¥" + buyingPower.ToString("N0");
        }
    }

    // トヨタ
    public void SelectToyota()
    {
        stockPrice = 3245;
        quantity = 100;

        selectedStockNameText.text =
            "トヨタ自動車（7203）";

        selectedStockPriceText.text =
            "¥3,245\n+2.36%";

        priceChartImage.sprite = toyotaChart;

        UpdateQuantityText();
    }

    // ソニー
    public void SelectSony()
    {
        stockPrice = 3512;
        quantity = 100;

        selectedStockNameText.text =
            "ソニーグループ（6758）";

        selectedStockPriceText.text =
            "¥3,512\n+1.21%";

        priceChartImage.sprite = sonyChart;

        UpdateQuantityText();
    }

    // 任天堂
    public void SelectNintendo()
    {
        stockPrice = 7820;
        quantity = 100;

        selectedStockNameText.text =
            "任天堂（7974）";

        selectedStockPriceText.text =
            "¥7,820\n-0.81%";

        priceChartImage.sprite = nintendoChart;

        UpdateQuantityText();
    }

    // 三菱UFJ
    public void SelectMufg()
    {
        stockPrice = 1567;
        quantity = 100;

        selectedStockNameText.text =
            "三菱UFJ FG（8306）";

        selectedStockPriceText.text =
            "¥1,567\n+0.45%";

        priceChartImage.sprite = mufgChart;

        UpdateQuantityText();
    }

    // キーエンス
    public void SelectKeyence()
    {
        stockPrice = 69250;
        quantity = 100;

        selectedStockNameText.text =
            "キーエンス（6861）";

        selectedStockPriceText.text =
            "¥69,250\n+0.92%";

        priceChartImage.sprite = keyenceChart;

        UpdateQuantityText();
    }

    // 数量と予想購入金額の更新
    private void UpdateQuantityText()
    {
        quantityText.text = quantity + "株";

        int totalPrice = stockPrice * quantity;

        estimatedPriceText.text =
            "¥" + totalPrice.ToString("N0");
    }
}
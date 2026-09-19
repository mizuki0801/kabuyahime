using TMPro;
using UnityEngine;

public class TradeController : MonoBehaviour
{
    [Header("注文")]
    public TMP_Text quantityText;
    public TMP_Text estimatedPriceText;
    public TMP_Text buyingPowerText;

    [Header("選択中の企業")]
    public TMP_Text selectedStockNameText;
    public TMP_Text companyDescriptionText;

    private int quantity = 100;
    private int stockPrice = 0;
    private int buyingPower = 1024500;


    // =========================
    // 注文株数
    // =========================

    public void IncreaseQuantity()
    {
        quantity += 10;
        UpdateQuantityText();
    }

    public void DecreaseQuantity()
    {
        quantity -= 10;

        if (quantity < 10)
        {
            quantity = 10;
        }

        UpdateQuantityText();
    }

    public void Add10()
    {
        quantity += 10;
        UpdateQuantityText();
    }

    public void Add50()
    {
        quantity += 50;
        UpdateQuantityText();
    }

    public void Add100()
    {
        quantity += 100;
        UpdateQuantityText();
    }

    public void SetMax()
    {
        if (stockPrice > 0)
        {
            quantity = buyingPower / stockPrice;
        }

        UpdateQuantityText();
    }


    // =========================
    // 購入
    // =========================

    public void BuyStock()
    {
        int totalPrice = stockPrice * quantity;

        if (totalPrice <= buyingPower)
        {
            buyingPower -= totalPrice;

            if (buyingPowerText != null)
            {
                buyingPowerText.text =
                    buyingPower.ToString("N0");
            }
        }
    }


    // =========================
    // トヨタ自動車
    // =========================

    public void SelectToyota()
    {
        stockPrice = 3245;
        quantity = 100;

        selectedStockNameText.text =
            "トヨタ自動車（7203）";

        companyDescriptionText.text =
            "トヨタ自動車は、自動車の開発・生産・販売を行う企業です。\n\n" +
            "乗用車をはじめ、さまざまな車両を世界各地で展開しています。";

        UpdateQuantityText();
    }


    // =========================
    // ソニーグループ
    // =========================

    public void SelectSony()
    {
        stockPrice = 3512;
        quantity = 100;

        selectedStockNameText.text =
            "ソニーグループ（6758）";

        companyDescriptionText.text =
            "ソニーグループは、ゲーム、音楽、映画、エレクトロニクスなど、" +
            "幅広い事業を展開する企業です。\n\n" +
            "PlayStationなどのゲーム事業も展開しています。";

        UpdateQuantityText();
    }


    // =========================
    // 任天堂
    // =========================

    public void SelectNintendo()
    {
        stockPrice = 7820;
        quantity = 100;

        selectedStockNameText.text =
            "任天堂（7974）";

        companyDescriptionText.text =
            "任天堂は、ゲーム機やゲームソフトの開発・販売を行う企業です。\n\n" +
            "Nintendo Switchやマリオなど、" +
            "世界的に知られる製品やキャラクターを展開しています。";

        UpdateQuantityText();
    }


    // =========================
    // 三菱UFJフィナンシャル・グループ
    // =========================

    public void SelectMufg()
    {
        stockPrice = 1567;
        quantity = 100;

        selectedStockNameText.text =
            "三菱UFJ FG（8306）";

        companyDescriptionText.text =
            "三菱UFJフィナンシャル・グループは、" +
            "銀行を中心に金融サービスを展開する企業グループです。\n\n" +
            "銀行、信託、証券など幅広い金融事業を行っています。";

        UpdateQuantityText();
    }


    // =========================
    // キーエンス
    // =========================

    public void SelectKeyence()
    {
        stockPrice = 69250;
        quantity = 100;

        selectedStockNameText.text =
            "キーエンス（6861）";

        companyDescriptionText.text =
            "キーエンスは、工場の自動化などに使用される" +
            "センサや測定機器などを開発・販売する企業です。\n\n" +
            "製造現場の自動化や効率化を支える製品を展開しています。";

        UpdateQuantityText();
    }


    // =========================
    // APIから取得した株価を反映
    // =========================

    public void SetStockPrice(float newPrice)
    {
        stockPrice = Mathf.RoundToInt(newPrice);
        UpdateQuantityText();
    }


    // =========================
    // 注文表示更新
    // =========================

    private void UpdateQuantityText()
    {
        if (quantityText != null)
        {
            quantityText.text = quantity + "株";
        }

        int totalPrice = stockPrice * quantity;

        if (estimatedPriceText != null)
        {
            estimatedPriceText.text =
                totalPrice.ToString("N0");
        }
    }
}
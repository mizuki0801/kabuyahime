using System;
using System.IO;
using TMPro;
using UnityEngine;

public class TradeController : MonoBehaviour
{
    [Header("注文")]
    public TMP_Text quantityText;
    public TMP_Text estimatedPriceText;

    [Header("選択中の企業")]
    public TMP_Text selectedStockNameText;
    public TMP_Text companyDescriptionText;

    [Header("JSON設定")]
    [SerializeField] private string jsonFileName = "user.json";
    [SerializeField] private string holdingsFileName = "holdings.json";

    [Header("株価API連携")]
    [SerializeField] private StockApiManager stockApiManager;

    [Header("診断設定")]
    [SerializeField] private bool enableDiagnostics = true;

    [Serializable]
    private class UserData
    {
        public int investmentCoins;
        public int oshiCoins;
    }
    [Serializable]
    private class HoldingData
    {
        public string ticker;
        public string name;
        public int quantity;
        public float averagePurchasePrice;
    }

    [Serializable]
    private class HoldingsData
    {
        public HoldingData[] holdings;
    }

    private int quantity = 100;
    private int stockPrice;
    private int buyingPower;

    private string selectedTicker = "7203.T";
    private string selectedStockName = "トヨタ自動車";
    private bool apiQuoteReady;

    private string jsonPath;
    private bool isDataLoaded;

    private string holdingsJsonPath;
    private HoldingsData holdingsData;

    // =====================================
    // 初期化
    // =====================================

    private void Awake()
    {
        jsonPath = Path.Combine(
            Application.streamingAssetsPath,
            jsonFileName
        );

        holdingsJsonPath = Path.Combine(
            Application.streamingAssetsPath,
            holdingsFileName
        );

        if (enableDiagnostics)
        {
            Debug.Log("【JSON使用先】" + jsonPath);
        }

        LoadCoins();
        if (stockApiManager == null)
            stockApiManager = FindObjectOfType<StockApiManager>();
    }

    private void Start()
    {
        UpdateAllTexts();
    }

    // =====================================
    // 保有株JSON読み込み
    // =====================================

    private bool LoadHoldings()
    {
        try
        {
            if (!File.Exists(holdingsJsonPath))
            {
                Debug.LogError(
                    "【保有株読込失敗】ファイルなし: " +
                    holdingsJsonPath
                );
                return false;
            }

            string json = File.ReadAllText(holdingsJsonPath);

            if (!json.Contains("\"holdings\""))
            {
                Debug.LogError(
                    "【保有株読込失敗】holdings項目がありません"
                );
                return false;
            }

            HoldingsData data =
                JsonUtility.FromJson<HoldingsData>(json);

            if (data == null || data.holdings == null)
            {
                Debug.LogError(
                    "【保有株読込失敗】データが不正です"
                );
                return false;
            }

            foreach (HoldingData holding in data.holdings)
            {
                if (holding == null ||
                    string.IsNullOrEmpty(holding.ticker) ||
                    holding.quantity <= 0 ||
                    holding.averagePurchasePrice < 0)
                {
                    Debug.LogError(
                        "【保有株読込失敗】保有銘柄データが不正です"
                    );
                    return false;
                }
            }

            holdingsData = data;

            if (enableDiagnostics)
            {
                Debug.Log(
                    "【保有株読込成功】銘柄数：" +
                    holdingsData.holdings.Length
                );
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(
                "【保有株読込エラー】" + e.Message
            );
            return false;
        }
    }

    // =====================================
    // 保有株JSON保存
    // =====================================

    private bool SaveHoldings()
    {
        if (holdingsData == null ||
            holdingsData.holdings == null)
        {
            Debug.LogError(
                "【保有株保存失敗】データが未設定です"
            );
            return false;
        }

        try
        {
            if (!File.Exists(holdingsJsonPath))
            {
                Debug.LogError(
                    "【保有株保存失敗】ファイルなし: " +
                    holdingsJsonPath
                );
                return false;
            }

            string json = JsonUtility.ToJson(
                holdingsData,
                true
            );

            File.WriteAllText(holdingsJsonPath, json);

            if (enableDiagnostics)
            {
                Debug.Log("【保有株保存成功】");
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(
                "【保有株保存エラー】" + e.Message
            );
            return false;
        }
    }

    // =====================================
    // 保有株データの購入反映
    // =====================================

    private bool AddHolding(
        string ticker,
        string stockName,
        int purchaseQuantity,
        int purchasePrice
    )
    {
        if (string.IsNullOrEmpty(ticker) ||
            purchaseQuantity <= 0 ||
            purchasePrice <= 0)
        {
            Debug.LogError("【保有株追加失敗】購入データが不正です");
            return false;
        }

        if (!LoadHoldings())
        {
            return false;
        }

        // 既存の保有銘柄を検索
        foreach (HoldingData holding in holdingsData.holdings)
        {
            if (holding.ticker == ticker)
            {
                long newQuantity =
                    (long)holding.quantity + purchaseQuantity;

                if (newQuantity > int.MaxValue)
                {
                    Debug.LogError("【保有株追加失敗】株数が上限を超えています");
                    return false;
                }

                double totalCost =
                    (double)holding.averagePurchasePrice *
                    holding.quantity +
                    (double)purchasePrice * purchaseQuantity;

                holding.averagePurchasePrice =
                    (float)(totalCost / newQuantity);

                holding.quantity = (int)newQuantity;

                return SaveHoldings();
            }
        }

        // 初めて購入する銘柄
        HoldingData newHolding = new HoldingData
        {
            ticker = ticker,
            name = stockName,
            quantity = purchaseQuantity,
            averagePurchasePrice = purchasePrice
        };

        HoldingData[] newArray =
            new HoldingData[holdingsData.holdings.Length + 1];

        Array.Copy(
            holdingsData.holdings,
            newArray,
            holdingsData.holdings.Length
        );

        newArray[newArray.Length - 1] = newHolding;

        holdingsData.holdings = newArray;

        return SaveHoldings();
    }

    // =====================================
    // JSON読み込み
    // =====================================

    private bool LoadCoins()
    {
        isDataLoaded = false;

        try
        {
            if (!File.Exists(jsonPath))
            {
                Debug.LogError(
                    "【JSON読込失敗】ファイルなし: " + jsonPath
                );
                return false;
            }

            string json = File.ReadAllText(jsonPath);

            if (!json.Contains("\"investmentCoins\"") ||
                !json.Contains("\"oshiCoins\""))
            {
                Debug.LogError(
                    "【JSON読込失敗】必要な項目がありません"
                );
                return false;
            }

            UserData data =
                JsonUtility.FromJson<UserData>(json);

            if (data == null ||
                data.investmentCoins < 0 ||
                data.oshiCoins < 0)
            {
                Debug.LogError(
                    "【JSON読込失敗】データが不正です"
                );
                return false;
            }

            buyingPower = data.investmentCoins;
            isDataLoaded = true;

            if (enableDiagnostics)
            {
                Debug.Log(
                    "【投資コイン読込成功】" +
                    buyingPower.ToString("N0")
                );
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(
                "【JSON読込エラー】" + e.Message
            );
            return false;
        }
    }

    // =====================================
    // JSON保存
    // =====================================

    private bool SaveCoins(int newBalance)
    {
        if (!isDataLoaded || newBalance < 0)
        {
            Debug.LogError(
                "【JSON保存失敗】残高または読込状態が不正です"
            );
            return false;
        }

        try
        {
            if (!File.Exists(jsonPath))
            {
                Debug.LogError(
                    "【JSON保存失敗】ファイルなし: " + jsonPath
                );
                return false;
            }

            string json = File.ReadAllText(jsonPath);

            UserData data =
                JsonUtility.FromJson<UserData>(json);

            if (data == null)
            {
                Debug.LogError(
                    "【JSON保存失敗】データ変換に失敗しました"
                );
                return false;
            }

            // 投資コインのみ変更
            // 推しコインは変更しない
            data.investmentCoins = newBalance;

            File.WriteAllText(
                jsonPath,
                JsonUtility.ToJson(data, true)
            );

            buyingPower = newBalance;

            if (enableDiagnostics)
            {
                Debug.Log(
                    "【投資コイン保存成功】" +
                    buyingPower.ToString("N0")
                );
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(
                "【JSON保存エラー】" + e.Message
            );
            return false;
        }
    }

    // =====================================
    // 注文操作
    // =====================================

    private void OnOrderChanged()
    {
        UpdateAllTexts();
    }

    public void IncreaseQuantity()
    {
        if (quantity <= int.MaxValue - 10)
        {
            quantity += 10;
        }

        OnOrderChanged();
    }

    public void DecreaseQuantity()
    {
        quantity = Mathf.Max(10, quantity - 10);
        OnOrderChanged();
    }

    public void Add10()
    {
        IncreaseQuantity();
    }

    public void Add50()
    {
        if (quantity <= int.MaxValue - 50)
        {
            quantity += 50;
        }

        OnOrderChanged();
    }

    public void Add100()
    {
        if (quantity <= int.MaxValue - 100)
        {
            quantity += 100;
        }

        OnOrderChanged();
    }

    public void SetMax()
    {
        if (!LoadCoins() || stockPrice <= 0)
        {
            Debug.LogWarning(
                "【最大購入】株価または残高が未設定です"
            );
            return;
        }

        quantity = buyingPower / stockPrice;
        OnOrderChanged();
    }

    // =====================================
    // 株式購入
    // =====================================

    public void BuyStock()
    {
        if (!apiQuoteReady || stockPrice <= 0 || quantity <= 0 ||
            string.IsNullOrEmpty(selectedTicker))
        {
            Debug.LogWarning("【購入失敗】銘柄・株価・株数が不正です");
            return;
        }

        if (!LoadCoins() || !LoadHoldings())
        {
            Debug.LogError("【購入失敗】JSONを読み込めません");
            return;
        }

        long totalPrice = (long)stockPrice * quantity;
        if (totalPrice > buyingPower)
        {
            Debug.LogWarning(
                $"【購入失敗】残高不足 必要:{totalPrice:N0} 所持:{buyingPower:N0}"
            );
            UpdateAllTexts();
            return;
        }

        // 両方のJSONをバックアップしてから更新する
        string originalUserJson;
        string originalHoldingsJson;
        try
        {
            originalUserJson = File.ReadAllText(jsonPath);
            originalHoldingsJson = File.ReadAllText(holdingsJsonPath);
        }
        catch (Exception e)
        {
            Debug.LogError("【購入失敗】バックアップ作成エラー: " + e.Message);
            return;
        }

        int newBalance = buyingPower - (int)totalPrice;

        // 保有株を先に保存し、次に投資コインを保存する
        bool holdingsSaved = AddHolding(
            selectedTicker, selectedStockName, quantity, stockPrice
        );

        if (!holdingsSaved)
        {
            Debug.LogError("【購入失敗】保有株の保存に失敗しました");
            RestorePurchaseFiles(originalUserJson, originalHoldingsJson);
            return;
        }

        if (!SaveCoins(newBalance))
        {
            Debug.LogError("【購入失敗】投資コインの保存に失敗しました");
            RestorePurchaseFiles(originalUserJson, originalHoldingsJson);
            return;
        }

        UpdateAllTexts();
        Debug.Log(
            $"【購入成功】銘柄:{selectedTicker} 株数:{quantity} " +
            $"購入単価:{stockPrice:N0} 購入金額:{totalPrice:N0} " +
            $"残高:{buyingPower:N0}"
        );
    }

    // 保存に失敗したとき、両方のJSONを元に戻す（ベストエフォート）
    private void RestorePurchaseFiles(string userBackup, string holdingsBackup)
    {
        bool restored = true;
        try
        {
            File.WriteAllText(jsonPath, userBackup);
        }
        catch (Exception e)
        {
            restored = false;
            Debug.LogError("【復元失敗】user.json: " + e.Message);
        }

        try
        {
            File.WriteAllText(holdingsJsonPath, holdingsBackup);
        }
        catch (Exception e)
        {
            restored = false;
            Debug.LogError("【復元失敗】holdings.json: " + e.Message);
        }

        LoadCoins();
        LoadHoldings();
        UpdateAllTexts();

        if (restored)
            Debug.LogWarning("【購入取消】両方のJSONを元に戻しました");
        else
            Debug.LogError("【要確認】JSON復元に失敗したため手動確認が必要です");
    }

    // =====================================
    // =====================================
    // UI表示
    // =====================================

    private void UpdateAllTexts()
    {
        // 注文株数
        if (quantityText != null)
        {
            quantityText.text = quantity + "株";
        }

        // 買付可能額
        long totalPrice = (long)stockPrice * quantity;

        if (estimatedPriceText != null)
        {
            estimatedPriceText.text =
                totalPrice.ToString("N0");
        }

        // BuyingPowerValueには一切触れない
        // UserCoinDisplay.csがJSONから表示する
    }

    // =====================================
    // 外部連携
    // =====================================

    public void SetCoins(int coins)
    {
        if (!LoadCoins())
        {
            Debug.LogWarning(
                "【コイン変更失敗】JSON未読込"
            );
            return;
        }

        if (SaveCoins(Mathf.Max(0, coins)))
        {
            UpdateAllTexts();
        }
    }

    public int GetCoins()
    {
        return buyingPower;
    }

    public void RefreshCoins()
    {
        LoadCoins();
        UpdateAllTexts();
    }

    // API取得を開始する時点で、古い株価による購入を禁止する。
    // StockApiManager側のボタンから直接呼ばれても銘柄を同期する。
    public void BeginQuoteRequest(string ticker)
    {
        string normalized = NormalizeTicker(ticker);
        if (string.IsNullOrEmpty(normalized)) return;

        bool changedTicker = selectedTicker != normalized;
        selectedTicker = normalized;
        apiQuoteReady = false;
        stockPrice = 0;

        switch (normalized)
        {
            case "7203.T": selectedStockName = "トヨタ自動車（7203）"; break;
            case "6758.T": selectedStockName = "ソニーグループ（6758）"; break;
            case "7974.T": selectedStockName = "任天堂（7974）"; break;
            case "8306.T": selectedStockName = "三菱UFJ FG（8306）"; break;
            case "6861.T": selectedStockName = "キーエンス（6861）"; break;
            default: selectedStockName = normalized; break;
        }

        if (selectedStockNameText != null)
            selectedStockNameText.text = selectedStockName;

        // API側だけで銘柄変更された場合、前の企業説明を残さない。
        if (changedTicker && companyDescriptionText != null)
            companyDescriptionText.text = "株価を取得しています...";

        OnOrderChanged();
    }

    private static string NormalizeTicker(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker)) return "";
        string code = ticker.Trim().ToUpperInvariant();
        return code.EndsWith(".T", StringComparison.Ordinal) ? code : code + ".T";
    }

    // APIレスポンスの銘柄コードと選択銘柄が一致する場合だけ反映する。
    public void ApplyApiStockPrice(string ticker, float newPrice)
    {
        if (NormalizeTicker(ticker) != selectedTicker)
        {
            Debug.LogWarning("【株価無視】現在選択中の銘柄と異なります: " + ticker);
            return;
        }
        if (float.IsNaN(newPrice) || float.IsInfinity(newPrice) || newPrice <= 0 ||
            newPrice > int.MaxValue)
        {
            Debug.LogError("【株価取得失敗】API株価が不正です");
            apiQuoteReady = false;
            stockPrice = 0;
            OnOrderChanged();
            return;
        }

        stockPrice = Mathf.RoundToInt(newPrice);
        apiQuoteReady = stockPrice > 0;
        OnOrderChanged();
        if (enableDiagnostics)
            Debug.Log($"【API株価反映】{selectedTicker} / {stockPrice:N0}円");
    }

    // 旧スクリプトからの呼び出し互換用。銘柄を検証できないため購入は許可しない。
    public void SetStockPrice(float newPrice)
    {
        apiQuoteReady = false;
        stockPrice = 0;
        Debug.LogWarning("SetStockPriceは購入用に使用できません。ApplyApiStockPriceを使用してください。");
        OnOrderChanged();
    }

    // =====================================
    // 銘柄選択
    // =====================================

    public void SelectToyota()
    {
        SetSelectedStock(
            3245,
            "7203.T",
            "トヨタ自動車（7203）",
            "トヨタ自動車は、自動車の開発・生産・販売を行う企業です。\n\n" +
            "乗用車をはじめ、さまざまな車両を世界各地で展開しています。"
        );
    }

    public void SelectSony()
    {
        SetSelectedStock(
            3512,
            "6758.T",
            "ソニーグループ（6758）",
            "ソニーグループは、ゲーム、音楽、映画、エレクトロニクスなど、" +
            "幅広い事業を展開する企業です。\n\n" +
            "PlayStationなどのゲーム事業も展開しています。"
        );
    }

    public void SelectNintendo()
    {
        SetSelectedStock(
            7820,
            "7974.T",
            "任天堂（7974）",
            "任天堂は、ゲーム機やゲームソフトの開発・販売を行う企業です。\n\n" +
            "Nintendo Switchやマリオなど、世界的に知られる製品やキャラクターを展開しています。"
        );
    }

    public void SelectMufg()
    {
        SetSelectedStock(
            1567,
            "8306.T",
            "三菱UFJ FG（8306）",
            "三菱UFJフィナンシャル・グループは、銀行を中心に金融サービスを展開する企業グループです。\n\n" +
            "銀行、信託、証券など幅広い金融事業を行っています。"
        );
    }

    public void SelectKeyence()
    {
        SetSelectedStock(
            69250,
            "6861.T",
            "キーエンス（6861）",
            "キーエンスは、工場の自動化などに使用されるセンサや測定機器などを開発・販売する企業です。\n\n" +
            "製造現場の自動化や効率化を支える製品を展開しています。"
        );
    }

    private void SetSelectedStock(
        int price,
        string ticker,
        string stockName,
        string description
    )
    {
        selectedTicker = ticker;
        selectedStockName = stockName;

        // 固定価格は購入に使わない。API取得完了まで注文を無効にする。
        stockPrice = 0;
        apiQuoteReady = false;
        quantity = 100;

        if (selectedStockNameText != null)
        {
            selectedStockNameText.text = stockName;
        }

        if (companyDescriptionText != null)
        {
            companyDescriptionText.text = description;
        }

        OnOrderChanged();

        if (stockApiManager == null)
            stockApiManager = FindObjectOfType<StockApiManager>();

        if (stockApiManager != null)
            stockApiManager.GetStockPrice(ticker);
        else
            Debug.LogError("【株価取得不可】StockApiManagerが見つかりません。購入は無効です。");
    }
}

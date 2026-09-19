using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class MarketPanelController : MonoBehaviour
{
    [Header("トヨタ自動車")]
    public TMP_Text toyotaPrice;
    public TMP_Text toyotaChange;
    public MiniStockChartRenderer toyotaGraph;

    [Header("任天堂")]
    public TMP_Text nintendoPrice;
    public TMP_Text nintendoChange;
    public MiniStockChartRenderer nintendoGraph;

    [Header("ソニーグループ")]
    public TMP_Text sonyPrice;
    public TMP_Text sonyChange;
    public MiniStockChartRenderer sonyGraph;

    private string quoteBaseUrl =
        "http://localhost:8000/api/v1/quotes/";

    private string historyBaseUrl =
        "http://localhost:8000/api/v1/history/";

    void Start()
    {
        // 現在株価・騰落率
        StartCoroutine(
            GetStockData("7203", toyotaPrice, toyotaChange)
        );

        StartCoroutine(
            GetStockData("7974", nintendoPrice, nintendoChange)
        );

        StartCoroutine(
            GetStockData("6758", sonyPrice, sonyChange)
        );

        // ミニグラフ
        StartCoroutine(
            GetMiniChart("7203", toyotaGraph)
        );

        StartCoroutine(
            GetMiniChart("7974", nintendoGraph)
        );

        StartCoroutine(
            GetMiniChart("6758", sonyGraph)
        );
    }

    // =========================
    // 現在株価・騰落率
    // =========================
    private IEnumerator GetStockData(
        string ticker,
        TMP_Text priceText,
        TMP_Text changeText
    )
    {
        string apiUrl = quoteBaseUrl + ticker;

        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                MarketStockData data =
                    JsonUtility.FromJson<MarketStockData>(
                        request.downloadHandler.text
                    );

                if (priceText != null)
                {
                    priceText.text =
                        "¥" + data.price.ToString("N0");
                }

                if (changeText != null)
                {
                    changeText.text =
                        data.change_percent
                            .ToString("+0.00;-0.00;0.00")
                        + "%";

                    if (data.change_percent > 0)
                    {
                        changeText.color =
                            new Color32(70, 190, 80, 255);
                    }
                    else if (data.change_percent < 0)
                    {
                        changeText.color =
                            new Color32(220, 70, 70, 255);
                    }
                    else
                    {
                        changeText.color =
                            new Color32(80, 80, 80, 255);
                    }
                }

                Debug.Log(
                    ticker +
                    " 市場データ取得成功：" +
                    data.price +
                    " / " +
                    data.change_percent +
                    "%"
                );
            }
            else
            {
                Debug.LogError(
                    ticker +
                    " 市場データ取得失敗：" +
                    request.error
                );
            }
        }
    }

    // =========================
    // ミニグラフ
    // =========================
    private IEnumerator GetMiniChart(
        string ticker,
        MiniStockChartRenderer graph
    )
    {
        string endDate =
            System.DateTime.Now.ToString("yyyy-MM-dd");

        string startDate =
            System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

        string apiUrl =
            historyBaseUrl +
            ticker +
            "?start=" +
            startDate +
            "&end=" +
            endDate +
            "&interval=5m";

        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                MarketHistoryData data =
                    JsonUtility.FromJson<MarketHistoryData>(
                        request.downloadHandler.text
                    );

                if (data != null &&
                    data.bars != null &&
                    data.bars.Length >= 2)
                {
                    float[] prices =
                        new float[data.bars.Length];

                    for (int i = 0; i < data.bars.Length; i++)
                    {
                        prices[i] = data.bars[i].close;
                    }

                    if (graph != null)
                    {
                        graph.DrawChart(prices);
                    }

                    Debug.Log(
                        ticker +
                        " ミニグラフ取得成功：" +
                        prices.Length +
                        "件"
                    );
                }
                else
                {
                    Debug.LogWarning(
                        ticker +
                        " のグラフデータがありません"
                    );
                }
            }
            else
            {
                Debug.LogError(
                    ticker +
                    " ミニグラフ取得失敗：" +
                    request.error
                );
            }
        }
    }
}

[System.Serializable]
public class MarketStockData
{
    public string ticker;
    public string name;
    public float price;
    public float previous_close;
    public float change;
    public float change_percent;
}

[System.Serializable]
public class MarketHistoryData
{
    public string ticker;
    public string name;
    public string currency;
    public string interval;
    public MarketHistoryBar[] bars;
}

[System.Serializable]
public class MarketHistoryBar
{
    public string ts;
    public string date;
    public float open;
    public float high;
    public float low;
    public float close;
    public long volume;
}
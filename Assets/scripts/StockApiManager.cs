using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class StockApiManager : MonoBehaviour
{
    [Header("���݊���")]
    public TMP_Text stockPriceText;
    public TMP_Text stockChangeText;

    [Header("株価の取引日時")]
    public TMP_Text stockMarketTimeText;

    [Header("����")]
    public TradeController tradeController;

    [Header("�`���[�g")]
    public StockChartRenderer stockChartRenderer;

    private string quoteBaseUrl =
        "http://localhost:8000/api/v1/quotes/";

    private string historyBaseUrl =
        "http://localhost:8000/api/v1/history/";

    private string currentTicker = "7203";
    private int latestQuoteRequestId;
    private int latestHistoryRequestId;

    private static string NormalizeTicker(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker)) return "";
        string code = ticker.Trim().ToUpperInvariant();
        return code.EndsWith(".T", StringComparison.Ordinal) ? code : code + ".T";
    }

    // 既存のFastAPIエンドポイントは数字4桁を受け取る想定。
    private static string ApiTicker(string ticker)
    {
        string normalized = NormalizeTicker(ticker);
        return normalized.EndsWith(".T", StringComparison.Ordinal)
            ? normalized.Substring(0, normalized.Length - 2)
            : normalized;
    }

    private ChartPeriod currentPeriod = ChartPeriod.OneMonth;

    private enum ChartPeriod
    {
        OneDay,
        OneWeek,
        OneMonth,
        ThreeMonths,
        OneYear
    }


    // =====================================
    // =====================================

    void Start()
    {
        GetStockPrice(currentTicker);

        ShowOneMonth();
    }


    // =====================================
    // =====================================

    public void GetStockPrice(string ticker)
    {
        string normalized = NormalizeTicker(ticker);
        if (string.IsNullOrEmpty(normalized))
        {
            Debug.LogError("銘柄コードが空です");
            return;
        }

        currentTicker = normalized;
        int requestId = ++latestQuoteRequestId;

        if (tradeController != null)
            tradeController.BeginQuoteRequest(normalized);

        StartCoroutine(GetStockPriceCoroutine(normalized, requestId));

        RefreshCurrentChart();
    }


    // =====================================
    // =====================================


    // =====================================
    // 現在株価API
    // =====================================

    IEnumerator GetStockPriceCoroutine(string ticker, int requestId)
    {
        string apiUrl = quoteBaseUrl + ApiTicker(ticker);

        using (UnityWebRequest request =
               UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            // 古い銘柄のレスポンスで現在の価格や時刻を上書きしない。
            if (requestId != latestQuoteRequestId || ticker != currentTicker)
                yield break;

            if (request.result ==
                UnityWebRequest.Result.Success)
            {
                StockData data =
                    JsonUtility.FromJson<StockData>(
                        request.downloadHandler.text
                    );

                if (data == null ||
                    (!string.IsNullOrEmpty(data.ticker) && NormalizeTicker(data.ticker) != ticker) ||
                    float.IsNaN(data.price) || float.IsInfinity(data.price) || data.price <= 0)
                {
                    Debug.LogError("株価データが不正、または銘柄が一致しません");
                    yield break;
                }

                // 株価表示
                if (stockPriceText != null)
                {
                    stockPriceText.text =
                        data.price.ToString("N0");
                }

                // 前日比表示
                if (stockChangeText != null)
                {
                    stockChangeText.text =
                        data.change_percent.ToString(
                            "+0.00;-0.00;0.00"
                        ) + "%";
                }

                // 株価の取引日時を日本時間で表示
                UpdateMarketTime(data.market_time);

                // 売買画面の株価を更新
                if (tradeController != null)
                {
                    tradeController.ApplyApiStockPrice(ticker, data.price);
                }

                Debug.Log(
                    "現在株価取得成功：" +
                    data.ticker +
                    " / 株価：" +
                    data.price +
                    " / 取引日時：" +
                    data.market_time
                );
            }
            else
            {
                Debug.LogError(
                    "株価取得失敗：" +
                    request.error
                );
            }
        }
    }

    // =====================================
    // 取引日時を日本時間で表示
    // =====================================

    private void UpdateMarketTime(string marketTime)
    {
        if (stockMarketTimeText == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(marketTime))
        {
            stockMarketTimeText.text = "株価更新：不明";
            return;
        }

        if (DateTimeOffset.TryParse(
            marketTime,
            out DateTimeOffset utcTime))
        {
            DateTimeOffset japanTime =
                utcTime.ToUniversalTime().ToOffset(
                    TimeSpan.FromHours(9)
                );

            stockMarketTimeText.text =
                "株価更新：" +
                japanTime.ToString("yyyy/MM/dd HH:mm");
        }
        else
        {
            stockMarketTimeText.text = "株価更新：不明";
        }
    }



    // =====================================
    // =====================================

    public void ShowOneDay()
    {
        currentPeriod = ChartPeriod.OneDay;

        DateTime endDate =
            DateTime.Now.AddDays(1);

        DateTime startDate =
            DateTime.Now.AddDays(-1);

        GetHistory(
            startDate,
            endDate,
            "5m"
        );
    }


    // =====================================
    // =====================================

    public void ShowOneWeek()
    {
        currentPeriod = ChartPeriod.OneWeek;

        DateTime endDate =
            DateTime.Now.AddDays(1);

        DateTime startDate =
            DateTime.Now.AddDays(-7);

        GetHistory(
            startDate,
            endDate,
            "30m"
        );
    }


    // =====================================
    // =====================================

    public void ShowOneMonth()
    {
        currentPeriod = ChartPeriod.OneMonth;

        DateTime endDate =
            DateTime.Now.AddDays(1);

        DateTime startDate =
            DateTime.Now.AddMonths(-1);

        GetHistory(
            startDate,
            endDate,
            "1h"
        );
    }


    // =====================================
    // =====================================

    public void ShowThreeMonths()
    {
        currentPeriod = ChartPeriod.ThreeMonths;

        DateTime endDate =
            DateTime.Now.AddDays(1);

        DateTime startDate =
            DateTime.Now.AddMonths(-3);

        GetHistory(
            startDate,
            endDate,
            "1d"
        );
    }


    // =====================================
    // =====================================

    public void ShowOneYear()
    {
        currentPeriod = ChartPeriod.OneYear;

        DateTime endDate =
            DateTime.Now.AddDays(1);

        DateTime startDate =
            DateTime.Now.AddYears(-1);

        GetHistory(
            startDate,
            endDate,
            "1d"
        );
    }


    // =====================================
    // =====================================

    private void RefreshCurrentChart()
    {
        switch (currentPeriod)
        {
            case ChartPeriod.OneDay:
                ShowOneDay();
                break;

            case ChartPeriod.OneWeek:
                ShowOneWeek();
                break;

            case ChartPeriod.OneMonth:
                ShowOneMonth();
                break;

            case ChartPeriod.ThreeMonths:
                ShowThreeMonths();
                break;

            case ChartPeriod.OneYear:
                ShowOneYear();
                break;
        }
    }


    // =====================================
    // =====================================

    private void GetHistory(
        DateTime startDate,
        DateTime endDate,
        string interval
    )
    {
        string start =
            startDate.ToString("yyyy-MM-dd");

        string end =
            endDate.ToString("yyyy-MM-dd");

        int requestId = ++latestHistoryRequestId;
        StartCoroutine(
            GetStockHistoryCoroutine(
                currentTicker,
                start,
                end,
                interval,
                requestId
            )
        );
    }


    // =====================================
    // =====================================

    IEnumerator GetStockHistoryCoroutine(
        string ticker,
        string start,
        string end,
        string interval,
        int requestId
    )
    {
        string apiUrl =
            historyBaseUrl +
            ApiTicker(ticker) +
            "?start=" + start +
            "&end=" + end +
            "&interval=" + interval;


        Debug.Log(
            "�����擾�F" +
            ticker +
            " / " +
            start +
            " �` " +
            end +
            " / " +
            interval
        );


        using (UnityWebRequest request =
               UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (requestId != latestHistoryRequestId || ticker != currentTicker)
                yield break;

            if (request.result ==
                UnityWebRequest.Result.Success)
            {
                HistoryData history =
                    JsonUtility.FromJson<HistoryData>(
                        request.downloadHandler.text
                    );


                if (history != null &&
                    history.bars != null &&
                    history.bars.Length > 0)
                {
                    Debug.Log(
                        "�����擾�����I �f�[�^���F" +
                        history.bars.Length +
                        " / interval�F" +
                        interval
                    );


                    if (stockChartRenderer != null)
                    {
                        stockChartRenderer.DrawChart(
                            history.bars
                        );
                    }
                }
                else
                {
                    Debug.LogWarning(
                        "�����f�[�^������܂���"
                    );
                }
            }
            else
            {
                Debug.LogError(
                    "�����擾���s�F" +
                    request.error
                );
            }
        }
    }
}


// =====================================
// =====================================

[System.Serializable]
public class StockData
{
    public string ticker;
    public string name;

    public float price;
    public float previous_close;

    public float change;
    public float change_percent;

    public string market_time;
}


// =====================================
// =====================================

[System.Serializable]
public class HistoryData
{
    public string ticker;
    public string interval;

    public HistoryItem[] bars;
}


// =====================================
// =====================================

[System.Serializable]
public class HistoryItem
{
    public string ts;

    public string date;

    public float open;
    public float high;
    public float low;
    public float close;

    public long volume;
}

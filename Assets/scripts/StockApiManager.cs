using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class StockApiManager : MonoBehaviour
{
    [Header("現在株価")]
    public TMP_Text stockPriceText;
    public TMP_Text stockChangeText;

    [Header("注文")]
    public TradeController tradeController;

    [Header("チャート")]
    public StockChartRenderer stockChartRenderer;

    private string quoteBaseUrl =
        "http://localhost:8000/api/v1/quotes/";

    private string historyBaseUrl =
        "http://localhost:8000/api/v1/history/";

    // 現在選択している銘柄
    private string currentTicker = "7203";

    // 現在選択している期間
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
    // 最初の表示
    // =====================================

    void Start()
    {
        // 最初はトヨタ
        GetStockPrice(currentTicker);

        // 最初は1か月表示
        ShowOneMonth();
    }


    // =====================================
    // 銘柄を変更
    // =====================================

    public void GetStockPrice(string ticker)
    {
        currentTicker = ticker;

        StartCoroutine(
            GetStockPriceCoroutine(ticker)
        );

        // 銘柄を変えても現在選択中の期間を維持
        RefreshCurrentChart();
    }


    // =====================================
    // 現在株価API
    // =====================================

    IEnumerator GetStockPriceCoroutine(string ticker)
    {
        string apiUrl =
            quoteBaseUrl + ticker;

        using (UnityWebRequest request =
               UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result ==
                UnityWebRequest.Result.Success)
            {
                StockData data =
                    JsonUtility.FromJson<StockData>(
                        request.downloadHandler.text
                    );

                if (stockPriceText != null)
                {
                    stockPriceText.text =
                        data.price.ToString("N0");
                }

                if (stockChangeText != null)
                {
                    stockChangeText.text =
                        data.change_percent.ToString(
                            "+0.00;-0.00;0.00"
                        ) + "%";
                }

                if (tradeController != null)
                {
                    tradeController.SetStockPrice(
                        Mathf.RoundToInt(data.price)
                    );
                }

                Debug.Log(
                    "現在株価取得成功：" +
                    data.ticker +
                    " / " +
                    data.price
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
    // 1日
    // 5分足
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
    // 1週間
    // 30分足
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
    // 1か月
    // 1時間足
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
    // 3か月
    // 1日足
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
    // 1年
    // 1日足
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
    // 現在の期間でグラフを再取得
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
    // 履歴取得開始
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

        StartCoroutine(
            GetStockHistoryCoroutine(
                currentTicker,
                start,
                end,
                interval
            )
        );
    }


    // =====================================
    // 過去株価API
    // =====================================

    IEnumerator GetStockHistoryCoroutine(
        string ticker,
        string start,
        string end,
        string interval
    )
    {
        string apiUrl =
            historyBaseUrl +
            ticker +
            "?start=" + start +
            "&end=" + end +
            "&interval=" + interval;


        Debug.Log(
            "履歴取得：" +
            ticker +
            " / " +
            start +
            " ～ " +
            end +
            " / " +
            interval
        );


        using (UnityWebRequest request =
               UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

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
                        "履歴取得成功！ データ数：" +
                        history.bars.Length +
                        " / interval：" +
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
                        "履歴データがありません"
                    );
                }
            }
            else
            {
                Debug.LogError(
                    "履歴取得失敗：" +
                    request.error
                );
            }
        }
    }
}


// =====================================
// 現在株価
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
}


// =====================================
// 過去株価
// =====================================

[System.Serializable]
public class HistoryData
{
    public string ticker;
    public string interval;

    public HistoryItem[] bars;
}


// =====================================
// 1本分の株価
// =====================================

[System.Serializable]
public class HistoryItem
{
    // 時間足で使う
    public string ts;

    public string date;

    public float open;
    public float high;
    public float low;
    public float close;

    public long volume;
}
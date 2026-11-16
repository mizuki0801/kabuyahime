using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TradeChartController : MonoBehaviour
{
    [Header("グラフ描画")]
    [SerializeField] private StockChartRenderer chartRenderer;
    [Header("取引マーカー")]
    [SerializeField] private TradeMarkerRenderer tradeMarkerRenderer;
    [Header("旧・休場日マーカー（使用しません）")]
    [SerializeField] private TradeMarkerRenderer offMarketMarkerRenderer;
    [Header("株価履歴API")]
    [SerializeField] private string historyBaseUrl = "http://localhost:8000/api/v1/history/";

    private class TradeMarkerData
    {
        public string type;
        public string dateTime;
        public int quantity;
        public float price;
    }

    private readonly List<TradeMarkerData> tradeMarkers = new List<TradeMarkerData>();
    private Coroutine loadingCoroutine;

    public void AddTradeMarker(string type, string dateTime, int quantity, float price)
    {
        tradeMarkers.Add(new TradeMarkerData
        {
            type = type,
            dateTime = dateTime,
            quantity = quantity,
            price = price
        });
    }

    public void ClearTradeMarkers()
    {
        tradeMarkers.Clear();
        if (tradeMarkerRenderer != null) tradeMarkerRenderer.ClearMarkers();
        if (offMarketMarkerRenderer != null) offMarketMarkerRenderer.ClearMarkers();
        if (chartRenderer != null)
            chartRenderer.SetTradeChartPoints(new List<StockChartRenderer.TradeChartPoint>());
    }

    public void ShowChart(string ticker, string start, string end, string interval = "1d")
    {
        if (loadingCoroutine != null) StopCoroutine(loadingCoroutine);
        loadingCoroutine = StartCoroutine(LoadHistory(ticker, start, end, interval));
    }

    private IEnumerator LoadHistory(string ticker, string start, string end, string interval)
    {
        string apiUrl = historyBaseUrl + UnityWebRequest.EscapeURL(ticker)
            + "?start=" + UnityWebRequest.EscapeURL(start)
            + "&end=" + UnityWebRequest.EscapeURL(end)
            + "&interval=" + UnityWebRequest.EscapeURL(interval);
        Debug.Log("【振り返り画面・株価API】" + apiUrl);

        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("【株価履歴取得失敗】" + request.error);
                loadingCoroutine = null;
                yield break;
            }
            HistoryData history = JsonUtility.FromJson<HistoryData>(request.downloadHandler.text);
            if (history == null || history.bars == null || history.bars.Length < 2)
            {
                Debug.LogWarning("【株価履歴】データ不足");
                loadingCoroutine = null;
                yield break;
            }
            if (chartRenderer == null)
            {
                Debug.LogError("【株価グラフ】Renderer未設定");
                loadingCoroutine = null;
                yield break;
            }
            SendTradePointsToChart();
            chartRenderer.DrawChart(history.bars);
            DrawTradeMarkers();
            loadingCoroutine = null;
        }
    }

    private void SendTradePointsToChart()
    {
        List<StockChartRenderer.TradeChartPoint> points = new List<StockChartRenderer.TradeChartPoint>();
        foreach (TradeMarkerData trade in tradeMarkers)
        {
            if (!DateTime.TryParse(trade.dateTime, out DateTime time))
            {
                Debug.LogWarning("【取引日時の変換失敗】" + trade.dateTime);
                continue;
            }
            points.Add(new StockChartRenderer.TradeChartPoint
            {
                dateTime = time,
                price = trade.price,
                isTrade = true,
                tradeType = trade.type
            });
        }
        chartRenderer.SetTradeChartPoints(points);
        Debug.Log("【取引データ送信完了】" + points.Count + "件");
    }

    private void DrawTradeMarkers()
    {
        if (tradeMarkerRenderer != null) tradeMarkerRenderer.ClearMarkers();
        if (offMarketMarkerRenderer != null) offMarketMarkerRenderer.ClearMarkers();
        if (chartRenderer == null || !chartRenderer.HasChartData) return;

        List<TradeMarkerData> sorted = new List<TradeMarkerData>(tradeMarkers);
        sorted.Sort((a, b) => string.CompareOrdinal(a.dateTime, b.dateTime));
        int drawn = 0;
        foreach (TradeMarkerData trade in sorted)
        {
            if (!chartRenderer.TryGetTradePosition(trade.dateTime, trade.price, out Vector2 pos))
            {
                Debug.LogWarning("【取引マーカー座標なし】" + trade.dateTime);
                continue;
            }
            if (tradeMarkerRenderer != null)
            {
                tradeMarkerRenderer.AddMarker(trade.type, trade.dateTime,
                    trade.quantity, trade.price, pos.x, pos.y);
                drawn++;
            }
        }
        // 休場日も統合グラフに描画するため、専用エリアには配置しない。
        Debug.Log("【取引マーカー描画結果】グラフ上：" + drawn
            + "件 / 未配置：" + (sorted.Count - drawn) + "件");
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StockChartRenderer : MonoBehaviour
{
    [Header("グラフ")]
    public float lineThickness = 4f;
    public Color lineColor = new Color(0.45f, 0.30f, 0.85f, 1f);

    [Header("ツールチップ")]
    public GameObject chartTooltip;
    public TMP_Text tooltipText;

    [Header("日付表示")]
    [SerializeField] private RectTransform dateLabelLayer;
    [SerializeField] private TMP_FontAsset dateLabelFont;
    [SerializeField] private float dateFontSize = 20f;
    [SerializeField] private int maxDateLabels = 4;

    [Header("取引マーカーと線")]
    [SerializeField] private RectTransform tradeMarkerLayer;
    [SerializeField] private Color tradeLineColor = new Color(0.45f, 0.30f, 0.85f, 1f);
    [SerializeField] private Color bridgeColor = new Color(0.45f, 0.30f, 0.85f, 0.65f);

    [Serializable]
    public class TradeChartPoint
    {
        public DateTime dateTime;
        public float price;
        public bool isTrade;
        public string tradeType;
    }

    private readonly List<TradeChartPoint> tradeChartPoints = new List<TradeChartPoint>();
    private readonly List<GameObject> chartObjects = new List<GameObject>();
    private readonly List<GameObject> dateObjects = new List<GameObject>();
    private HistoryItem[] currentBars;
    private float minPrice;
    private float maxPrice;

    // Market history occupies the first part of the same plot; transaction times are
    // magnified in the remaining part to keep minute-apart markers distinguishable.
    private const float MarketEndX = 0.62f;
    private const float TradeStartX = 0.70f;
    private const float TradeEndX = 0.94f;

    public RectTransform ChartRect => GetComponent<RectTransform>();
    public bool HasChartData => currentBars != null && currentBars.Length >= 2;

    public void SetTradeChartPoints(IEnumerable<TradeChartPoint> points)
    {
        tradeChartPoints.Clear();
        if (points != null)
        {
            foreach (TradeChartPoint point in points)
            {
                if (point != null && !float.IsNaN(point.price) && !float.IsInfinity(point.price))
                    tradeChartPoints.Add(point);
            }
            tradeChartPoints.Sort((a, b) => a.dateTime.CompareTo(b.dateTime));
        }
        Debug.Log("【グラフ取引データ】" + tradeChartPoints.Count + "件を登録");
    }

    public void DrawChart(HistoryItem[] bars)
    {
        ClearChart();
        if (bars == null || bars.Length < 2)
        {
            Debug.LogWarning("【株価グラフ】株価履歴が不足しています");
            return;
        }
        // APIの返却順に依存せず、日時順に並べる。
        // デモ取引より後の市場データを取引より左に描かない。
        List<HistoryItem> ordered = new List<HistoryItem>();
        foreach (HistoryItem bar in bars)
        {
            if (bar == null) continue;
            if (!DateTime.TryParse(bar.date, out DateTime barTime)) continue;
            if (tradeChartPoints.Count > 0 &&
                barTime >= tradeChartPoints[0].dateTime) continue;
            ordered.Add(bar);
        }
        ordered.Sort((a, b) => DateTime.Parse(a.date).CompareTo(DateTime.Parse(b.date)));
        if (ordered.Count < 2)
        {
            Debug.LogWarning("【株価グラフ】取引前の市場データが2件未満です");
            return;
        }
        bars = ordered.ToArray();
        currentBars = bars;
        float width = ChartRect.rect.width;
        float height = ChartRect.rect.height;
        if (width <= 0f || height <= 0f)
        {
            Debug.LogWarning("【株価グラフ】CharAreaの幅または高さが0です");
            return;
        }

        minPrice = bars[0].close;
        maxPrice = bars[0].close;
        foreach (HistoryItem bar in bars)
        {
            minPrice = Mathf.Min(minPrice, bar.close);
            maxPrice = Mathf.Max(maxPrice, bar.close);
        }
        foreach (TradeChartPoint trade in tradeChartPoints)
        {
            minPrice = Mathf.Min(minPrice, trade.price);
            maxPrice = Mathf.Max(maxPrice, trade.price);
        }
        float range = maxPrice - minPrice;
        if (range < 0.01f) range = 2f;
        float padding = range * 0.12f;
        minPrice -= padding;
        maxPrice += padding;

        bool hasTrades = tradeChartPoints.Count > 0;
        float marketRight = hasTrades ? MarketEndX : 1f;
        Vector2[] marketPoints = new Vector2[bars.Length];
        for (int i = 0; i < bars.Length; i++)
        {
            float nx = (float)i / (bars.Length - 1) * marketRight;
            float ny = Mathf.InverseLerp(minPrice, maxPrice, bars[i].close);
            marketPoints[i] = new Vector2(nx * width, ny * height);
        }
        for (int i = 0; i < marketPoints.Length - 1; i++)
            CreateLine(transform, marketPoints[i], marketPoints[i + 1], lineColor, "ChartLine");
        for (int i = 0; i < marketPoints.Length; i++)
            CreateHoverPoint(marketPoints[i], bars[i]);

        if (hasTrades)
        {
            // The dashed segment indicates a gap in market data. It is NOT a
            // claimed market-price movement during the closed session.
            Vector2 firstTrade = TradePosition(0, width, height);
            CreateDashedLine(transform, marketPoints[marketPoints.Length - 1], firstTrade, bridgeColor);

            // Draw the actual recorded demo execution prices in the SAME plot.
            // Using the ChartRect as parent keeps coordinates identical to the
            // existing market lines and to normalized trade-marker coordinates.
            for (int i = 0; i < tradeChartPoints.Count - 1; i++)
            {
                Vector2 a = TradePosition(i, width, height);
                Vector2 b = TradePosition(i + 1, width, height);
                CreateLine(transform, a, b, tradeLineColor, "DemoTradeLine");
            }
        }

        DrawDateLabels(bars);
        // MarkerLayer is a full-stretch child of ChartRect. Bring it above all lines.
        RectTransform layer = tradeMarkerLayer != null
            ? tradeMarkerLayer : transform.Find("TradeMarkerLayer") as RectTransform;
        if (layer != null) layer.SetAsLastSibling();
        HideTooltip();
        Debug.Log("【統合グラフ描画完了】市場：" + bars.Length + "件 / デモ取引：" + tradeChartPoints.Count + "件");
    }

    private Vector2 TradePosition(int index, float width, float height)
    {
        float t = 0.5f;
        if (tradeChartPoints.Count > 1)
        {
            DateTime first = tradeChartPoints[0].dateTime;
            DateTime last = tradeChartPoints[tradeChartPoints.Count - 1].dateTime;
            double duration = (last - first).TotalSeconds;
            t = duration > 0.0
                ? (float)((tradeChartPoints[index].dateTime - first).TotalSeconds / duration)
                : (float)index / (tradeChartPoints.Count - 1);
        }
        float x = Mathf.Lerp(TradeStartX, TradeEndX, Mathf.Clamp01(t));
        float y = Mathf.InverseLerp(minPrice, maxPrice, tradeChartPoints[index].price);
        return new Vector2(x * width, y * height);
    }

    public bool TryGetTradePosition(string tradeDateTime, float tradePrice, out Vector2 normalizedPosition)
    {
        normalizedPosition = Vector2.zero;
        if (!HasChartData || tradeChartPoints.Count == 0) return false;
        if (!DateTime.TryParse(tradeDateTime, out DateTime requested)) return false;
        float width = ChartRect.rect.width;
        float height = ChartRect.rect.height;
        if (width <= 0f || height <= 0f) return false;
        for (int i = 0; i < tradeChartPoints.Count; i++)
        {
            TradeChartPoint trade = tradeChartPoints[i];
            if (trade.dateTime == requested && Mathf.Abs(trade.price - tradePrice) < 0.01f)
            {
                Vector2 point = TradePosition(i, width, height);
                normalizedPosition = new Vector2(point.x / width, point.y / height);
                return true;
            }
        }
        return false;
    }

    private void DrawDateLabels(HistoryItem[] bars)
    {
        if (dateLabelLayer == null)
        {
            Debug.LogWarning("【日付表示】Date Label Layerが未設定です");
            return;
        }
        int count = Mathf.Min(Mathf.Max(2, maxDateLabels), bars.Length);
        // 日時ラベルの重なりを抑えるため、取引ありの場合は市場側を最大3件に。
        if (tradeChartPoints.Count > 0) count = Mathf.Min(count, 3);
        HashSet<int> indices = new HashSet<int>();
        for (int i = 0; i < count; i++)
            indices.Add(Mathf.RoundToInt((float)i / (count - 1) * (bars.Length - 1)));
        List<int> sorted = new List<int>(indices);
        sorted.Sort();
        float right = tradeChartPoints.Count > 0 ? MarketEndX : 1f;
        foreach (int i in sorted)
        {
            string value = DateTime.TryParse(bars[i].date, out DateTime parsed)
                ? (parsed.TimeOfDay == TimeSpan.Zero ? parsed.ToString("MM/dd") : parsed.ToString("MM/dd HH:mm"))
                : bars[i].date;
            AddDateLabel("MarketDate_" + i, value, (float)i / (bars.Length - 1) * right, 60f);
        }
        if (tradeChartPoints.Count > 0)
        {
            // The date/time region is magnified. Full timestamp remains in marker hover.
            AddDateLabel("TradeStartDate", tradeChartPoints[0].dateTime.ToString("MM/dd HH:mm"), TradeStartX, 85f);
            if (tradeChartPoints.Count > 1)
                AddDateLabel("TradeEndDate", tradeChartPoints[tradeChartPoints.Count - 1].dateTime.ToString("HH:mm"), TradeEndX, 55f);
        }
        Debug.Log("【日付ラベル描画成功】" + dateObjects.Count + "件");
    }

    private void AddDateLabel(string name, string value, float normalizedX, float labelWidth)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(dateLabelLayer, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(normalizedX, 1f);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(labelWidth, 22f);
        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        if (dateLabelFont != null) label.font = dateLabelFont;
        label.fontSize = Mathf.Min(dateFontSize, 20f);
        label.color = Color.black;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
        label.overflowMode = TextOverflowModes.Overflow;
        label.text = value;
        dateObjects.Add(obj);
    }

    private void CreateDashedLine(Transform parent, Vector2 a, Vector2 b, Color color)
    {
        Vector2 delta = b - a;
        float length = delta.magnitude;
        if (length < 0.01f) return;
        const float dash = 9f;
        const float gap = 6f;
        for (float offset = 0f; offset < length; offset += dash + gap)
        {
            float end = Mathf.Min(offset + dash, length);
            Vector2 startPoint = a + delta * (offset / length);
            Vector2 endPoint = a + delta * (end / length);
            CreateLine(parent, startPoint, endPoint, color, "MarketClosedDash");
        }
    }

    private void CreateLine(Transform parent, Vector2 pointA, Vector2 pointB, Color color, string objectName)
    {
        Vector2 direction = pointB - pointA;
        if (direction.sqrMagnitude < 0.0001f) return;
        GameObject obj = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = pointA;
        rect.sizeDelta = new Vector2(direction.magnitude, lineThickness);
        rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        chartObjects.Add(obj);
    }

    private void CreateHoverPoint(Vector2 position, HistoryItem item)
    {
        GameObject obj = new GameObject("HoverPoint", typeof(RectTransform), typeof(Image), typeof(ChartHoverPoint));
        obj.transform.SetParent(transform, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(30f, 50f);
        Image image = obj.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;
        obj.GetComponent<ChartHoverPoint>().Setup(this, item.date, item.close);
        chartObjects.Add(obj);
    }

    public void ShowTooltip(string date, float closePrice, Vector3 worldPosition)
    {
        if (chartTooltip == null || tooltipText == null) return;
        chartTooltip.SetActive(true);
        string displayDate = date;
        if (DateTime.TryParse(date, out DateTime parsed))
            displayDate = parsed.ToString("MM/dd HH:mm");
        tooltipText.text = displayDate + "\n終値 " + closePrice.ToString("N0") + "円";
        chartTooltip.transform.position = worldPosition + new Vector3(0f, -18f, 0f);
    }

    public void HideTooltip()
    {
        if (chartTooltip != null) chartTooltip.SetActive(false);
    }

    public void ClearChart()
    {
        foreach (GameObject obj in chartObjects)
            if (obj != null) Destroy(obj);
        chartObjects.Clear();
        foreach (GameObject obj in dateObjects)
            if (obj != null) Destroy(obj);
        dateObjects.Clear();
        currentBars = null;
        // Do not clear tradeChartPoints: they are supplied before DrawChart().
    }
}

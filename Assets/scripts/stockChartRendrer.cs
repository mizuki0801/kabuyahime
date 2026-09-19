using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StockChartRenderer : MonoBehaviour
{
    [Header("グラフ")]
    public float lineThickness = 4f;
    public Color lineColor = new Color(0.45f, 0.30f, 0.85f, 1f);

    [Header("ツールチップ")]
    public GameObject chartTooltip;
    public TMP_Text tooltipText;

    private List<GameObject> chartObjects = new List<GameObject>();


    // =====================================
    // グラフを描画
    // =====================================

    public void DrawChart(HistoryItem[] bars)
    {
        ClearChart();

        if (bars == null || bars.Length < 2)
        {
            Debug.LogWarning("グラフ用のデータが足りません");
            return;
        }

        RectTransform chartRect = GetComponent<RectTransform>();

        float width = chartRect.rect.width;
        float height = chartRect.rect.height;

        float minPrice = bars[0].close;
        float maxPrice = bars[0].close;

        foreach (HistoryItem item in bars)
        {
            if (item.close < minPrice)
                minPrice = item.close;

            if (item.close > maxPrice)
                maxPrice = item.close;
        }

        if (Mathf.Approximately(minPrice, maxPrice))
        {
            maxPrice += 1f;
            minPrice -= 1f;
        }


        // =====================================
        // 各日の位置を計算
        // =====================================

        Vector2[] points = new Vector2[bars.Length];

        for (int i = 0; i < bars.Length; i++)
        {
            float x =
                ((float)i / (bars.Length - 1)) * width;

            float y =
                Mathf.InverseLerp(
                    minPrice,
                    maxPrice,
                    bars[i].close
                ) * height;

            points[i] = new Vector2(x, y);
        }


        // =====================================
        // 折れ線を描く
        // =====================================

        for (int i = 0; i < points.Length - 1; i++)
        {
            CreateLine(
                points[i],
                points[i + 1]
            );
        }


        // =====================================
        // 各日の透明な当たり判定を作る
        // =====================================

        for (int i = 0; i < points.Length; i++)
        {
            CreateHoverPoint(
                points[i],
                bars[i]
            );
        }


        // 最初は吹き出しを非表示
        HideTooltip();

        Debug.Log(
            "グラフ描画成功！ 点数：" +
            bars.Length
        );
    }


    // =====================================
    // 折れ線を作る
    // =====================================

    private void CreateLine(
        Vector2 pointA,
        Vector2 pointB
    )
    {
        GameObject lineObject =
            new GameObject(
                "ChartLine",
                typeof(Image)
            );

        lineObject.transform.SetParent(
            transform,
            false
        );

        Image image =
            lineObject.GetComponent<Image>();

        image.color = lineColor;
        image.raycastTarget = false;


        RectTransform rect =
            lineObject.GetComponent<RectTransform>();

        Vector2 direction =
            pointB - pointA;

        float distance =
            direction.magnitude;


        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0.5f);

        rect.anchoredPosition = pointA;

        rect.sizeDelta =
            new Vector2(
                distance,
                lineThickness
            );


        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        rect.localEulerAngles =
            new Vector3(
                0,
                0,
                angle
            );


        chartObjects.Add(lineObject);
    }


    // =====================================
    // 透明なホバー判定を作る
    // =====================================

    private void CreateHoverPoint(
        Vector2 position,
        HistoryItem item
    )
    {
        GameObject hoverObject =
            new GameObject(
                "HoverPoint",
                typeof(Image),
                typeof(ChartHoverPoint)
            );

        hoverObject.transform.SetParent(
            transform,
            false
        );


        Image image =
            hoverObject.GetComponent<Image>();

        // 見えないがRaycastは受け取れる
        image.color =
            new Color(1f, 1f, 1f, 0f);

        image.raycastTarget = true;


        RectTransform rect =
            hoverObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.anchoredPosition = position;

        // カーソルを合わせやすい大きさ
        rect.sizeDelta =
            new Vector2(30f, 50f);


        ChartHoverPoint hover =
            hoverObject.GetComponent<ChartHoverPoint>();

        hover.Setup(
            this,
            item.date,
            item.close
        );


        chartObjects.Add(hoverObject);
    }


    // =====================================
    // 吹き出しを表示
    // =====================================

    public void ShowTooltip(
        string date,
        float closePrice,
        Vector3 worldPosition
    )
    {
        if (chartTooltip == null ||
            tooltipText == null)
        {
            return;
        }


        chartTooltip.SetActive(true);


        // 2026-09-18 → 9月18日
        string displayDate = date;

        System.DateTime parsedDate;

        if (System.DateTime.TryParse(
            date,
            out parsedDate
        ))
        {
            displayDate =
                parsedDate.Month +
                "月" +
                parsedDate.Day +
                "日";
        }


        tooltipText.text =
            displayDate +
            "\n終値 " +
            closePrice.ToString("N0") +
            "円";


        // データ点の少し上に表示
        chartTooltip.transform.position =
            worldPosition +
            new Vector3(0f, -18f, 0f);
    }


    // =====================================
    // 吹き出しを非表示
    // =====================================

    public void HideTooltip()
    {
        if (chartTooltip != null)
        {
            chartTooltip.SetActive(false);
        }
    }


    // =====================================
    // 古いグラフを削除
    // =====================================

    public void ClearChart()
    {
        foreach (GameObject obj in chartObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        chartObjects.Clear();
    }
}
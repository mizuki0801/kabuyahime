using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniStockChartRenderer : MonoBehaviour
{
    [Header("ê¸ÇÃê›íË")]
    public float lineWidth = 4f;

    private readonly List<GameObject> lines = new List<GameObject>();

    public void DrawChart(float[] prices)
    {
        ClearChart();

        if (prices == null || prices.Length < 2)
            return;

        RectTransform chartRect = GetComponent<RectTransform>();

        float width = chartRect.rect.width;
        float height = chartRect.rect.height;

        float minPrice = prices[0];
        float maxPrice = prices[0];

        foreach (float price in prices)
        {
            if (price < minPrice) minPrice = price;
            if (price > maxPrice) maxPrice = price;
        }

        float priceRange = maxPrice - minPrice;

        if (priceRange <= 0)
            priceRange = 1f;

        Vector2 previousPoint = Vector2.zero;

        for (int i = 0; i < prices.Length; i++)
        {
            float x =
                ((float)i / (prices.Length - 1)) * width
                - width / 2f;

            float normalizedPrice =
                (prices[i] - minPrice) / priceRange;

            float y =
                normalizedPrice * height
                - height / 2f;

            // è„â∫Ç…è≠Çµó]îíÇçÏÇÈ
            y *= 0.75f;

            Vector2 currentPoint = new Vector2(x, y);

            if (i > 0)
            {
                CreateLine(previousPoint, currentPoint);
            }

            previousPoint = currentPoint;
        }
    }

    private void CreateLine(Vector2 start, Vector2 end)
    {
        GameObject lineObject = new GameObject(
            "MiniChartLine",
            typeof(RectTransform),
            typeof(Image)
        );

        lineObject.transform.SetParent(transform, false);

        RectTransform rect =
            lineObject.GetComponent<RectTransform>();

        Image image =
            lineObject.GetComponent<Image>();

        // Ç∆ÇËÇ†Ç¶Ç∏óŒ
        image.color = new Color32(70, 190, 80, 255);
        image.raycastTarget = false;

        Vector2 direction = end - start;
        float distance = direction.magnitude;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);

        rect.anchoredPosition = start;
        rect.sizeDelta =
            new Vector2(distance, lineWidth);

        float angle =
            Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg;

        rect.localRotation =
            Quaternion.Euler(0f, 0f, angle);

        lines.Add(lineObject);
    }

    private void ClearChart()
    {
        foreach (GameObject line in lines)
        {
            if (line != null)
                Destroy(line);
        }

        lines.Clear();
    }
}

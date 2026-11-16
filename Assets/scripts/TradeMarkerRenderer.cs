
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TradeMarkerRenderer : MonoBehaviour
{
    [Header("マーカー表示先")]
    [SerializeField] private RectTransform markerArea;

    [Header("マーカー設定")]
    [SerializeField] private float markerSize = 25f;
    [SerializeField] private Color buyColor = Color.green;
    [SerializeField] private Color sellColor = Color.red;

    [Header("取引ツールチップ")]
    [SerializeField] private GameObject tradeTooltip;
    [SerializeField] private TMP_Text tooltipText;

    private readonly List<GameObject> markers =
        new List<GameObject>();

    // 購入・売却マーカーを追加
    public void AddMarker(
        string type,
        string dateTime,
        int quantity,
        float price,
        float normalizedX,
        float normalizedY
    )
    {
        if (markerArea == null)
        {
            Debug.LogError(
                "【取引マーカー】Marker Areaが未設定です"
            );
            return;
        }

        // 正規化座標を0〜1に制限
        float x = Mathf.Clamp01(normalizedX);
        float y = Mathf.Clamp01(normalizedY);

        GameObject marker = new GameObject(
            type == "buy" ? "BuyMarker" : "SellMarker",
            typeof(RectTransform),
            typeof(Image),
            typeof(TradeMarkerHover)
        );

        marker.transform.SetParent(markerArea, false);

        RectTransform rect =
            marker.GetComponent<RectTransform>();

        // 正規化座標をアンカーとして使用
        // 親のサイズ変更にも追従する
        rect.anchorMin = new Vector2(x, y);
        rect.anchorMax = new Vector2(x, y);

        // マーカーの中心を取引位置に合わせる
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        // マーカーサイズ
        rect.sizeDelta = new Vector2(
            markerSize,
            markerSize
        );

        // 購入・売却で色分け
        Image image = marker.GetComponent<Image>();

        image.color =
            type == "buy" ? buyColor : sellColor;

        image.raycastTarget = true;

        // ホバー時の取引情報を設定
        TradeMarkerHover hover =
            marker.GetComponent<TradeMarkerHover>();

        hover.Setup(
            this,
            type,
            dateTime,
            quantity,
            price
        );

        markers.Add(marker);

        Debug.Log(
            "【取引マーカー追加】" +
            type +
            " / " + dateTime +
            " / " + quantity + "株" +
            " / " + price + "円" +
            " / X:" + x.ToString("F3") +
            " / Y:" + y.ToString("F3")
        );
    }

    // 取引情報を表示
    public void ShowTradeTooltip(
        string type,
        string dateTime,
        int quantity,
        float price,
        Vector3 worldPosition
    )
    {
        if (tradeTooltip == null || tooltipText == null)
        {
            Debug.LogWarning(
                "【取引ツールチップ】UI参照が未設定です"
            );
            return;
        }

        string tradeName =
            type == "buy" ? "購入" : "売却";

        tooltipText.text =
            tradeName + "\n" +
            dateTime + "\n" +
            "数量：" + quantity + "株\n" +
            "価格：" + price.ToString("N0") + "円";

        tradeTooltip.SetActive(true);

        tradeTooltip.transform.position =
            worldPosition + new Vector3(0f, 60f, 0f);
    }

    // 取引情報を非表示
    public void HideTradeTooltip()
    {
        if (tradeTooltip != null)
        {
            tradeTooltip.SetActive(false);
        }
    }

    // すべてのマーカーを削除
    public void ClearMarkers()
    {
        HideTradeTooltip();

        foreach (GameObject marker in markers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }

        markers.Clear();
    }
}

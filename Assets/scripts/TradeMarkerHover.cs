
using UnityEngine;
using UnityEngine.EventSystems;

public class TradeMarkerHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private TradeMarkerRenderer markerRenderer;

    private string tradeType;
    private string tradeDateTime;
    private int tradeQuantity;
    private float tradePrice;

    // マーカーの取引情報を登録
    public void Setup(
        TradeMarkerRenderer renderer,
        string type,
        string dateTime,
        int quantity,
        float price
    )
    {
        markerRenderer = renderer;
        tradeType = type;
        tradeDateTime = dateTime;
        tradeQuantity = quantity;
        tradePrice = price;
    }

    // マウスがマーカーに乗ったとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (markerRenderer == null)
            return;

        markerRenderer.ShowTradeTooltip(
            tradeType,
            tradeDateTime,
            tradeQuantity,
            tradePrice,
            transform.position
        );
    }

    // マウスがマーカーから離れたとき
    public void OnPointerExit(PointerEventData eventData)
    {
        if (markerRenderer == null)
            return;

        markerRenderer.HideTradeTooltip();
    }

    private void OnDisable()
    {
        if (markerRenderer != null)
        {
            markerRenderer.HideTradeTooltip();
        }
    }
}

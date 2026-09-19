using UnityEngine;
using UnityEngine.EventSystems;

public class ChartHoverPoint : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private StockChartRenderer chartRenderer;
    private string date;
    private float closePrice;

    public void Setup(
        StockChartRenderer renderer,
        string stockDate,
        float stockClosePrice
    )
    {
        chartRenderer = renderer;
        date = stockDate;
        closePrice = stockClosePrice;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (chartRenderer != null)
        {
            chartRenderer.ShowTooltip(
                date,
                closePrice,
                transform.position
            );
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (chartRenderer != null)
        {
            chartRenderer.HideTooltip();
        }
    }
}
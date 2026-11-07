
using UnityEngine;
using UnityEngine.UI;

public class SuperChatPageController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform giftGrid;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    [Header("Settings")]
    [SerializeField, Min(1)] private int itemsPerPage = 5;

    private int currentPage = 0;
    private int totalPages;

    private void Start()
    {
        if (giftGrid == null || prevButton == null || nextButton == null)
        {
            Debug.LogError("SuperChatPageController: UIの参照が未設定です。");
            enabled = false;
            return;
        }

        totalPages = Mathf.CeilToInt(
            (float)giftGrid.childCount / itemsPerPage
        );

        prevButton.onClick.AddListener(PreviousPage);
        nextButton.onClick.AddListener(NextPage);

        ShowPage();
    }

    private void PreviousPage()
    {
        if (currentPage <= 0) return;

        currentPage--;
        ShowPage();
    }

    private void NextPage()
    {
        if (currentPage >= totalPages - 1) return;

        currentPage++;
        ShowPage();
    }

    private void ShowPage()
    {
        int startIndex = currentPage * itemsPerPage;
        int endIndex = startIndex + itemsPerPage;

        for (int i = 0; i < giftGrid.childCount; i++)
        {
            bool visible = i >= startIndex && i < endIndex;
            giftGrid.GetChild(i).gameObject.SetActive(visible);
        }

        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < totalPages - 1;

        LayoutRebuilder.ForceRebuildLayoutImmediate(giftGrid);
    }

    private void OnDestroy()
    {
        if (prevButton != null)
            prevButton.onClick.RemoveListener(PreviousPage);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextPage);
    }
}

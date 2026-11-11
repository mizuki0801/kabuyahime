
using System;
using System.IO;
using UnityEngine;

public class TradeHistoryListUI : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private Transform content;
    [SerializeField] private TradeHistoryItemUI itemPrefab;

    [Header("JSON設定")]
    [SerializeField] private string fileName = "trade_history.json";

    [Serializable]
    private class TradeData
    {
        public string id;
        public string ticker;
        public string stockName;
        public string type;
        public string dateTime;
        public int quantity;
        public int price;
        public float averagePurchasePrice;
        public double realizedProfit;
    }

    [Serializable]
    private class TradeHistoryData
    {
        public TradeData[] trades;
    }

    private void OnEnable()
    {
        RefreshHistory();
    }

    public void RefreshHistory()
    {
        if (content == null || itemPrefab == null)
        {
            Debug.LogError("【取引履歴】UI参照が未設定です");
            return;
        }

        string path = Path.Combine(
            Application.streamingAssetsPath,
            fileName
        );

        if (!File.Exists(path))
        {
            Debug.LogError("【取引履歴】JSONが見つかりません: " + path);
            return;
        }

        try
        {
            string json = File.ReadAllText(path);

            TradeHistoryData history =
                JsonUtility.FromJson<TradeHistoryData>(json);

            if (history == null || history.trades == null)
            {
                Debug.LogError("【取引履歴】JSONの形式が不正です");
                return;
            }

            // 以前表示した履歴を削除
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            // 新しい取引から表示
            for (int i = history.trades.Length - 1; i >= 0; i--)
            {
                TradeData trade = history.trades[i];

                TradeHistoryItemUI item = Instantiate(
                    itemPrefab,
                    content
                );

                item.Setup(
                    trade.type,
                    trade.stockName,
                    trade.dateTime,
                    trade.quantity,
                    trade.realizedProfit
                );
            }

            Debug.Log(
                $"【取引履歴表示成功】{history.trades.Length}件"
            );
        }
        catch (Exception e)
        {
            Debug.LogError("【取引履歴読込エラー】" + e.Message);
        }
    }
}

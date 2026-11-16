
using System;
using System.IO;
using UnityEngine;

public class TradeHistoryListUI : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private Transform content;
    [SerializeField] private TradeHistoryItemUI itemPrefab;
    [SerializeField] private TradeDetailUI tradeDetailUI;
    [SerializeField] private TradeChartController tradeChartController;

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
            Debug.LogError(
                "【取引履歴】JSONが見つかりません: " + path
            );
            return;
        }

        try
        {
            string json = File.ReadAllText(path);

            TradeHistoryData history =
                JsonUtility.FromJson<TradeHistoryData>(json);

            if (history == null || history.trades == null)
            {
                Debug.LogError(
                    "【取引履歴】JSONの形式が不正です"
                );
                return;
            }

            // 古い表示を削除
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            // 新しい取引から表示
            for (int i = history.trades.Length - 1; i >= 0; i--)
            {
                TradeData trade = history.trades[i];
                int selectedIndex = i;

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

                double profit = trade.realizedProfit;

                if (item.button != null)
                {
                    item.button.onClick.AddListener(() =>
                    {
                        // 実現損益を更新
                        if (tradeDetailUI != null)
                        {
                            tradeDetailUI.SetProfit(profit);
                        }

                        if (tradeChartController != null)
                        {
                            DateTime startDate;
                            DateTime endDate;

                            bool found = TryGetHoldingPeriod(
                                history.trades,
                                selectedIndex,
                                out startDate,
                                out endDate
                            );

                            if (found)
                            {
                                // 取引履歴をグラフ側に渡す
                                SendTradeMarkers(
                                    history.trades,
                                    selectedIndex,
                                    startDate,
                                    endDate
                                );

                                // 取引開始日の7日前から取得
                                // 休場日でも過去の株価を表示する
                                string start =
                                    startDate.AddDays(-7)
                                    .ToString("yyyy-MM-dd");

                                // 取引終了日の翌日まで取得
                                string end =
                                    endDate.AddDays(1)
                                    .ToString("yyyy-MM-dd");

                                tradeChartController.ShowChart(
                                    trade.ticker,
                                    start,
                                    end,
                                    "1d"
                                );
                            }
                            else
                            {
                                Debug.LogWarning(
                                    "【取引履歴】保有期間を判定できません"
                                );
                            }
                        }
                    });
                }
            }

            Debug.Log(
                $"【取引履歴表示成功】{history.trades.Length}件"
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "【取引履歴読込エラー】" + e.Message
            );
        }
    }

    // 購入・売却履歴をグラフ側に渡す
    private void SendTradeMarkers(
        TradeData[] trades,
        int selectedIndex,
        DateTime startDate,
        DateTime endDate
    )
    {
        if (tradeChartController == null)
            return;

        // 前回の取引マーカーを削除して重複を防止
        tradeChartController.ClearTradeMarkers();

        string ticker = trades[selectedIndex].ticker;

        foreach (TradeData trade in trades)
        {
            if (trade.ticker != ticker)
                continue;

            DateTime tradeDate;

            if (!DateTime.TryParse(
                trade.dateTime,
                out tradeDate
            ))
            {
                continue;
            }

            // 選択した保有期間内の取引だけを対象にする
            if (tradeDate < startDate ||
                tradeDate > endDate)
            {
                continue;
            }

            tradeChartController.AddTradeMarker(
                trade.type,
                trade.dateTime,
                trade.quantity,
                trade.price
            );
        }
    }

    // 選択した取引が属する保有期間を取得
    private bool TryGetHoldingPeriod(
        TradeData[] trades,
        int selectedIndex,
        out DateTime startDate,
        out DateTime endDate
    )
    {
        startDate = DateTime.MinValue;
        endDate = DateTime.MinValue;

        if (trades == null ||
            selectedIndex < 0 ||
            selectedIndex >= trades.Length)
        {
            return false;
        }

        string ticker = trades[selectedIndex].ticker;

        int holdings = 0;
        int periodStartIndex = -1;

        for (int i = 0; i < trades.Length; i++)
        {
            TradeData trade = trades[i];

            if (trade.ticker != ticker)
                continue;

            // 保有株数が0なら新しい保有期間
            if (holdings == 0)
            {
                periodStartIndex = i;
            }

            if (trade.type == "buy")
            {
                holdings += trade.quantity;
            }
            else if (trade.type == "sell")
            {
                holdings -= trade.quantity;
            }

            if (holdings < 0)
            {
                Debug.LogWarning(
                    "【取引履歴】保有株数がマイナスになりました"
                );
                return false;
            }

            // 選択した取引の保有開始日を記録
            if (i == selectedIndex)
            {
                startDate = DateTime.Parse(
                    trades[periodStartIndex].dateTime
                );
            }

            // 全株売却した場合
            if (holdings == 0 && periodStartIndex >= 0)
            {
                if (selectedIndex >= periodStartIndex &&
                    selectedIndex <= i)
                {
                    startDate = DateTime.Parse(
                        trades[periodStartIndex].dateTime
                    );

                    endDate = DateTime.Parse(
                        trade.dateTime
                    );

                    return true;
                }
            }
        }

        // まだ保有中の場合
        if (startDate != DateTime.MinValue)
        {
            endDate = DateTime.Now;
            return true;
        }

        return false;
    }
}

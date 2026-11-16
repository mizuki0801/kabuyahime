
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeReflectionManager : MonoBehaviour
{
    [Header("振り返りUI")]
    [SerializeField] private TMP_InputField reflectionInput;
    [SerializeField] private Button saveButton;
    [SerializeField] private TMP_Text statusText;

    [Header("JSON設定")]
    [SerializeField] private string fileName = "trade_reviews.json";

    [Serializable]
    private class Review
    {
        public string tradeId;
        public string reflection;
        public string aiFeedback;
        public string updatedAt;
    }

    [Serializable]
    private class ReviewFile
    {
        public List<Review> reviews = new List<Review>();
    }

    // 旧データの読み込み専用
    [Serializable]
    private class LegacyEntry
    {
        public string tradeId;
        public string text;
    }

    [Serializable]
    private class LegacyFile
    {
        public List<LegacyEntry> entries =
            new List<LegacyEntry>();
    }

    private ReviewFile data = new ReviewFile();
    private LegacyFile legacyData = new LegacyFile();

    private string selectedTradeId;

    // 読み込み・保存先をStreamingAssetsに統一
    private string SavePath => Path.Combine(
        Application.streamingAssetsPath,
        fileName
    );

    // 以前の自動保存データは読み込み専用
    private string LegacyPath => Path.Combine(
        Application.persistentDataPath,
        "trade_reflections.json"
    );

    private void Awake()
    {
        FindUI();
        LoadReviews();
        LoadLegacyReviews();

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveReflection);
            saveButton.interactable = false;
        }

        if (reflectionInput != null)
        {
            reflectionInput.interactable = false;
        }
    }

    private void FindUI()
    {
        if (reflectionInput == null)
        {
            TMP_InputField[] inputs =
                FindObjectsByType<TMP_InputField>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (TMP_InputField input in inputs)
            {
                if (input.name == "ReflectionInputField")
                {
                    reflectionInput = input;
                    break;
                }
            }
        }

        if (saveButton == null)
        {
            Button[] buttons =
                FindObjectsByType<Button>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (Button button in buttons)
            {
                if (button.name == "ReflectionSaveButton")
                {
                    saveButton = button;
                    break;
                }
            }
        }

        if (reflectionInput == null)
        {
            Debug.LogError(
                "【振り返り】ReflectionInputFieldが見つかりません"
            );
        }

        if (saveButton == null)
        {
            Debug.LogError(
                "【振り返り】ReflectionSaveButtonが見つかりません"
            );
        }
    }

    private void OnDestroy()
    {
        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(SaveReflection);
        }
    }

    private void LoadReviews()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning(
                "【振り返り】JSONが見つかりません: " +
                SavePath
            );
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);

            ReviewFile loaded =
                JsonUtility.FromJson<ReviewFile>(json);

            if (loaded == null || loaded.reviews == null)
            {
                Debug.LogError(
                    "【振り返り】JSON形式が不正です"
                );
                return;
            }

            data = loaded;

            Debug.Log(
                "【振り返り読込成功】" + SavePath
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "【振り返り読込失敗】" + e.Message
            );
        }
    }

    private void LoadLegacyReviews()
    {
        if (!File.Exists(LegacyPath))
            return;

        try
        {
            LegacyFile loaded =
                JsonUtility.FromJson<LegacyFile>(
                    File.ReadAllText(LegacyPath)
                );

            if (loaded != null && loaded.entries != null)
            {
                legacyData = loaded;
            }

            Debug.Log(
                "【旧振り返り読込成功】" + LegacyPath
            );
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "【旧振り返り読込失敗】" + e.Message
            );
        }
    }

    // 6引数版：取引IDで管理
    public void SelectTrade(
        string id,
        string ticker,
        string dateTime,
        string type,
        int quantity,
        float price
    )
    {
        string legacyId = MakeLegacyId(
            ticker,
            dateTime,
            type,
            quantity,
            price
        );

        selectedTradeId = string.IsNullOrEmpty(id)
            ? legacyId
            : id;

        Review review = data.reviews.Find(
            r => r.tradeId == selectedTradeId
        );

        // 旧データがあれば引き継ぐ
        if (review == null)
        {
            LegacyEntry oldEntry = legacyData.entries.Find(
                e => e.tradeId == legacyId
            );

            if (oldEntry != null)
            {
                review = new Review
                {
                    tradeId = selectedTradeId,
                    reflection = oldEntry.text,
                    aiFeedback = "",
                    updatedAt = ""
                };

                data.reviews.Add(review);

                Debug.Log(
                    "【振り返り移行】" + selectedTradeId
                );
            }
        }

        if (reflectionInput != null)
        {
            reflectionInput.interactable = true;

            reflectionInput.SetTextWithoutNotify(
                review != null
                    ? review.reflection ?? ""
                    : ""
            );
        }

        if (saveButton != null)
        {
            saveButton.interactable =
                reflectionInput != null;
        }

        SetStatus(
            review != null
                ? "保存済みの振り返りを読み込みました"
                : "振り返りを入力できます"
        );
    }

    // 旧5引数版との互換性
    public void SelectTrade(
        string ticker,
        string dateTime,
        string type,
        int quantity,
        float price
    )
    {
        SelectTrade(
            "",
            ticker,
            dateTime,
            type,
            quantity,
            price
        );
    }

    public void SaveReflection()
    {
        if (string.IsNullOrEmpty(selectedTradeId) ||
            reflectionInput == null)
        {
            SetStatus("先に取引を選択してください");
            return;
        }

        Review review = data.reviews.Find(
            r => r.tradeId == selectedTradeId
        );

        if (review == null)
        {
            review = new Review
            {
                tradeId = selectedTradeId,
                aiFeedback = ""
            };

            data.reviews.Add(review);
        }

        review.reflection = reflectionInput.text;
        review.updatedAt =
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        try
        {
            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(SavePath, json);

            SetStatus("振り返りを保存しました");

            Debug.Log(
                "【振り返り保存成功】" + SavePath
            );
        }
        catch (Exception e)
        {
            SetStatus("保存に失敗しました");

            Debug.LogError(
                "【振り返り保存失敗】" + e.Message
            );
        }
    }

    private static string MakeLegacyId(
        string ticker,
        string dateTime,
        string type,
        int quantity,
        float price
    )
    {
        return ticker + "|" +
               dateTime + "|" +
               type + "|" +
               quantity + "|" +
               price.ToString(
                   "R",
                   CultureInfo.InvariantCulture
               );
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}

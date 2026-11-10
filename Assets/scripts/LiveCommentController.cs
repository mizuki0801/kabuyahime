
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LiveCommentController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField commentInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private RectTransform commentContent;
    [SerializeField] private TMP_Text commentPrefab;

    [Header("User Settings")]
    [SerializeField] private string userName = "あなた";

    [Header("AI Character")]
    [SerializeField] private string aiCharacterName = "麺田　食子";

    [Header("Test Settings")]
    [SerializeField] private bool enableTestAIReply = false;

    [Header("SuperChat")]
    [SerializeField] private RectTransform giftGrid;
    [SerializeField] private Color selectedGiftColor =
        new Color(1f, 0.82f, 0.35f, 1f);

    [Header("Coin Settings")]
    [SerializeField] private TMP_Text coinBalanceText;
    [SerializeField] private TMP_Text selectedGiftPriceText;

    [Header("Gift Prices (GiftGrid Order)")]
    [SerializeField] private int[] giftPrices =
    {
        //ギフトの価格設定
        //指ハート
        10,
        //通用＆印鑑 
        30,
        //バラ 
        50,
        // アップダウン
        100, 
        //札束
        200, 
        //マウス
        500,
        //花束
        1000, 
        //PC
        2000, 
        //冠
        3000, 
        //剣
        5000, 
        //指輪
        8000, 
        //ケーキ
        10000
    };

    [Header("Settings")]
    [SerializeField] private int maxComments = 30;

    [Serializable]
    private class UserData
    {
        public int investmentCoins;
        public int oshiCoins;
    }

    private readonly Queue<GameObject> comments = new();
    private readonly List<Button> giftButtons = new();
    private readonly Dictionary<Button, Color> originalColors = new();

    private Button selectedGift;
    private int selectedGiftIndex = -1;

    private int currentCoins;
    private UserData userData;
    private string savePath;

    private void Start()
    {
        savePath = Path.Combine(
            Application.persistentDataPath,
            "user.json"
        );

        LoadUserData();
        UpdateCoinUI();

        if (sendButton != null)
            sendButton.onClick.AddListener(SendComment);

        if (commentInput != null)
            commentInput.onSubmit.AddListener(OnSubmit);

        SetupGiftButtons();
    }

    private void LoadUserData()
    {
        string defaultPath = Path.Combine(
            Application.streamingAssetsPath,
            "user.json"
        );

        try
        {
            string loadPath = File.Exists(savePath)
                ? savePath
                : defaultPath;

            if (!File.Exists(loadPath))
            {
                Debug.LogError(
                    "【JSONエラー】user.jsonが見つかりません: "
                    + loadPath
                );

                userData = new UserData();
                currentCoins = 0;
                return;
            }

            string json = File.ReadAllText(loadPath);

            userData = JsonUtility.FromJson<UserData>(json);

            if (userData == null)
            {
                Debug.LogError("【JSONエラー】データが不正です");
                userData = new UserData();
            }

            currentCoins = Mathf.Max(0, userData.oshiCoins);

            Debug.Log(
                $"【推し活コイン読込成功】{currentCoins} / {loadPath}"
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "【JSON読込失敗】" + e.Message
            );

            userData = new UserData();
            currentCoins = 0;
        }
    }

    private bool SaveUserData()
    {
        if (userData == null)
            return false;

        try
        {
            // 保存直前に最新のデータを取得し、
            // 他の項目をなるべく保持する
            if (File.Exists(savePath))
            {
                string latestJson = File.ReadAllText(savePath);

                UserData latestData =
                    JsonUtility.FromJson<UserData>(latestJson);

                if (latestData != null)
                    userData = latestData;
            }

            userData.oshiCoins = currentCoins;

            string json = JsonUtility.ToJson(userData, true);

            Directory.CreateDirectory(
                Application.persistentDataPath
            );

            File.WriteAllText(savePath, json);

            Debug.Log(
                $"【推し活コイン保存成功】{currentCoins}"
            );

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(
                "【JSON保存失敗】" + e.Message
            );

            return false;
        }
    }

    private void SetupGiftButtons()
    {
        if (giftGrid == null)
        {
            Debug.LogWarning("GiftGridが未設定です。");
            return;
        }

        if (giftPrices == null ||
            giftPrices.Length != giftGrid.childCount)
        {
            Debug.LogWarning(
                "ギフト数と価格設定の数が一致していません。"
            );
        }

        for (int i = 0; i < giftGrid.childCount; i++)
        {
            Button button =
                giftGrid.GetChild(i).GetComponent<Button>();

            if (button == null)
                continue;

            giftButtons.Add(button);

            Image image = button.GetComponent<Image>();

            if (image != null)
                originalColors[button] = image.color;

            Button currentButton = button;
            int currentIndex = i;

            button.onClick.AddListener(
                () => SelectGift(currentButton, currentIndex)
            );
        }
    }

    private void SelectGift(Button button, int index)
    {
        if (selectedGift == button)
        {
            ClearGiftSelection();
            return;
        }

        ClearGiftSelection();

        selectedGift = button;
        selectedGiftIndex = index;

        Image image = button.GetComponent<Image>();

        if (image != null)
            image.color = selectedGiftColor;

        UpdateCoinUI();
    }

    private void ClearGiftSelection()
    {
        if (selectedGift != null)
        {
            Image image = selectedGift.GetComponent<Image>();

            if (image != null &&
                originalColors.TryGetValue(
                    selectedGift, out Color color))
            {
                image.color = color;
            }
        }

        selectedGift = null;
        selectedGiftIndex = -1;

        UpdateCoinUI();
    }

    private int GetGiftPrice(int index)
    {
        if (giftPrices == null ||
            index < 0 ||
            index >= giftPrices.Length)
        {
            return -1;
        }

        return giftPrices[index];
    }

    private void UpdateCoinUI()
    {
        if (coinBalanceText != null)
        {
            coinBalanceText.text =
                currentCoins.ToString("N0");
        }

        if (selectedGiftPriceText != null)
        {
            int price = GetGiftPrice(selectedGiftIndex);

            selectedGiftPriceText.text =
                price >= 0
                    ? price.ToString("N0") + " コイン"
                    : "";
        }
    }

    private void OnSubmit(string value)
    {
        SendComment();
    }

    public void SendComment()
    {
        string message = commentInput != null
            ? commentInput.text.Trim()
            : "";

        bool hasMessage = !string.IsNullOrEmpty(message);
        bool hasGift = selectedGift != null;

        if (!hasMessage && !hasGift)
            return;

        if (hasGift)
        {
            int price = GetGiftPrice(selectedGiftIndex);

            if (price < 0)
            {
                Debug.LogWarning(
                    "ギフトの価格が未設定です。"
                );
                return;
            }

            if (currentCoins < price)
            {
                Debug.LogWarning(
                    "コインが不足しています。"
                );
                return;
            }

            string giftName =
                GetGiftDisplayName(
                    selectedGift.gameObject.name
                );

            int previousCoins = currentCoins;

            currentCoins -= price;

            // 保存失敗時はコインを戻して送信中止
            if (!SaveUserData())
            {
                currentCoins = previousCoins;
                UpdateCoinUI();
                return;
            }

            UpdateCoinUI();

            string giftMessage =
                $"{giftName}を贈りました！";

            if (hasMessage)
                giftMessage += $" {message}";

            AddGiftComment(userName, giftMessage);
        }
        else
        {
            AddComment(userName, message);
        }

        if (commentInput != null)
        {
            commentInput.text = "";
            commentInput.ActivateInputField();
        }

        // テスト用：コメント／スパチャ送信後にキャラクターが自動返信する
        if (enableTestAIReply)
        {
            AddComment(
                aiCharacterName,
                hasGift ? "スパチャありがとうございます！" : "コメントありがとうございます！"
            );
        }

        ClearGiftSelection();
    }

    private string GetGiftDisplayName(string objectName)
    {
        switch (objectName)
        {
            case "Finger": return "指ハート";
            case "BankBook": return "通帳＆印鑑";
            case "Rose": return "バラ";
            case "Icon": return "株価アイコン";
            case "Many": return "お札の束";
            case "Maus": return "ハートマウス";
            case "Flower": return "花束";
            case "PC": return "投資PCセット";
            case "King": return "投資の王冠";
            case "Soud": return "勇者の剣";
            case "Fingerring": return "ダイヤの指輪";
            case "Cake": return "ウェディングケーキ";
            default: return objectName;
        }
    }

    public void AddComment(
        string username,
        string message)
    {
        string nameColor =
            username == aiCharacterName
                ? "#F7B3C8"
                : "#E9B75A";

        CreateComment(
            $"<color={nameColor}>{username}</color>：{message}"
        );
    }

    private void AddGiftComment(
        string username,
        string message)
    {
        CreateComment(
            $"<color=#E9B75A>{username}</color>：" +
            $"<color=#FFD36B>{message}</color>"
        );
    }

    private void CreateComment(string text)
    {
        if (commentPrefab == null ||
            commentContent == null)
            return;

        TMP_Text item =
            Instantiate(commentPrefab, commentContent);

        item.gameObject.SetActive(true);
        item.richText = true;
        item.text = text;

        comments.Enqueue(item.gameObject);

        while (comments.Count > Mathf.Max(1, maxComments))
        {
            Destroy(comments.Dequeue());
        }

        Canvas.ForceUpdateCanvases();
    }

    private void OnDestroy()
    {
        if (sendButton != null)
            sendButton.onClick.RemoveListener(SendComment);

        if (commentInput != null)
            commentInput.onSubmit.RemoveListener(OnSubmit);
    }
}

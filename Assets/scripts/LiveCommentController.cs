
using System.Collections.Generic;
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

    [Header("Settings")]
    [SerializeField] private int maxComments = 30;

    [Header("Comment Colors")]
    [SerializeField] private Color32 userNameColor =
        new Color32(233, 183, 90, 255);

    [SerializeField] private Color32 aiNameColor =
        new Color32(247, 179, 200, 255);

    [SerializeField] private Color32 messageColor =
        new Color32(255, 255, 255, 255);

    [Header("AI Character")]
    [SerializeField] private string aiCharacterName = "麺田　食子";

    [Header("Test Settings")]
    [SerializeField] private bool enableTestAIReply = true;

    private readonly Queue<GameObject> comments = new();

    private void Start()
    {
        sendButton.onClick.AddListener(SendComment);
        commentInput.onSubmit.AddListener(OnSubmit);
    }

    private void OnSubmit(string value)
    {
        SendComment();
    }

    public void SendComment()
    {
        string message = commentInput.text.Trim();

        if (string.IsNullOrEmpty(message))
            return;

        // ユーザーのコメントを表示
        AddComment("あなた", message);

        commentInput.text = "";
        commentInput.ActivateInputField();

        // AI未完成時のテスト返信
        if (enableTestAIReply)
        {
            AddAIComment("コメントありがとう！");
        }
    }

    // 通常コメントの追加
    public void AddComment(string username, string message)
    {
        bool isUser = username == "あなた";

        Color32 nameColor = isUser
            ? userNameColor
            : aiNameColor;

        CreateComment(username, message, nameColor);
    }

    // AIキャラクター専用
    public void AddAIComment(string message)
    {
        CreateComment(
            aiCharacterName,
            message,
            aiNameColor
        );
    }

    // コメント生成処理
    private void CreateComment(
        string username,
        string message,
        Color32 nameColor)
    {
        if (commentPrefab == null || commentContent == null)
            return;

        TMP_Text item = Instantiate(
            commentPrefab,
            commentContent
        );

        item.gameObject.SetActive(true);
        item.richText = true;

        string nameHex = ColorUtility.ToHtmlStringRGB(nameColor);
        string messageHex = ColorUtility.ToHtmlStringRGB(messageColor);

        // 入力文字がTMPタグとして解釈されるのを防ぐ
        string safeName = EscapeRichText(username);
        string safeMessage = EscapeRichText(message);

        item.text =
            $"<color=#{nameHex}>{safeName}</color>" +
            $"<color=#{messageHex}>：{safeMessage}</color>";

        comments.Enqueue(item.gameObject);

        while (comments.Count > Mathf.Max(1, maxComments))
        {
            Destroy(comments.Dequeue());
        }

        Canvas.ForceUpdateCanvases();
    }

    // Rich Text用エスケープ
    private string EscapeRichText(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    private void OnDestroy()
    {
        if (sendButton != null)
            sendButton.onClick.RemoveListener(SendComment);

        if (commentInput != null)
            commentInput.onSubmit.RemoveListener(OnSubmit);
    }
}

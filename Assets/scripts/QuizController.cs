
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizController : MonoBehaviour
{
    [Header("選択肢ボタン")]
    public Button answerAButton;
    public Button answerBButton;
    public Button answerCButton;

    [Header("次のレッスン")]
    public Button nextLessonButton;

    [Header("正誤メッセージ")]
    public TMP_Text oshiCommentText;

    [Header("レッスン管理")]
    public LessonMenuController lessonMenuController;

    private string correctAnswer = "";
    private bool isAnswered = false;
    [Header("正誤判定の色")]
    public Color normalColor = Color.white;
    public Color correctColor = new Color32(150, 230, 160, 255);
    public Color wrongColor = new Color32(255, 150, 150, 255);

    // JSONから正解を設定
    public void SetCorrectAnswer(string answer)
    {
        correctAnswer = string.IsNullOrEmpty(answer)
            ? ""
            : answer.Trim().ToUpperInvariant();

        ResetQuiz();
    }

    // Aを選択
    public void SelectA()
    {
        CheckAnswer("A");
    }

    // Bを選択
    public void SelectB()
    {
        CheckAnswer("B");
    }

    // Cを選択
    public void SelectC()
    {
        CheckAnswer("C");
    }

    // 正誤判定

    private void CheckAnswer(string selectedAnswer)
    {
        if (isAnswered)
            return;

        if (string.IsNullOrEmpty(correctAnswer))
        {
            Debug.LogWarning("クイズの正解が設定されていません");
            return;
        }

        if (selectedAnswer == correctAnswer)
        {
            isAnswered = true;

            // 正解したボタンを緑にする
            ChangeAnswerColor(selectedAnswer, correctColor);

            if (oshiCommentText != null)
            {
                oshiCommentText.text = "正解！よくできました！";
            }

            if (lessonMenuController != null)
            {
                lessonMenuController.ClearLesson(
                lessonMenuController.GetSelectedLesson()
                );
            }

            if (nextLessonButton != null)
            {
                nextLessonButton.interactable = true;
            }

            Debug.Log("クイズ正解！");
        }
        else
        {
            // 選択したボタンを赤にする
            ChangeAnswerColor(selectedAnswer, wrongColor);

            if (oshiCommentText != null)
            {
                oshiCommentText.text = "不正解！もう一度考えてみよう！";
            }

            Debug.Log("クイズ不正解！");
        }
}


    // レッスン切り替え時の初期化
    public void ResetQuiz()
    {
        isAnswered = false;

        if (oshiCommentText != null)
        {
            oshiCommentText.text =
                "正しい答えを選んでね！";
        }

        if (nextLessonButton != null)
        {
            nextLessonButton.interactable = false;
        }
        if (answerAButton != null)
        {
            answerAButton.image.color = normalColor;
        }
        if (answerBButton != null)
        {
            answerBButton.image.color = normalColor;
        }
        if (answerCButton != null)
        {
            answerCButton.image.color = normalColor;
        }
    }
    private void ChangeAnswerColor(string answer, Color color)
    {
        Button targetButton = null;

        switch (answer)
        {
            case "A":
                targetButton = answerAButton;
                break;
            case "B":
                targetButton = answerBButton;
                break;
            case "C":
                targetButton = answerCButton;
                break;
        }

        if (targetButton != null && targetButton.image != null)
        {
            targetButton.image.color = color;
        }
    }
}

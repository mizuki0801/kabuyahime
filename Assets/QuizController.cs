using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizController : MonoBehaviour
{
    public Button answerAButton;
    public Button answerBButton;
    public Button answerCButton;

    public Button nextLessonButton;

    public TMP_Text oshiCommentText;

    public LessonMenuController lessonMenuController;

    public Color normalColor = Color.white;
    public Color correctColor = new Color(0.75f, 0.95f, 0.78f);
    public Color wrongColor = new Color(1f, 0.75f, 0.75f);

    public void SelectA()
    {
        ShowWrong(answerAButton);
    }

    public void SelectB()
    {
        ResetColors();

        answerBButton.image.color = correctColor;

        oshiCommentText.text =
            "正解たい！\nしっかり理解\nできとるね！";

        // 今選んでいるLessonをクリア状態にする
        lessonMenuController.ClearLesson(
            lessonMenuController.GetSelectedLesson()
        );

        // 正解したら「次のレッスンへ」を押せるようにする
        nextLessonButton.interactable = true;
    }

    public void SelectC()
    {
        ShowWrong(answerCButton);
    }

    private void ShowWrong(Button selectedButton)
    {
        ResetColors();

        selectedButton.image.color = wrongColor;
        answerBButton.image.color = correctColor;

        oshiCommentText.text =
            "おしい！\n正解はBたい！\nもう一回確認しよ！";

        // 不正解なら次へ進めない
        nextLessonButton.interactable = false;
    }

    private void ResetColors()
    {
        answerAButton.image.color = normalColor;
        answerBButton.image.color = normalColor;
        answerCButton.image.color = normalColor;
    }

    // Lessonを切り替えたときに呼ばれる
    public void ResetQuiz()
    {
        ResetColors();

        oshiCommentText.text =
            "問題に挑戦してみよ！";

        // 新しいLessonでは、正解するまで次へ進めない
        nextLessonButton.interactable = false;
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LessonMenuController : MonoBehaviour
{
    public Button[] lessonButtons;

    public TMP_Text lessonTitle;
    public TMP_Text lessonDescription;
    public TMP_Text quizQuestion;

    public TMP_Text answerAText;
    public TMP_Text answerBText;
    public TMP_Text answerCText;

    public TMP_Text triviaText;
    public TMP_Text oshiCommentText;

    // 追加
    public QuizController quizController;

    public Color normalColor = new Color(1f, 1f, 1f, 0.85f);
    public Color selectedColor = new Color(0.49f, 0.29f, 0.75f, 1f);
    public Color clearedColor = new Color(0.35f, 0.75f, 0.45f, 1f);

    private bool[] clearedLessons;
    private int selectedLesson = 2;

    void Start()
    {
        clearedLessons = new bool[lessonButtons.Length];

        // 最初は「3. 株主とは？」を表示
        SelectLesson(2);
    }

    // 左のLessonを押したとき
    public void SelectLesson(int index)
    {
        selectedLesson = index;

        UpdateLessonContent();
        UpdateButtonColors();

        // クイズの色・吹き出しをリセット
        if (quizController != null)
        {
            quizController.ResetQuiz();
        }
    }

    // 正解したLessonを緑にする
    public void ClearLesson(int index)
    {
        if (index >= 0 && index < clearedLessons.Length)
        {
            clearedLessons[index] = true;
            UpdateButtonColors();
        }
    }

    // 現在選択中のLesson番号
    public int GetSelectedLesson()
    {
        return selectedLesson;
    }

    // 次のレッスンへ
    public void GoToNextLesson()
    {
        int nextLesson = selectedLesson + 1;

        if (nextLesson < lessonButtons.Length)
        {
            SelectLesson(nextLesson);
        }
    }

    // 左メニューの色
    void UpdateButtonColors()
    {
        for (int i = 0; i < lessonButtons.Length; i++)
        {
            if (clearedLessons[i])
            {
                lessonButtons[i].image.color = clearedColor;
            }
            else if (i == selectedLesson)
            {
                lessonButtons[i].image.color = selectedColor;
            }
            else
            {
                lessonButtons[i].image.color = normalColor;
            }
        }
    }

    // Lesson内容の切り替え
    void UpdateLessonContent()
    {
        switch (selectedLesson)
        {
            case 0:
                lessonTitle.text = "1. 株とは？";

                lessonDescription.text =
                    "株とは、会社が事業に必要なお金を集めるために発行するものです。\n" +
                    "株を買うことで、その会社の一部を持つことになります。";

                quizQuestion.text =
                    "Q. 株とは何を表すもの？";

                answerAText.text =
                    "A 会社から借りたお金";

                answerBText.text =
                    "B 会社の一部を持つ権利";

                answerCText.text =
                    "C 商品を買うための券";

                triviaText.text =
                    "豆知識\n" +
                    "株を持っている人のことを\n" +
                    "「株主」と呼ぶとよ！";
                break;

            case 1:
                lessonTitle.text = "2. 株式会社とは？";

                lessonDescription.text =
                    "株式会社とは、株式を発行して集めたお金を使い、事業を行う会社です。\n" +
                    "多くの人から資金を集められる特徴があります。";

                quizQuestion.text =
                    "Q. 株式会社が株式を発行する主な目的は？";

                answerAText.text =
                    "A 社員を増やすため";

                answerBText.text =
                    "B 事業に必要な資金を集めるため";

                answerCText.text =
                    "C 商品の価格を決めるため";

                triviaText.text =
                    "豆知識\n" +
                    "株式を発行することで、\n" +
                    "大きな事業にも挑戦しやすくなるとよ！";
                break;

            case 2:
                lessonTitle.text = "3. 株主とは？";

                lessonDescription.text =
                    "株主とは、会社の株式を保有している人のことです。\n" +
                    "株主になると、会社の利益の一部を受け取ったり、\n" +
                    "会社の重要な決定に参加する権利を得ることがあります。";

                quizQuestion.text =
                    "Q. 株主について正しいものはどれ？";

                answerAText.text =
                    "A 会社にお金を貸している人";

                answerBText.text =
                    "B 会社の株式を保有している人";

                answerCText.text =
                    "C 会社で働いている人";

                triviaText.text =
                    "豆知識\n" +
                    "株主には、会社の利益に応じて\n" +
                    "「配当金」がもらえる場合もあるとよ！";
                break;

            case 3:
                lessonTitle.text = "4. 株価とは？";

                lessonDescription.text =
                    "株価とは、株式市場で売買されている株1株あたりの価格です。\n" +
                    "会社の業績や将来への期待、ニュースなどによって変化します。";

                quizQuestion.text =
                    "Q. 株価について正しいものはどれ？";

                answerAText.text =
                    "A 一度決まったら変わらない";

                answerBText.text =
                    "B 売買の状況などによって変化する";

                answerCText.text =
                    "C 会社の社員が自由に決める";

                triviaText.text =
                    "豆知識\n" +
                    "買いたい人が増えると株価は上がり、\n" +
                    "売りたい人が増えると下がりやすいとよ！";
                break;

            case 4:
                lessonTitle.text = "5. 配当金とは？";

                lessonDescription.text =
                    "配当金とは、会社が得た利益の一部を株主に還元するお金です。\n" +
                    "ただし、すべての会社が必ず配当金を出すわけではありません。";

                quizQuestion.text =
                    "Q. 配当金とはどのようなお金？";

                answerAText.text =
                    "A 株を買うときに払う手数料";

                answerBText.text =
                    "B 会社の利益の一部として株主が受け取るお金";

                answerCText.text =
                    "C 会社から借りるお金";

                triviaText.text =
                    "豆知識\n" +
                    "配当金の金額や有無は、\n" +
                    "会社によって違うとよ！";
                break;
        }
    }
}
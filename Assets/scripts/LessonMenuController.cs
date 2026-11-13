
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LessonMenuController : MonoBehaviour
{
    [Header("JSON設定")]
    public string jsonFileName = "lessons.json";

    [Header("レッスン一覧")]
    public Transform contentParent;
    public GameObject lessonButtonPrefab;

    [Header("学習内容")]
    public TMP_Text lessonTitle;
    public TMP_Text lessonDescription;

    [Header("クイズ")]
    public TMP_Text quizQuestion;
    public TMP_Text answerAText;
    public TMP_Text answerBText;
    public TMP_Text answerCText;

    [Header("クイズ管理")]
    public QuizController quizController;

    private LessonDataList lessonDataList;
    private int selectedLessonIndex = 0;
    private bool[] clearedLessons;
    private List<Button> lessonButtons = new List<Button>();

    void Start()
    {
        LoadLessons();
    }

    void LoadLessons()
    {
        string path = Path.Combine(
            Application.streamingAssetsPath,
            jsonFileName
        );

        if (!File.Exists(path))
        {
            Debug.LogError("JSONファイルが見つかりません: " + path);
            return;
        }

        string json = File.ReadAllText(path);

        lessonDataList = JsonUtility.FromJson<LessonDataList>(json);

        if (lessonDataList == null ||
            lessonDataList.lessons == null ||
            lessonDataList.lessons.Length == 0)
        {
            Debug.LogError("レッスンデータがありません");
            return;
        }

        Debug.Log(
            "レッスン読み込み成功: " +
            lessonDataList.lessons.Length + "件"
        );
        clearedLessons = new bool[lessonDataList.lessons.Length];

        CreateLessonButtons();
        SelectLesson(0);
    }

    void CreateLessonButtons()
    {
        lessonButtons.Clear();
        if (contentParent == null || lessonButtonPrefab == null)
        {
            Debug.LogError("ContentまたはボタンPrefabが未設定です");
            return;
        }

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < lessonDataList.lessons.Length; i++)
        {
            int index = i;
            LessonData lesson = lessonDataList.lessons[i];

            GameObject buttonObject = Instantiate(
                lessonButtonPrefab,
                contentParent
            );

            Button button = buttonObject.GetComponent<Button>();

            TMP_Text buttonText =
                buttonObject.GetComponentInChildren<TMP_Text>();

            if (button == null || buttonText == null)
            {
                Debug.LogError(
                    "レッスンボタンPrefabの設定が不足しています"
                );
                continue;
            }

            buttonText.text = lesson.id + ". " + lesson.title;

            button.onClick.AddListener(() => SelectLesson(index));
            lessonButtons.Add(button);
        }
    }

    public void SelectLesson(int index)
    {
        if (lessonDataList == null ||
            lessonDataList.lessons == null ||
            index < 0 ||
            index >= lessonDataList.lessons.Length)
        {
            return;
        }      

        selectedLessonIndex = index;

        // ① 最初にlessonを宣言する
        LessonData lesson = lessonDataList.lessons[index];

        // ② 宣言した後に使用する
        if (quizController != null)
        {
            quizController.SetCorrectAnswer(lesson.correctAnswer);
        }

        if (lessonTitle != null)
            lessonTitle.text = lesson.id + ". " + lesson.title;

        if (lessonDescription != null)
            lessonDescription.text = lesson.description;

        if (quizQuestion != null)
            quizQuestion.text = lesson.question;

        if (answerAText != null)
            answerAText.text = lesson.answerA;

        if (answerBText != null)
            answerBText.text = lesson.answerB;

        if (answerCText != null)
            answerCText.text = lesson.answerC;

        UpdateButtonColors();

        Debug.Log("レッスン選択: " + lesson.title);
    }

    public void GoToNextLesson()
    {
        if (lessonDataList == null ||
            lessonDataList.lessons == null)
        {
            return;
        }

        int nextIndex = selectedLessonIndex + 1;

        if (nextIndex < lessonDataList.lessons.Length)
        {
            SelectLesson(nextIndex);
        }
    }

    // 現在選択中のレッスン番号を取得
    public int GetSelectedLesson()
    {
        return selectedLessonIndex;
    }

    // レッスンをクリア済みにする
    public void ClearLesson(int index)
    {
        if (clearedLessons == null ||
            index < 0 ||
            index >= clearedLessons.Length)
        {
            return;
        }

        clearedLessons[index] = true;

        Debug.Log(
            "レッスンクリア: " +
            lessonDataList.lessons[index].title
        );
    }

    private void UpdateButtonColors()
    {
        for (int i = 0; i < lessonButtons.Count; i++)
        {
            if (lessonButtons[i] == null)
                continue;
        

            Image buttonImage = lessonButtons[i].GetComponent<Image>();

            if (buttonImage == null)
                continue;

            if (i == selectedLessonIndex)
            {
                // 選択中：薄い金色
                buttonImage.color = new Color32(255, 225, 160, 255);
            }
            else
            {
                // 未選択：白色
                buttonImage.color = Color.white;
            }
        }
    }
}
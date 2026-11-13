
using System;
using UnityEngine;

[Serializable]
public class LessonData
{
    public int id;
    public string title;
    public string description;

    public string question;
    public string answerA;
    public string answerB;
    public string answerC;

    public string correctAnswer;
    public string modelAnswer;
}

[Serializable]
public class LessonDataList
{
    public LessonData[] lessons;
}

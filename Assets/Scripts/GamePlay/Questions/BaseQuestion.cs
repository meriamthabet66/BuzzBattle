using UnityEngine;

[CreateAssetMenu(fileName = "BaseQuestion", menuName = "Scriptable Objects/BaseQuestion")]

//this class is the parent class of all questions types, to avoid repetition and encourage weak coupling 
public abstract class BaseQuestion : ScriptableObject
{
    [Header("Common Data")]
    public long id;
    public string questionText;
    public AudioClip voiceClip;
    public string voiceClipUrl;
    public int difficulty;
   

    public abstract QuestionType GetQuestionType();
}

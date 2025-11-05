using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "SentenceData", menuName = "DataBase/CreateSentence")]
public class NovelSentenceData : ScriptableObject
{
    [TextArea]
    public string sentence;
}

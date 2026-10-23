using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTopic", menuName = "StudyApp/Topic Data")]
public class TopicSO : ScriptableObject
{
    public string topicName;
    
    [TextArea(3, 6)]
    public List<string> introDialogue;
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnit", menuName = "StudyApp/Unit Data")]
public class UnitSO : ScriptableObject
{
    public string unitTitle;
    public List<TopicSO> topics;
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewClass", menuName = "StudyApp/Class Data")]
public class ClassSO : ScriptableObject
{
    public string className;
    public SubjectType subject;
    public TutorData assignedTutor;

    public List<UnitSO> units;
}
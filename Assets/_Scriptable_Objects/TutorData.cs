using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTutor", menuName = "StudyApp/Tutor Data")]
public class TutorData : ScriptableObject
{

    [Header("Basic Info")]
    public string tutorName;

    [TextArea(2, 4)]
    public string bio;
    public Color nameplateColor;

    [Header("Portraits & Expressions")]
    public Sprite defaultSprite;
    public Sprite happySprite;
    public Sprite thinkingSprite;
    public Sprite wrongAnswerSprite;

    [Header("UI Extras")]
    public Sprite avatarIcon; // Small square avatar for UI/menus
    public AudioClip greetingSound; // Optional voice cue or blip on dialogue start

    [Header("Subjects Taught")]
    public List<SubjectType> subjectsTaught;

    [Header("Taught Classes")]
    public List<ClassSO> taughtClasses;
}

// Subject categories enum for easy filtering
public enum SubjectType
{
    English,
    SocialStudies,
    History,
    Math,
    Science,
    ComputerScience
}

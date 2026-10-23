using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorSelectMenu : MonoBehaviour
{
    [System.Serializable]
    public class TutorCardData
    {
        public TutorData tutorData;
        public Sprite cardArt;         // The illustration image (from your 5 images)
        public ClassSO assignedClass;  // Linked Class (e.g. POL 101, ENG 101)
    }

    [Header("Tutor List (5 Quintuplets)")]
    public List<TutorCardData> tutorCards;

    [Header("UI Slots")]
    public Transform cardContainer;     // Object with HorizontalLayoutGroup
    public GameObject tutorCardPrefab;  // Card button prefab
    
    [Header("Selected Info Display")]
    public TextMeshProUGUI selectedTutorNameText;
    public TextMeshProUGUI selectedSubjectText;
    public Button startStudyButton;

    private TutorCardData currentSelection;

    private void Start()
    {
        GenerateCards();
    }

    private void GenerateCards()
    {
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        foreach (var card in tutorCards)
        {
            GameObject newCard = Instantiate(tutorCardPrefab, cardContainer);
            
            // Set Image
            Image cardImage = newCard.GetComponent<Image>();
            if (cardImage != null && card.cardArt != null)
            {
                cardImage.sprite = card.cardArt;
            }

            // Set Label
            TextMeshProUGUI label = newCard.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null && card.tutorData != null)
            {
                label.text = card.tutorData.tutorName;
            }

            // Button Click Listener
            Button btn = newCard.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => SelectTutor(card));
            }
        }
    }

    public void SelectTutor(TutorCardData selected)
    {
        currentSelection = selected;

        if (selectedTutorNameText != null)
            selectedTutorNameText.text = selected.tutorData.tutorName;

        if (selectedSubjectText != null && selected.assignedClass != null)
            selectedSubjectText.text = $"{selected.assignedClass.className}";

        if (startStudyButton != null)
            startStudyButton.interactable = true;

        // Pass selection to the session holder
        StudySessionData.ActiveClass = selected.assignedClass;
    }
}
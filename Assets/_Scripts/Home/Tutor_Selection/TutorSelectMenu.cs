using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TutorSelectMenu : MonoBehaviour
{
    [Header("Tutor List (5 Quintuplets)")]
    public List<TutorData> tutorDataList;

    [Header("Card Art Mapping (Optional if stored in TutorData)")]
    public List<Sprite> cardArtList;

    [Header("UI Slots & Containers")]
    public Transform cardContainer;
    public GameObject tutorCardPrefab;
    public RectTransform selectedCharAtLeftTransform;

    [Header("Class List UI")]
    public Transform classListContainer;
    public GameObject classButtonPrefab;

    [Header("Action Buttons")]
    public Button unselectButton;
    public TextMeshProUGUI selectedTutorNameText;
    public TextMeshProUGUI selectedSubjectText;
    public Button startStudyButton;

    // Helper structure to remember each spawned card's original layout order
    private struct CardInfo
    {
        public GameObject cardObject;
        public int originalIndex;
    }

    private Dictionary<TutorData, CardInfo> spawnedCards = new Dictionary<TutorData, CardInfo>();
    private List<GameObject> spawnedClassButtons = new List<GameObject>();
    private TutorData currentSelection;

    private void Start()
    {
        GenerateCards();

        if (unselectButton != null)
        {
            unselectButton.gameObject.SetActive(false);
            unselectButton.onClick.AddListener(UnselectTutor);
        }

        if (classListContainer != null)
            classListContainer.gameObject.SetActive(false);
    }

    private void GenerateCards()
    {
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        spawnedCards.Clear();

        for (int i = 0; i < tutorDataList.Count; i++)
        {
            TutorData tutor = tutorDataList[i];
            GameObject newCard = Instantiate(tutorCardPrefab, cardContainer);

            Image cardImage = newCard.GetComponent<Image>();
            if (cardImage != null)
            {
                Sprite art = (cardArtList != null && i < cardArtList.Count && cardArtList[i] != null)
                             ? cardArtList[i]
                             : tutor.defaultSprite;
                cardImage.sprite = art;
            }

            TextMeshProUGUI label = newCard.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = tutor.tutorName;

            Button btn = newCard.GetComponent<Button>();
            if (btn != null)
            {
                TutorData capturedTutor = tutor;
                btn.onClick.AddListener(() => SelectTutor(capturedTutor, newCard));
            }

            // Store the card object and its initial sibling index
            spawnedCards.Add(tutor, new CardInfo { cardObject = newCard, originalIndex = i });
        }
    }

    public void SelectTutor(TutorData selected, GameObject cardObj)
    {
        currentSelection = selected;

        // 1. Disable layout group on container so we can freely move/reparent the selected card
        HorizontalLayoutGroup layoutGroup = cardContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null) layoutGroup.enabled = false;

        // 2. Hide or deactivate other cards, move selected card to the left
        foreach (var kvp in spawnedCards)
        {
            if (kvp.Key == selected)
            {
                RectTransform rt = kvp.Value.cardObject.GetComponent<RectTransform>();
                if (selectedCharAtLeftTransform != null)
                {
                    rt.SetParent(selectedCharAtLeftTransform);
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = selectedCharAtLeftTransform.localScale;
                }
                else
                {
                    rt.anchoredPosition = new Vector2(-400, 0);
                }

                var cardUI = kvp.Value.cardObject.GetComponent<TutorCardUI>();
                if (cardUI != null) cardUI.enabled = false;
            }
            else
            {
                kvp.Value.cardObject.SetActive(false);
            }
        }

        // 3. Update top info display
        if (selectedTutorNameText != null)
            selectedTutorNameText.text = selected.tutorName;

        // 4. Populate and show the list of classes directly from the TutorData SO
        ShowTutorClasses(selected);

        // 5. Enable Unselect Button
        if (unselectButton != null)
            unselectButton.gameObject.SetActive(true);
    }

    private void ShowTutorClasses(TutorData tutorData)
    {
        if (classListContainer == null || classButtonPrefab == null) return;

        classListContainer.gameObject.SetActive(true);

        foreach (Transform child in classListContainer)
            Destroy(child.gameObject);
        spawnedClassButtons.Clear();

        foreach (var classObj in tutorData.taughtClasses)
        {
            GameObject classBtnObj = Instantiate(classButtonPrefab, classListContainer);
            TextMeshProUGUI btnText = classBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = classObj.className;

            Button btn = classBtnObj.GetComponent<Button>();
            if (btn != null)
            {
                ClassSO capturedClass = classObj;
                btn.onClick.AddListener(() => OnClassSelected(capturedClass));
            }

            spawnedClassButtons.Add(classBtnObj);
        }
    }

    private void OnClassSelected(ClassSO selectedClass)
    {
        StudySessionData.ActiveClass = selectedClass;

        if (selectedSubjectText != null)
            selectedSubjectText.text = selectedClass.className;

        if (startStudyButton != null)
            startStudyButton.interactable = true;
    }

    public void UnselectTutor()
    {
        currentSelection = null;

        // Re-enable layout group
        HorizontalLayoutGroup layoutGroup = cardContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null) layoutGroup.enabled = true;

        // Restore all cards back to the container and re-sort them to their original indexes
        foreach (var kvp in spawnedCards)
        {
            GameObject cardObj = kvp.Value.cardObject;
            cardObj.SetActive(true);

            RectTransform rt = cardObj.GetComponent<RectTransform>();
            rt.SetParent(cardContainer);
            rt.localScale = Vector3.one; // Fixes stuck hover scaling sizes
            rt.SetSiblingIndex(kvp.Value.originalIndex); // Restores original order in grid

            var cardUI = cardObj.GetComponent<TutorCardUI>();
            if (cardUI != null)
            {
                cardUI.enabled = true;
                cardUI.ResetWidth();
            }
        }

        // Clear and hide class list
        foreach (Transform child in classListContainer)
            Destroy(child.gameObject);

        if (classListContainer != null)
            classListContainer.gameObject.SetActive(false);

        // Hide unselect button & reset study button
        if (unselectButton != null)
            unselectButton.gameObject.SetActive(false);

        if (selectedTutorNameText != null) selectedTutorNameText.text = "";
        if (selectedSubjectText != null) selectedSubjectText.text = "";
        if (startStudyButton != null) startStudyButton.interactable = false;
    }
}
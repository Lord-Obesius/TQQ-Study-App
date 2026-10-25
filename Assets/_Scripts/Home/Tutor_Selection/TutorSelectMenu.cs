using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Animation Settings")]
    public float moveAnimDuration = 0.35f;
    public float fadeAnimDuration = 0.25f;
    public float cascadeDelay = 0.05f;

    private struct CardInfo
    {
        public GameObject cardObject;
        public int originalIndex;
    }

    private Dictionary<TutorData, CardInfo> spawnedCards = new Dictionary<TutorData, CardInfo>();
    private List<GameObject> spawnedClassButtons = new List<GameObject>();
    private TutorData currentSelection;
    private Coroutine activeAnimationRoutine;

    private void Start()
    {
        GenerateCards();

        if (unselectButton != null)
        {
            unselectButton.gameObject.SetActive(false);
            unselectButton.onClick.AddListener(UnselectTutor);
        }

        if (classListContainer != null)
        {
            classListContainer.gameObject.SetActive(false);
            SetContainerAlpha(classListContainer, 0f);
        }
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
                btn.onClick.AddListener(() => SelectTutor(capturedTutor));
            }

            spawnedCards.Add(tutor, new CardInfo { cardObject = newCard, originalIndex = i });
        }
    }

    public void SelectTutor(TutorData selected)
    {
        if (currentSelection != null) return;
        currentSelection = selected;

        if (activeAnimationRoutine != null) StopCoroutine(activeAnimationRoutine);
        activeAnimationRoutine = StartCoroutine(AnimateSelectTutorRoutine(selected));
    }

    private IEnumerator AnimateSelectTutorRoutine(TutorData selected)
    {
        Dictionary<GameObject, Vector3> initialCardPositions = new Dictionary<GameObject, Vector3>();
        foreach (var kvp in spawnedCards)
        {
            if (kvp.Key != selected)
            {
                initialCardPositions[kvp.Value.cardObject] = kvp.Value.cardObject.transform.position;
            }
        }

        HorizontalLayoutGroup layoutGroup = cardContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null) layoutGroup.enabled = false;

        GameObject selectedObj = spawnedCards[selected].cardObject;
        RectTransform selectedRT = selectedObj.GetComponent<RectTransform>();

        var selectedCardUI = selectedObj.GetComponent<TutorCardUI>();
        if (selectedCardUI != null)
        {
            // Expand card to hovered size before locking position/disabling script
            selectedCardUI.OnPointerEnter(null);
            selectedCardUI.enabled = false;
        }

        Vector3 startWorldPos = selectedRT.position;
        Vector3 fixedScale = selectedRT.localScale;

        if (selectedTutorNameText != null)
            selectedTutorNameText.text = selected.tutorName;

        float elapsedTime = 0f;
        while (elapsedTime < moveAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveAnimDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (selectedCharAtLeftTransform != null)
            {
                selectedRT.position = Vector3.Lerp(startWorldPos, selectedCharAtLeftTransform.position, smoothT);
                selectedRT.localScale = fixedScale;
            }

            foreach (var kvp in spawnedCards)
            {
                if (kvp.Key != selected)
                {
                    kvp.Value.cardObject.transform.position = initialCardPositions[kvp.Value.cardObject];

                    var cardUI = kvp.Value.cardObject.GetComponent<TutorCardUI>();
                    if (cardUI != null)
                    {
                        float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeAnimDuration);
                        cardUI.SetAlpha(alpha);
                    }
                }
            }

            yield return null;
        }

        if (selectedCharAtLeftTransform != null)
        {
            selectedRT.SetParent(selectedCharAtLeftTransform);
            selectedRT.anchoredPosition = Vector2.zero;
            selectedRT.localScale = fixedScale;
        }

        foreach (var kvp in spawnedCards)
        {
            if (kvp.Key != selected)
            {
                kvp.Value.cardObject.SetActive(false);
            }
        }

        yield return StartCoroutine(ShowTutorClassesCascaded(selected));

        if (unselectButton != null)
            unselectButton.gameObject.SetActive(true);
    }

    private IEnumerator ShowTutorClassesCascaded(TutorData tutorData)
    {
        if (classListContainer == null || classButtonPrefab == null) yield break;

        LayoutGroup classLayout = classListContainer.GetComponent<LayoutGroup>();
        if (classLayout != null) classLayout.enabled = false;

        classListContainer.gameObject.SetActive(true);

        foreach (Transform child in classListContainer)
            Destroy(child.gameObject);
        spawnedClassButtons.Clear();

        List<CanvasGroup> buttonCanvasGroups = new List<CanvasGroup>();
        foreach (var classObj in tutorData.taughtClasses)
        {
            GameObject classBtnObj = Instantiate(classButtonPrefab, classListContainer);

            CanvasGroup cg = classBtnObj.GetComponent<CanvasGroup>();
            if (cg == null) cg = classBtnObj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            buttonCanvasGroups.Add(cg);

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

        if (classLayout != null) classLayout.enabled = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(classListContainer as RectTransform);

        SetContainerAlpha(classListContainer, 1f);

        for (int i = 0; i < buttonCanvasGroups.Count; i++)
        {
            StartCoroutine(FadeInButtonRoutine(buttonCanvasGroups[i]));
            yield return new WaitForSeconds(cascadeDelay);
        }
    }

    private IEnumerator FadeInButtonRoutine(CanvasGroup cg)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        RectTransform rt = cg.GetComponent<RectTransform>();

        Vector2 targetPos = rt.anchoredPosition;
        Vector2 startPos = targetPos + new Vector2(0, 15);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        cg.alpha = 1f;
        rt.anchoredPosition = targetPos;
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
        if (currentSelection == null) return;

        if (activeAnimationRoutine != null) StopCoroutine(activeAnimationRoutine);
        activeAnimationRoutine = StartCoroutine(AnimateUnselectRoutine());
    }

    private IEnumerator AnimateUnselectRoutine()
    {
        TutorData previousSelection = currentSelection;
        currentSelection = null;

        if (classListContainer != null)
            classListContainer.gameObject.SetActive(false);

        GameObject selectedObj = spawnedCards[previousSelection].cardObject;
        RectTransform selectedRT = selectedObj.GetComponent<RectTransform>();
        Vector3 fixedScale = selectedRT.localScale;

        HorizontalLayoutGroup layoutGroup = cardContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null) layoutGroup.enabled = true;

        foreach (var kvp in spawnedCards)
        {
            kvp.Value.cardObject.SetActive(true);
            var cardUI = kvp.Value.cardObject.GetComponent<TutorCardUI>();
            if (cardUI != null)
            {
                cardUI.SetAlpha(0f);
                cardUI.ResetWidth();
            }
        }

        foreach (var kvp in spawnedCards)
        {
            kvp.Value.cardObject.transform.SetSiblingIndex(kvp.Value.originalIndex);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer as RectTransform);

        Dictionary<GameObject, Vector3> finalCardPositions = new Dictionary<GameObject, Vector3>();
        foreach (var kvp in spawnedCards)
        {
            if (kvp.Key != previousSelection)
            {
                finalCardPositions[kvp.Value.cardObject] = kvp.Value.cardObject.transform.position;
            }
        }

        Vector3 startPos = selectedRT.position;
        selectedRT.SetParent(cardContainer);
        selectedRT.SetSiblingIndex(spawnedCards[previousSelection].originalIndex);

        LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer as RectTransform);
        Vector3 endWorldPos = selectedRT.position;

        if (layoutGroup != null) layoutGroup.enabled = false;
        selectedRT.position = startPos;

        float elapsedTime = 0f;
        while (elapsedTime < moveAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveAnimDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            selectedRT.position = Vector3.Lerp(startPos, endWorldPos, smoothT);
            selectedRT.localScale = fixedScale;

            foreach (var kvp in spawnedCards)
            {
                if (kvp.Key != previousSelection)
                {
                    kvp.Value.cardObject.transform.position = finalCardPositions[kvp.Value.cardObject];
                }
            }

            yield return null;
        }

        if (layoutGroup != null) layoutGroup.enabled = true;
        selectedRT.localScale = fixedScale;
        selectedRT.SetSiblingIndex(spawnedCards[previousSelection].originalIndex);
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer as RectTransform);

        float fadeElapsed = 0f;
        while (fadeElapsed < fadeAnimDuration)
        {
            fadeElapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeElapsed / fadeAnimDuration);

            foreach (var kvp in spawnedCards)
            {
                var cardUI = kvp.Value.cardObject.GetComponent<TutorCardUI>();
                if (cardUI != null)
                {
                    cardUI.SetAlpha(alpha);
                }
            }

            yield return null;
        }

        foreach (var kvp in spawnedCards)
        {
            GameObject cardObj = kvp.Value.cardObject;
            var cardUI = cardObj.GetComponent<TutorCardUI>();
            if (cardUI != null)
            {
                cardUI.SetAlpha(1f);
                cardUI.enabled = true;
                cardUI.ResetWidth(); // Resets back to normal width when back in grid
            }
        }

        foreach (Transform child in classListContainer)
            Destroy(child.gameObject);

        if (unselectButton != null)
            unselectButton.gameObject.SetActive(false);

        if (selectedTutorNameText != null) selectedTutorNameText.text = "";
        if (selectedSubjectText != null) selectedSubjectText.text = "";
        if (startStudyButton != null) startStudyButton.interactable = false;
    }

    private void SetContainerAlpha(Transform container, float alpha)
    {
        CanvasGroup cg = container.GetComponent<CanvasGroup>();
        if (cg == null) cg = container.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = alpha;
    }
}
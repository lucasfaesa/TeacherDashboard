using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _TeacherDashboard.Scripts;
using API_Mestrado_Lucas;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SubjectAndThemesAndQuizDisplay : MonoBehaviour
{
    [Header("Canvas Update")] [SerializeField]
    private VerticalLayoutGroup verticalLayoutGroup;

    [Space] [SerializeField] private TextMeshProUGUI subjectOrQuizNameText;

    [Space] [SerializeField] private SubjectThemeAndQuizListContentDisplay subjectThemeAndQuizContDisplayPrefab;

    [SerializeField] private List<SubjectThemeAndQuizListContentDisplay> subjectThemeAndQuizListContDispPool;

    [Space] [SerializeField] private GameObject subjectThemesContent;

    private bool _isSubjectThemesShown;

    private LevelsListDisplay _levelsListDisplay;

    private LevelsListDisplay.ListCategory _listCategory;


    public void SetInfos(SubjectDTO subjectData, LevelsListDisplay levelsListDisplay, VerticalLayoutGroup vertLayout,
        LevelsListDisplay.ListCategory category)
    {
        verticalLayoutGroup = vertLayout;
        subjectOrQuizNameText.text = subjectData.Name;
        _levelsListDisplay = levelsListDisplay;
        _listCategory = category;

        GenerateSubjectThemeList(subjectData);
    }
    
    public void SetInfos(List<QuizDTO> quizes, LevelsListDisplay levelsListDisplay, VerticalLayoutGroup vertLayout,
        LevelsListDisplay.ListCategory category)
    {
        verticalLayoutGroup = vertLayout;
        subjectOrQuizNameText.text = "Quizes";
        _levelsListDisplay = levelsListDisplay;
        _listCategory = category;

        GenerateQuizesList(quizes);
    }

    private void GenerateSubjectThemeList(SubjectDTO subjectData)
    {
        foreach (var listObj in subjectThemeAndQuizListContDispPool) listObj.gameObject.SetActive(false);

        if (subjectThemeAndQuizListContDispPool.Count < subjectData.SubjectThemes.Count)
            InstantiateMoreSubjectThemeObjects(subjectData.SubjectThemes.Count - subjectThemeAndQuizListContDispPool.Count);

        var subjectsThemes = subjectData.SubjectThemes.ToList();

        for (var i = 0; i < subjectsThemes.Count; i++)
        {
            var subjectTheme = subjectsThemes[i];
            subjectThemeAndQuizListContDispPool[i].SetInfos(subjectTheme, this, verticalLayoutGroup, _listCategory);
            subjectThemeAndQuizListContDispPool[i].gameObject.SetActive(true);
        }
    }
    
    private void GenerateQuizesList(List<QuizDTO> quizes)
    {
        foreach (var listObj in subjectThemeAndQuizListContDispPool) listObj.gameObject.SetActive(false);

        if (subjectThemeAndQuizListContDispPool.Count < quizes.Count)
            InstantiateMoreSubjectThemeObjects(quizes.Count - subjectThemeAndQuizListContDispPool.Count);

        for (var i = 0; i < quizes.Count; i++)
        {
            subjectThemeAndQuizListContDispPool[i].SetInfos(quizes[i], this, verticalLayoutGroup, _listCategory);
            subjectThemeAndQuizListContDispPool[i].gameObject.SetActive(true);
        }
    }

    private void InstantiateMoreSubjectThemeObjects(int quantity)
    {
        for (var i = 0; i < quantity; i++)
        {
            var newObject = Instantiate(subjectThemeAndQuizContDisplayPrefab, subjectThemesContent.transform);

            subjectThemeAndQuizListContDispPool.Add(newObject);
        }
    }

    public void ToggleSubjectThemes()
    {
        _isSubjectThemesShown = !_isSubjectThemesShown;

        if (_isSubjectThemesShown)
            subjectThemesContent.SetActive(true);
        else
            subjectThemesContent.SetActive(false);

        StartCoroutine(UpdateCanvas());
    }

    private IEnumerator UpdateCanvas()
    {
        Canvas.ForceUpdateCanvases();
        verticalLayoutGroup.enabled = false;
        yield return new WaitForEndOfFrame();

        Debug.Log("Updating canvas");
        Canvas.ForceUpdateCanvases();
        verticalLayoutGroup.enabled = true;
        Canvas.ForceUpdateCanvases();
    }

    public void AddOrRemoveSubjectThemeOrQuiz(LevelsListDisplay.ListCategory category, SubjectThemeDTO subjectTheme, QuizDTO quizDto)
    {
        _levelsListDisplay.AddOrRemoveSubjectThemeOrQuiz(category, subjectTheme, quizDto);
    }
}
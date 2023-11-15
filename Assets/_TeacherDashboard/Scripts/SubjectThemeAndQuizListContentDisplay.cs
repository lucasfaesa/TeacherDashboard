using System.Collections;
using _TeacherDashboard.Scripts;
using API_Mestrado_Lucas;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SubjectThemeAndQuizListContentDisplay : MonoBehaviour
{
    [Header("Canvas Update")] [SerializeField]
    private VerticalLayoutGroup verticalLayoutGroup;

    [Space] [SerializeField] private TextMeshProUGUI titleTxt;

    [SerializeField] private TextMeshProUGUI buttonText;

    [Space] [SerializeField] private GameObject descriptionContent;

    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private GameObject arrow;

    private bool _isDescriptionShown;

    private LevelsListDisplay.ListCategory _listCategory;
    private SubjectAndThemesAndQuizDisplay subjectAndThemesAndQuizDisplay;
    private SubjectThemeDTO _subjectThemeDto;
    private QuizDTO _quizDto;


    public void SetInfos(SubjectThemeDTO subjectThemeDto, SubjectAndThemesAndQuizDisplay subjThemDispl,
        VerticalLayoutGroup vertLayout, LevelsListDisplay.ListCategory category)
    {
        _subjectThemeDto = subjectThemeDto;
        _quizDto = null;
        verticalLayoutGroup = vertLayout;
        arrow.SetActive(true);
        titleTxt.text = subjectThemeDto.Name;
        descriptionText.text = subjectThemeDto.Description;
        subjectAndThemesAndQuizDisplay = subjThemDispl;
        _listCategory = category;
        buttonText.text = category == LevelsListDisplay.ListCategory.AllLevels ? "Adicionar" : "Remover";
    }
    
    public void SetInfos(QuizDTO quiz, SubjectAndThemesAndQuizDisplay subjThemDispl,
        VerticalLayoutGroup vertLayout, LevelsListDisplay.ListCategory category)
    {
        _quizDto = quiz;
        _subjectThemeDto = null;
        verticalLayoutGroup = vertLayout;
        titleTxt.text = quiz.Name;
        arrow.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        subjectAndThemesAndQuizDisplay = subjThemDispl;
        _listCategory = category;
        buttonText.text = category == LevelsListDisplay.ListCategory.AllLevels ? "Adicionar" : "Remover";
    }

    public void ToggleDescription()
    {
        if (_quizDto != null) return; //quizes não tem descrição
        
        _isDescriptionShown = !_isDescriptionShown;

        if (_isDescriptionShown)
            descriptionContent.SetActive(true);
        else
            descriptionContent.SetActive(false);

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

    public void AddOrRemoveSubjectThemeOrQuiz()
    {
        subjectAndThemesAndQuizDisplay.AddOrRemoveSubjectThemeOrQuiz(_listCategory, _subjectThemeDto, _quizDto);
    }
}
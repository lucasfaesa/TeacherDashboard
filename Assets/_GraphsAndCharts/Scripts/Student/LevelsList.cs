using System.Collections.Generic;
using System.Linq;
using _GraphsAndCharts.Scripts.General;
using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.Student
{
    public class LevelsList : MonoBehaviour
    {
        [SerializeField] private LevelsDataSO levelsDataSo;
        [SerializeField] private StudentsSessionsDataSO studentsSessionsData;

        [Space] [SerializeField] private Transform content;

        [SerializeField] private LevelNameAndNumberDisplay textPrefab;
        [SerializeField] private List<LevelNameAndNumberDisplay> pooledTexts;

        [Space] [SerializeField] private List<GameObject> subjectOrQuizDivPool;

        private void OnEnable()
        {
            studentsSessionsData.GotLevelsOfGroupClass += FillTexts;
            studentsSessionsData.ClassInsufficientData += InsufficientData;
        }

        private void OnDisable()
        {
            studentsSessionsData.GotLevelsOfGroupClass -= FillTexts;
            studentsSessionsData.ClassInsufficientData -= InsufficientData;
        }

        private void InsufficientData()
        {
            NoData();
        }

        private void FillTexts()
        {
            pooledTexts.ForEach(x=>x.gameObject.SetActive(false));
            subjectOrQuizDivPool.ForEach(x=>x.gameObject.SetActive(false));
            
            if (pooledTexts.Count < studentsSessionsData.TotalLevelsAndNumbers.Count + levelsDataSo.AllQuizesOfThisTeacher.Count)
                InstantiateTexts(studentsSessionsData.TotalLevelsAndNumbers.Count + levelsDataSo.AllQuizesOfThisTeacher.Count - pooledTexts.Count);

            var count = 1;

            var previousSubjectName = "";

            for (var i = 0; i < studentsSessionsData.TotalLevelsAndNumbers.Count; i++)
            {
                var subjectThemeName = studentsSessionsData.TotalLevelsAndNumbers[i].LevelDto.SubjectTheme
                    .Subject.Name;

                if (previousSubjectName != subjectThemeName)
                {
                    previousSubjectName = subjectThemeName;
                    var subjectDiv = subjectOrQuizDivPool.First(x => !x.activeSelf);
                    subjectDiv.SetActive(true);
                    subjectDiv.GetComponentInChildren<TextMeshProUGUI>().text = subjectThemeName;

                    if (!pooledTexts.Any(x => x.gameObject.activeSelf))
                    {
                        subjectDiv.transform.SetAsFirstSibling();
                    }
                    else
                    {
                        var text = pooledTexts.First(x => !x.gameObject.activeSelf);

                        if (text == null)
                            subjectDiv.transform.SetAsLastSibling();
                        else
                            subjectDiv.transform.SetSiblingIndex(pooledTexts.First(x => !x.gameObject.activeSelf)
                                .transform.GetSiblingIndex() - 1);
                    }
                }
                
                pooledTexts[i].SetTexts(count.ToString(),
                    studentsSessionsData.TotalLevelsAndNumbers[i].LevelDto.SubjectTheme.Name +
                    " - Nivel " + studentsSessionsData.TotalLevelsAndNumbers[i].number);

                pooledTexts[i].gameObject.SetActive(true);

                count++;
            }

            
            #region Quizes instantion
            
            var div = subjectOrQuizDivPool.First(x => !x.activeSelf);
            div.SetActive(true);
            div.GetComponentInChildren<TextMeshProUGUI>().text = "Quizes";
            
            for (int i = 0; i < levelsDataSo.AllQuizesOfThisTeacher.Count; i++)
            {
                var quizName = levelsDataSo.AllQuizesOfThisTeacher[i].Name;

                if (!pooledTexts.Any(x => x.gameObject.activeSelf))
                {
                    div.transform.SetAsFirstSibling();
                }
                else
                {
                    var text = pooledTexts.First(x => !x.gameObject.activeSelf);

                    if (text == null)
                        div.transform.SetAsLastSibling();
                    else
                        div.transform.SetSiblingIndex(pooledTexts.First(x => !x.gameObject.activeSelf)
                                .transform.GetSiblingIndex() - 1);
                }


                pooledTexts[count].SetTexts(count.ToString(),
                    $"{quizName[..(quizName.Length > 32 ? 32 : quizName.Length)]}{(quizName.Length > 32 ? "..." : "")}");
                
                pooledTexts[count].gameObject.SetActive(true);

                count++;
            }
            #endregion
        }

        private void InstantiateTexts(int quantity)
        {
            for (var i = 0; i < quantity; i++)
            {
                var text = Instantiate(textPrefab, content);
                text.gameObject.SetActive(false);
                pooledTexts.Add(text);
            }
        }

        private void NoData()
        {
            foreach (var texts in pooledTexts) texts.gameObject.SetActive(false);
        }
    }
}
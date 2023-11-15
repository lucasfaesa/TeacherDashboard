using System.Collections;
using System.Collections.Generic;
using System.Linq;
using API_Mestrado_Lucas;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _TeacherDashboard.Scripts
{
    public class LevelsListDisplay : MonoBehaviour
    {
        public enum ListCategory
        {
            AllLevels,
            AssignedLevels
        }

        [Header("Canvas Update")] [SerializeField]
        private VerticalLayoutGroup verticalLayoutGroup;
        [Space] 
        [SerializeField] private SubjectAndThemesAndQuizDisplay subjectAndThemesAndQuizDisplayPrefab;

        [SerializeField] private List<SubjectAndThemesAndQuizDisplay> subjectAndThemesAndQuizDispPool;

        [Space] [SerializeField] private GameObject subjectsContent;

        [Space] [SerializeField] private ListCategory listCategory;

        private AssignLevelsWindowController _assignLevelsWindowController;

        private void OnEnable()
        {
            foreach (var listObject in subjectAndThemesAndQuizDispPool) listObject.gameObject.SetActive(false);
        }

        public void GenerateSubjectList(List<SubjectThemeDTO> subjectThemes, List<QuizDTO> quizes,
            AssignLevelsWindowController assignLevelsWindowController)
        {
            
            foreach (var listObject in subjectAndThemesAndQuizDispPool) listObject.gameObject.SetActive(false);

            _assignLevelsWindowController = assignLevelsWindowController;

            if (subjectThemes.Count == 0 && quizes.Count == 0) return;

            #region Converting Back To Subject

            var groupedSubjectsThemesBySubject = subjectThemes.GroupBy(x => x.SubjectId);

            List<SubjectDTO> subjects = new();

            foreach (var groupOfSubjectThemes in groupedSubjectsThemesBySubject)
            {
                var newSubject = new SubjectDTO();
                List<SubjectThemeDTO> themes = new();

                foreach (var subjectThemeDto in groupOfSubjectThemes)
                {
                    newSubject.Id = subjectThemeDto.Subject.Id;
                    newSubject.Name = subjectThemeDto.Subject.Name;
                    themes.Add(subjectThemeDto);
                }

                newSubject.SubjectThemes = themes;

                subjects.Add(newSubject);
            }

            #endregion

            if (subjectAndThemesAndQuizDispPool.Count < subjects.Count + 1 /*quiz*/)
                InstantiateMoreListObjects(subjects.Count - subjectAndThemesAndQuizDispPool.Count);

            var count = 0;
            for (var i = 0; i < subjects.Count; i++)
            {
                var subject = subjects[i];
                subjectAndThemesAndQuizDispPool[i].SetInfos(subject, this, verticalLayoutGroup, listCategory);
                subjectAndThemesAndQuizDispPool[i].gameObject.SetActive(true);
                count++;
            }
            
            //quiz
            if (quizes.Count > 0)
            {
                subjectAndThemesAndQuizDispPool[count].SetInfos(quizes, this, verticalLayoutGroup, listCategory);
                subjectAndThemesAndQuizDispPool[count].gameObject.SetActive(true);    
            }

            StartCoroutine(UpdateCanvas());
        }

        private void InstantiateMoreListObjects(int quantity)
        {
            for (var i = 0; i < quantity; i++)
            {
                var newObject = Instantiate(subjectAndThemesAndQuizDisplayPrefab, subjectsContent.transform);

                subjectAndThemesAndQuizDispPool.Add(newObject);
            }
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


        public void AddOrRemoveSubjectThemeOrQuiz(ListCategory category, SubjectThemeDTO subjectTheme, QuizDTO quizDto)
        {
            _assignLevelsWindowController.ReGenerateLists(category, subjectTheme, quizDto);
        }
    }
}
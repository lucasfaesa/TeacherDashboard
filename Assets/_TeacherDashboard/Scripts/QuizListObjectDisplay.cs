using System.Collections.Generic;
using API_Mestrado_Lucas;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _TeacherDashboard.Scripts
{
    public class QuizListObjectDisplay : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private Toggle activeToggle;
        [SerializeField] private TextMeshProUGUI nameText;
        public EditQuestionsWindowController editQuestionsWindowController;

        private QuizesTabController quizesTabController;
        
        public QuizDTO Quiz { get; set; }
        
        public void SetInfos(QuizDTO quiz, QuizesTabController quizesTabController)
        {
            toggle.isOn = false;
            nameText.text = quiz.Name;
            activeToggle.SetIsOnWithoutNotify(quiz.Active);

            Quiz = quiz;
            this.quizesTabController = quizesTabController;
        }
        
        public void QuizToggle(bool status)
        {
            if (status)
                quizesTabController.QuizSelected(Quiz);
            else
                quizesTabController.QuizDeselected(Quiz);
        }

        public void ActiveToggle(bool status)
        {
            Quiz.Active = status;
            quizesTabController.ChangeQuizActiveStatus(Quiz);
        }

        public void EditQuestionsButton()
        {
            editQuestionsWindowController.EditQuizQuestions(Quiz);
        }
    }
}
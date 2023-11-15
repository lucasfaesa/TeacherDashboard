using API_Mestrado_Lucas;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _TeacherDashboard.Scripts
{
    public class StudentListObjectDisplay : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;

        [Header("Basic Infos")] [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField] private TextMeshProUGUI classText;
        [SerializeField] private TextMeshProUGUI lastLoginText;

        [Header("Login Infos")] [SerializeField]
        private GameObject loginInfoContent;

        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI creationDateText;


        private bool _loginInfoContentShown;
        private StudentsTabController _studentsTabController;

        private EditStudentWindowController _editStudentWindowController { get; set; }
        public StudentCompleteInfoDTO StudentData { get; set; }

        public void SetInfos(StudentCompleteInfoDTO studentDto, StudentsTabController studentsTabController,
            EditStudentWindowController editStudentWindowController)
        {
            toggle.isOn = false;
            nameText.text = studentDto.Name;
            classText.text = studentDto.GroupClass == null ? "" : studentDto.GroupClass.Name;
            lastLoginText.text = studentDto.LastLoginDate.ToString("dd/MM/yyyy");

            usernameText.text = studentDto.Username;
            creationDateText.text = studentDto.CreationDate.ToString("dd/MM/yy");

            StudentData = studentDto;
            _studentsTabController = studentsTabController;
            _editStudentWindowController = editStudentWindowController;
        }

        public void LoginInfosToggle()
        {
            _loginInfoContentShown = !_loginInfoContentShown;

            if (!_loginInfoContentShown)
                loginInfoContent.SetActive(false);
            else
                loginInfoContent.SetActive(true);

            _studentsTabController.UpdateCanvas();
        }

        public void StudentToggle(bool status)
        {
            if (status)
                _studentsTabController.StudentSelected(StudentData);
            else
                _studentsTabController.StudentDeselected(StudentData);
        }

        public void OpenEditView()
        {
            _editStudentWindowController.ActivateContent(StudentData);
        }
    }
}
using _TeacherDashboard.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace _TeacherLogin.Scripts
{
    public class SceneChoiceBoxController : MonoBehaviour
    {
        [SerializeField] private TeacherDataSO teacherDataSo;

        [Space] [SerializeField] private Button graphsSceneButton;

        private void OnEnable()
        {
            graphsSceneButton.interactable = teacherDataSo.TeacherDto.GroupClasses.Count != 0;
        }
    }
}
using _TeacherDashboard.Scripts;
using TMPro;
using UnityEngine;

public class WelcomeText : MonoBehaviour
{
    [SerializeField] private TeacherDataSO teacherData;
    [SerializeField] private TextMeshProUGUI welcomeTxt;

    private void Start()
    {
        welcomeTxt.text = "Painel de Controle de " + teacherData.TeacherDto.Name;
    }
}
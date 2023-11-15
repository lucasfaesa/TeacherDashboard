using System.Collections.Generic;
using API_Mestrado_Lucas;
using UnityEngine;

namespace _TeacherDashboard.Scripts
{
    [CreateAssetMenu(fileName = "SubjectsData", menuName = "ScriptableObjects/SubjectsData")]
    public class SubjectsDataSO : ScriptableObject
    {
        public List<SubjectDTO> SubjectInfo { get; set; }
    }
}
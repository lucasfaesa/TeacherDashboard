using System;
using System.Collections.Generic;
using System.Linq;
using API_Mestrado_Lucas;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.Student
{
    [CreateAssetMenu(fileName = "StudentsSessionData", menuName = "ScriptableObjects/SessionsData/StudentsSessionData")]
    public class StudentsSessionsDataSO : ScriptableObject
    {
        public List<StudentAndCompleteLevelsSessionsDTO> CompleteStudentsSession { get; set; }

        public StudentAndCompleteLevelsSessionsDTO CurrentSelectedStudentSession { get; set; }

        public List<LevelAndNumber> TotalLevelsAndNumbers { get; set; } = new();
        public List<LevelAndNumber> LevelsAndNumbersOfGroupClass { get; set; } = new();
        
        public void Reset()
        {
            CompleteStudentsSession = null;
        }

        public event Action GotStudentsSessions;
        public event Action GotLevelsOfGroupClass;
        public event Action GotSessionsAndLevels;

        public event Action<StudentAndCompleteLevelsSessionsDTO> StudentSelectedOnDropdown;
        public event Action ClassInsufficientData;

        public void StudentSessionGot()
        {
            GotStudentsSessions?.Invoke();
        }

        public void LevelsOfGroupClassGot()
        {
            GotLevelsOfGroupClass?.Invoke();
        }

        public void GotSessionsAndLevel()
        {
            GotSessionsAndLevels?.Invoke();
        }

        public void StudentSelected(StudentDTO dto)
        {
            CurrentSelectedStudentSession = CompleteStudentsSession.Find(x => x.StudentDto.Id == dto.Id);
            StudentSelectedOnDropdown?.Invoke(CurrentSelectedStudentSession);
        }

        public void InsufficientData()
        {
            ClassInsufficientData?.Invoke();
        }

        public void SetLevelsOfGroupClass(List<LevelDTO> levels)
        {
            if (levels.Count > 0)
            {
                var groupedLevels = levels.GroupBy(x => x.SubjectTheme);

                List<LevelAndNumber>
                    levelsAndRespectiveNumbers =
                        new(); //numbers = difficulty, tipo no jogo onde uma fase tem fase 1, 2, 3... isso que estou calculando por tematica
                var count = 1;
                foreach (var groupedLevel in groupedLevels)
                {
                    count = 1;
                    foreach (var levelDto in groupedLevel)
                    {
                        levelsAndRespectiveNumbers.Add(new LevelAndNumber(levelDto, count));
                        count++;
                    }
                }

                LevelsAndNumbersOfGroupClass = levelsAndRespectiveNumbers;
            }
        }
        
        public void SetAllLevels(List<LevelDTO> levels)
        {
            if (levels.Count > 0)
            {
                var groupedLevels = levels.GroupBy(x => x.SubjectThemeId);

                List<LevelAndNumber>
                    levelsAndRespectiveNumbers =
                        new(); //numbers = difficulty, tipo no jogo onde uma fase tem fase 1, 2, 3... isso que estou calculando por tematica
                var count = 1;
                foreach (var groupedLevel in groupedLevels)
                {
                    count = 1;
                    foreach (var levelDto in groupedLevel)
                    {
                        levelsAndRespectiveNumbers.Add(new LevelAndNumber(levelDto, count));
                        count++;
                    }
                }

                TotalLevelsAndNumbers = levelsAndRespectiveNumbers;
            }
            LevelsOfGroupClassGot();
            GotSessionsAndLevel();
        }
        
        
    }

    public class LevelAndNumber
    {
        public LevelDTO LevelDto;
        public int number;

        public LevelAndNumber(LevelDTO level, int numb)
        {
            LevelDto = level;
            number = numb;
        }
    }
}
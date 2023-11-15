using System.Collections.Generic;
using System.Linq;
using API_Mestrado_Lucas;
using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.General
{
    public class RightSideInfosController : MonoBehaviour
    {
        [SerializeField] private GroupClassSessionsDataSO groupClassSessionsData;
        [SerializeField] private LevelsDataSO levelsData;
        [Space] [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI studentsText;
        [SerializeField] private TextMeshProUGUI levelsMostFails;
        [SerializeField] private TextMeshProUGUI levelMostPlayed;


        private void OnEnable()
        {
            groupClassSessionsData.OnSessionsAndLevelsGot += Fill;
        }

        private void OnDisable()
        {
            groupClassSessionsData.OnSessionsAndLevelsGot -= Fill;
        }

        private void Fill()
        {
            ResetTexts();
            FillTexts();
        }

        private void FillTexts()
        {
        
            if (groupClassSessionsData.CompleteGroupClassSession.Count == 0)
            {
                NoData();
                return;
            }

            dateText.text = groupClassSessionsData.CompleteGroupClassSession.OrderByDescending(x => x.LastPlayedDate)
                .FirstOrDefault()
                .LastPlayedDate.ToString("dd/MM/yyyy");

            studentsText.text = groupClassSessionsData.CompleteGroupClassSession.Count.ToString();

            var levelMostFailList = groupClassSessionsData.CompleteGroupClassSession
                .Where(x => x.LevelMaxFails != null).ToList();

            var levelMostPlayedList = groupClassSessionsData.CompleteGroupClassSession
                .Where(x => x.LevelMaxPlays != null).ToList();

            CompleteStudentSessionDTO levelMostFailed = null;
            CompleteStudentSessionDTO levelPlayedMost = null;

            if (levelMostFailList.Count > 0)
                levelMostFailed = GetMostFailedLevel(levelMostFailList);

            if (levelMostPlayedList.Count > 0)
                levelPlayedMost = GetMostPlayedLevel(levelMostPlayedList);
            

            var groupedLevels = levelsData.LevelsOfThisGroupClass.GroupBy(x => x.SubjectTheme);

            var doneFails = false;
            var doneMostPlayed = false;

            var levelMostFailNumber = 1;
            var levelMostPlayedNumber = 1;

            if (levelMostFailed == null)
            {
                levelsMostFails.text = "Sem dados suficientes";
            }
            else
            {
                if (levelMostFailed.LevelMaxFails.LevelId != null)
                {
                    foreach (var groupedLevel in groupedLevels)
                    {
                        levelMostFailNumber = 1;
                        foreach (var levelDto in groupedLevel)
                        {
                            if (levelDto.Id == levelMostFailed.LevelMaxFails.LevelId)
                            {
                                doneFails = true;
                                break;
                            }

                            levelMostFailNumber++;
                        }

                        if (doneFails) break;
                    }

                    levelsMostFails.text =
                        levelsData.AllLevelsOfGame.FirstOrDefault(x => x.Id == levelMostFailed.LevelMaxFails.LevelId)?.SubjectTheme.Name +
                        " - Nível " + levelMostFailNumber;
                }
                else
                {
                    var quizName = levelMostFailed.LevelMaxFails.Quiz.Name;
                    levelsMostFails.text = $"Quiz: {quizName[..(quizName.Length > 39 ? 39 : quizName.Length)]}{(quizName.Length > 39 ? "..." : "")}";
                }
                
            }

            if (levelPlayedMost == null)
            {
                levelMostPlayed.text = "Sem dados suficientes";
            }
            else
            {
                if (levelPlayedMost.LevelMaxPlays.LevelId != null)
                {
                    foreach (var groupedLevel in groupedLevels)
                    {
                        levelMostPlayedNumber = 1;
                        foreach (var levelDto in groupedLevel)
                        {
                            if (levelDto.Id == levelPlayedMost.LevelMaxPlays.LevelId)
                            {
                                doneMostPlayed = true;
                                break;
                            }

                            levelMostPlayedNumber++;
                        }

                        if (doneMostPlayed) break;
                    }

                    levelMostPlayed.text =
                        levelsData.AllLevelsOfGame.FirstOrDefault(x => x.Id == levelPlayedMost.LevelMaxPlays.LevelId)?.SubjectTheme.Name +
                        " - Nível " + levelMostPlayedNumber;
                }
                else
                {
                    var quizName = levelPlayedMost.LevelMaxPlays.Quiz.Name;
                    levelMostPlayed.text = $"Quiz: {quizName[..(quizName.Length > 39 ? 39 : quizName.Length)]}{(quizName.Length > 39 ? "..." : "")}";
                }
            }
        }

        private CompleteStudentSessionDTO GetMostPlayedLevel(List<CompleteStudentSessionDTO> levelMostPlayedList)
        {
            var levelMostPlayedStudentSessions = levelMostPlayedList.GroupBy(x => x.LevelMaxPlays.LevelId)
                .OrderByDescending(z => z.Count()).FirstOrDefault();

            var quizMostPlayedStudentSessions = levelMostPlayedList.GroupBy(x => x.LevelMaxPlays.QuizId)
                .OrderByDescending(z => z.Count()).FirstOrDefault();
                
            if (levelMostPlayedStudentSessions.Key == null)
                return quizMostPlayedStudentSessions.First();
                
            if (quizMostPlayedStudentSessions.Key == null)
                return levelMostPlayedStudentSessions.First();
                
            if (levelMostPlayedStudentSessions.Count() > quizMostPlayedStudentSessions.Count())
                return levelMostPlayedStudentSessions.First();

            if (quizMostPlayedStudentSessions.Count() > levelMostPlayedStudentSessions.Count())
                return quizMostPlayedStudentSessions.First();

            return null;
        }

        private CompleteStudentSessionDTO GetMostFailedLevel(List<CompleteStudentSessionDTO> levelMostFailedList)
        {
            var levelMostFailedStudentSessions = levelMostFailedList.GroupBy(x => x.LevelMaxFails.LevelId)
                .OrderByDescending(z => z.Count()).FirstOrDefault();

            var quizMostFailedStudentSessions = levelMostFailedList.GroupBy(x => x.LevelMaxFails.QuizId)
                .OrderByDescending(z => z.Count()).FirstOrDefault();
                
            if (levelMostFailedStudentSessions.Key == null)
                return quizMostFailedStudentSessions.First();
                
            if (quizMostFailedStudentSessions.Key == null)
                return levelMostFailedStudentSessions.First();
                
            if (levelMostFailedStudentSessions.Count() > quizMostFailedStudentSessions.Count())
                return levelMostFailedStudentSessions.First();

            if (quizMostFailedStudentSessions.Count() > levelMostFailedStudentSessions.Count())
                return quizMostFailedStudentSessions.First();

            return null;
        }

        private void ResetTexts()
        {
            dateText.text = "";
            studentsText.text = "";
            levelsMostFails.text = "";
            levelMostPlayed.text = "";
        }

        private void NoData()
        {
            dateText.text = "Dados insuficientes";
            studentsText.text = "0";
            levelsMostFails.text = "Dados insuficientes";
            levelMostPlayed.text = "Dados insuficientes";
        }
    }
}
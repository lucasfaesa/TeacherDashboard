using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using OfficeOpenXml;
using _GraphsAndCharts.Scripts.StudentSpaceship;
using _TeacherDashboard.Scripts;
using API_Mestrado_Lucas;
using ClosedXML.Excel;
using ClosedXML.Graphics;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using static Third_Party.WebGL_File_Saver.WebGLFileSaver;

namespace _GraphsAndCharts.Scripts
{
    public class ExportData : MonoBehaviour
    {
        [SerializeField] private StudentSpaceshipDataSO studentSpaceshipData;
        [SerializeField] private TeacherDataSO teacherData;
        [Space] 
        [SerializeField] private GameObject exportButton;
        [Space]
        [SerializeField] private Transform saveSuccessModal;
        [SerializeField] private Transform saveSuccessModalWebGl;
        [SerializeField] private Transform saveErrorModal;
        [Space] 
        [SerializeField] private UnityEvent startedSaving;
        [SerializeField] private UnityEvent endedSaving;
        
        private void Start()
        {
            /*#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN && !UNITY_STANDALONE_LINUX
                exportButton.SetActive(false);
            #endif*/
        }

        public void ExportToCSV()
        {
            startedSaving?.Invoke();
            
            List<StudentCompleteInfoDTO> students =
                new(teacherData.TeacherDto.Students.Where(
                    x => x.GroupClassId == teacherData.CurrentlyChosenGroupClassId));

            students = students.OrderBy(x => x.Name).ToList();

            List<string> headers = new();

            var questionsOfQuiz =
                studentSpaceshipData.AllQuizesOfTeacher.Find(x =>
                    x.Id == studentSpaceshipData.CurrentSelectedQuiz.Id).Questions.ToList();


            if (students.Count == 0 || questionsOfQuiz.Count == 0)
            {
                SavedModal(false);
                return;
            }
            
            headers.AddRange(new List<string> {"Aluno", "Máx. Pontos", "Qtas. vezes jogou"});

            for (int i = 0; i < questionsOfQuiz.Count; i++)
            {
                var question = questionsOfQuiz[i].QuestionTitle;

                if (question.Length > 50)
                    question = question[..47] + "...";

                headers.Add(question);
            }

            //string csv = "sep=;\n"; //buga o encoding do excel
            string csv = string.Join(";", headers) + "\n";

            foreach (var student in students)
            {
                var listOfQuestionsOfStudent = QuestionAndAnswersList(student);
                var sessionsOfThisStudent = studentSpaceshipData.AllSpaceshipSessionsOfTeacher.Where(x =>
                    x.StudentId == student.Id
                    && x.QuizId == studentSpaceshipData.CurrentSelectedQuiz.Id).ToList();
                
                csv += $"{student.Name};"; //nome
                csv += sessionsOfThisStudent.Any() ? sessionsOfThisStudent.Max(x => x.Score) : ""; //max score
                csv += sessionsOfThisStudent.Count > 0 ? sessionsOfThisStudent.Count : ""; //qtd de vezes que jogou

                for (int i = 0; i < listOfQuestionsOfStudent.Count; i++)
                {
                    string commonWrongResp = listOfQuestionsOfStudent[i].CommonWrongResponse;

                    if (commonWrongResp.Length > 50)
                    {
                        commonWrongResp = commonWrongResp[..40] + "...";
                    }

                    csv += ";";

                    if (!string.IsNullOrEmpty(listOfQuestionsOfStudent[i].TotalErrorsOnQuestion))
                        csv +=
                            $"{listOfQuestionsOfStudent[i].TotalErrorsOnQuestion} ({listOfQuestionsOfStudent[i].ErrorPercentageTotal}% - {commonWrongResp})";
                    else
                        csv += $"";
                }

                csv += "\n";
            }

            string groupClassName = teacherData.TeacherDto.GroupClasses
                .First(x => x.Id == teacherData.CurrentlyChosenGroupClassId).Name;

            string filename =
                $"{groupClassName} - {studentSpaceshipData.CurrentSelectedQuiz.Name} - {DateTime.Now:dd-MM-yy}.csv";

            string path = "";
            
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Dados Alunos Robotim/" + filename;
            #endif
            #if UNITY_STANDALONE_LINUX
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Desktop/Dados Alunos Robotim/" + filename;
            #endif
            
            if(string.IsNullOrEmpty(path))
                Debug.Log("Path nulo");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
                {
                    writer.WriteLine(csv);
                }
                
                endedSaving?.Invoke();
                SavedModal(true);
                
            }
            catch (Exception e)
            {
                endedSaving?.Invoke();
                Console.WriteLine(e);
                SavedModal(false);
            }
            
        }

        public void ExportToXlsx()
        {
            startedSaving?.Invoke();
            StartCoroutine(ExportToXlsxRoutine());
        }
        private IEnumerator ExportToXlsxRoutine()
        {
            yield return new WaitForSeconds(0.1f);
            
            try
            {
                List<StudentCompleteInfoDTO> students = new List<StudentCompleteInfoDTO>(
                    teacherData.TeacherDto.Students.Where(
                        x => x.GroupClassId == teacherData.CurrentlyChosenGroupClassId));
                students = students.OrderBy(x => x.Name).ToList();

                var questionsOfQuiz = studentSpaceshipData.AllQuizesOfTeacher
                    .Find(x => x.Id == studentSpaceshipData.CurrentSelectedQuiz.Id).Questions.ToList();

                #if UNITY_STANDALONE_WIN || UNITY_EDITOR
                    LoadOptions.DefaultGraphicEngine = new DefaultGraphicEngine("Arial");
                #elif UNITY_STANDALONE_LINUX
                    LoadOptions.DefaultGraphicEngine = new DefaultGraphicEngine("Liberation Sans");
                #endif
                
                IXLWorkbook workbook = new XLWorkbook();
                
                IXLWorksheet worksheet = workbook.Worksheets.Add("Data");

                // Set up header row
                worksheet.Cell(1, 1).Value = "Aluno";
                worksheet.Cell(1, 2).Value = "Máx. Pontos";
                worksheet.Cell(1, 3).Value = "Qtas. vezes jogou";

                if (students.Count == 0 || questionsOfQuiz.Count == 0)
                {
                    SavedModal(false);
                    yield break;
                }

                for (int i = 0; i < questionsOfQuiz.Count; i++)
                {
                    var question = questionsOfQuiz[i].QuestionTitle;

                    if (question.Length > 50)
                    {
                        question = question[..47] + "...";
                    }

                    worksheet.Cell(1, i + 4).Value = question;
                }

                // Set up data rows
                for (int i = 0; i < students.Count; i++)
                {
                    var student = students[i];
                    var listOfQuestionsOfStudent = QuestionAndAnswersList(student);

                    var sessionsOfThisStudent = studentSpaceshipData.AllSpaceshipSessionsOfTeacher.Where(x =>
                        x.StudentId == student.Id
                        && x.QuizId == studentSpaceshipData.CurrentSelectedQuiz.Id).ToList();
                    
                    worksheet.Cell(i + 2, 1).Value = student.Name; //nome aluno
                    worksheet.Cell(i + 2, 2).Value =
                        sessionsOfThisStudent.Any() ? sessionsOfThisStudent.Max(x => x.Score) : ""; //max score
                    worksheet.Cell(i + 2, 3).Value =
                        sessionsOfThisStudent.Count > 0 ? sessionsOfThisStudent.Count : ""; //qtd vezes que jogou

                    for (int j = 0; j < listOfQuestionsOfStudent.Count; j++)
                    {
                        string commonWrongResp = listOfQuestionsOfStudent[j].CommonWrongResponse;

                        if (commonWrongResp.Length > 50)
                        {
                            commonWrongResp = commonWrongResp[..40] + "...";
                        }

                        if (!string.IsNullOrEmpty(listOfQuestionsOfStudent[j].TotalErrorsOnQuestion))
                        {
                            var formattedValue = $"{listOfQuestionsOfStudent[j].TotalErrorsOnQuestion} ({listOfQuestionsOfStudent[j].ErrorPercentageTotal}% - {commonWrongResp})";
                            worksheet.Cell(i + 2, j + 4).Value = formattedValue;
                            worksheet.Cell(i + 2, j + 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFC5C5");
                        }
                        else if(sessionsOfThisStudent.Count > 0) //não pintar de verde caso o estudante não tenha jogado nenhuma vez
                        {
                            worksheet.Cell(i + 2, j + 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#C5FFC5");
                        }
                    }
                }

                #if UNITY_STANDALONE_WIN || UNITY_EDITOR
                    worksheet.Columns().Style.Font.FontName = "Arial";
                #elif UNITY_STANDALONE_LINUX
                    worksheet.Columns().Style.Font.FontName = "Liberation Sans";
                #endif
                
                // Autofit column width
                #if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    worksheet.Columns().AdjustToContents();
                #else
                    worksheet.Columns().Width = 52;
                    worksheet.Column("A").Width = 25;
                    worksheet.Column("B").Width = 15;
                    worksheet.Column("C").Width = 18;
                #endif
                
                // alinhando no centro
                worksheet.Columns().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                
                // Set filename and path
                string groupClassName = teacherData.TeacherDto.GroupClasses
                    .First(x => x.Id == teacherData.CurrentlyChosenGroupClassId).Name;
                string filename =
                    $"{groupClassName} - {studentSpaceshipData.CurrentSelectedQuiz.Name} - {DateTime.Now:dd-MM-yy}.xlsx";

                string path = "";
                
                #if UNITY_STANDALONE_WIN || UNITY_EDITOR
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Dados Alunos Robotim/" + filename;
                #endif
                
                #if UNITY_STANDALONE_LINUX
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Desktop/Dados Alunos Robotim/" + filename;
                #endif

                #if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_STANDALONE_LINUX
                    if (string.IsNullOrEmpty(path))
                        Debug.Log("Path nulo");
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    workbook.SaveAs(path);
                #endif

                #if UNITY_WEBGL && !UNITY_EDITOR
                    if (IsSavingSupported())
                    {
                        var stream = new MemoryStream();
                        workbook.SaveAs(stream);
                        var bytes = stream.ToArray();
                        
                        SaveFile(bytes,filename,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    }
                #endif
                
                SavedModal(true);
                endedSaving?.Invoke();
                
            }
            
            catch (Exception e)
            {
                SavedModal(false);
                Console.WriteLine(e);
                endedSaving?.Invoke();
            }
            
        }
        

        public List<StudentExportData> QuestionAndAnswersList(StudentCompleteInfoDTO student)
        {
            var allQuestionsOfQuiz =
                studentSpaceshipData.AllQuizesOfTeacher
                    .Find(x => x.Id == studentSpaceshipData.CurrentSelectedQuiz.Id).Questions.ToList();

            var wrongQuestionsOfStudentGroupedList = studentSpaceshipData.AllStudentWrongAnswersOfQuiz
                .Where(x => x.StudentId == student.Id)
                .GroupBy(x => x.QuestionTitle)
                .OrderByDescending(x => x.Count());


            int totalWrongAnswers = studentSpaceshipData.AllStudentWrongAnswersOfQuiz.Count;

            List<StudentExportData> questAndPercentageList = new();

            foreach (var group in wrongQuestionsOfStudentGroupedList)
            {
                var mostCommonWrongResponse = group.GroupBy(x => x.StudentWrongAnswer).OrderByDescending(x => x.Count())
                    .First().Key;

                questAndPercentageList.Add(new StudentExportData(group.Key,
                    Convert.ToInt32(((float)group.Count() / totalWrongAnswers) * 100).ToString(),
                    mostCommonWrongResponse, group.Count().ToString()));
            }

            foreach (var questionAndPercentage in questAndPercentageList)
            {
                allQuestionsOfQuiz = allQuestionsOfQuiz
                    .Where(x => x.QuestionTitle != questionAndPercentage.QuestionTitle)
                    .ToList(); //removendo as perguntas que já estão na lista de erros
            }

            if (allQuestionsOfQuiz.Count > 0)
            {
                foreach (var question in allQuestionsOfQuiz)
                {
                    questAndPercentageList.Add(new StudentExportData(question.QuestionTitle, "", "", ""));
                }
            }


            return questAndPercentageList;
        }

        private void SavedModal(bool success)
        {
            Sequence sequence = DOTween.Sequence();
            if (success)
            {
                Transform saveModal = saveSuccessModal;
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                    saveModal = saveSuccessModalWebGl;
                #endif
                
                saveModal.localScale = Vector3.zero;
                saveModal.gameObject.SetActive(true);

                sequence.Append(saveModal.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack)).AppendInterval(2f);
                sequence.Append(saveModal.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));

                sequence.OnComplete(() =>
                {
                    saveModal.gameObject.SetActive(false);
                });
            }
            else
            {
                saveErrorModal.localScale = Vector3.zero;
                saveErrorModal.gameObject.SetActive(true);

                sequence.Append(saveErrorModal.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack)).AppendInterval(2f);
                sequence.Append(saveErrorModal.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));

                sequence.OnComplete(() =>
                {
                    saveErrorModal.gameObject.SetActive(false);
                });
            }
        }
    }

    public class StudentExportData
    {
        public string QuestionTitle { get; set; }
        public string ErrorPercentageTotal { get; set; }
        public string CommonWrongResponse { get; set; }
        public string TotalErrorsOnQuestion { get; set; }

        public StudentExportData(string title, string percentageTotal, string commonResp, string errorsTotal)
        {
            QuestionTitle = title;
            ErrorPercentageTotal = percentageTotal;
            CommonWrongResponse = commonResp;
            TotalErrorsOnQuestion = errorsTotal;
        }
    }
}
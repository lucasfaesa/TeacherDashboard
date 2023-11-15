public static class ApiPaths
{
    public static string API_URL(bool useAzurePath)
    {
        if (useAzurePath)
            return "";
        return "https://localhost:5001/api/";
    }

    public static string EDIT_STUDENT(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }

    public static string BATCH_POST_STUDENT_URL(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }

    public static string BATCH_POST_GROUPCLASS_URL(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }

    public static string BATCH_DELETE_STUDENT(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }
    
    public static string BATCH_DELETE_QUIZES(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }

    public static string BATCH_DELETE_GROUPCLASSES(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }

    public static string GET_GROUP_CLASS_LEVELS(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }

    public static string POST_GROUP_CLASS_LEVELS(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }
    
    public static string CHANGE_ACTIVE_QUIZ(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }

    public static string COMPLETE_STUDENT_SESSION_TEACHER_GROUPCLASS(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }
    
    public static string STUDENTS_LEVEL_SESSIONS_BY_GROUP_CLASS_ID(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }

    public static string LEVEL_URL(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }


    //Group Class
    public static string GROUP_CLASS_AND_LEVELS(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }
    
    public static string GET_SESSIONS_OF_SPACESHIP_LEVELS(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }

    
    public static string GET_QUIZES_BY_TEACHER(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }
    
    public static string POST_QUIZ(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }

    //Teacher
    public static string GET_TEACHER_INFOS(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }

    //subjects
    public static string GET_SUBJECTS(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }

    public static string TEACHER_LOGIN(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }
    
    public static string TEACHER_REGISTRATION(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }
    
    public static string GET_STUDENTS_WRONG_ANSWERS_BY_GROUPCLASS(bool useAzurePath)
    {
        return API_URL(useAzurePath) + "";
    }
}
using System;
using System.Reflection;

namespace ChartAndGraph
{
    internal class ChartEditorCommon
    {
        internal static bool IsAlphaNum(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            for (var i = 0; i < str.Length; i++)
                if (!char.IsLetter(str[i]) && !char.IsNumber(str[i]) && str[i] != ' ')
                    return false;

            return true;
        }

        internal static bool HasAttributeOfType(Type type, string fieldName, Type attributeType)
        {
            var inf = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (inf == null)
                return false;
            var attrb = inf.GetCustomAttributes(attributeType, true);
            if (attrb == null)
                return false;
            return attrb.Length > 0;
        }
    }
}
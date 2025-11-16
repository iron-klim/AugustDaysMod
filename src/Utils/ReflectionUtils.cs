using System.Reflection;

namespace AugustDaysMod.Utils
{
    public class ReflectionUtils
    {
        public static T SetFieldValue<T>(T target, string fieldName, object newValue)
        {
            var fieldInfo = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance); 
            fieldInfo.SetValue(target, newValue);
        
            return target;
        }

        public static object getFieldValue<T>(T target, string fieldName)
        {
            var fieldInfo = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            return fieldInfo.GetValue(target);
        }
    }
}

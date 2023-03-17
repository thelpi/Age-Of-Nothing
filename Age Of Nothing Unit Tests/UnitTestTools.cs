using System;
using System.Reflection;

namespace Age_Of_Nothing_Unit_Tests
{
    public static class UnitTestTools
    {
        public static T GetPrivateMemberValue<T>(this object data, string fieldName, bool isProperty = false)
        {
            var type = data.GetType();
            var value = isProperty
                ? type.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(data)
                : type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(data);

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static T GetPrivateMemberValue<T, TCast>(this object data, string fieldName, bool isProperty = false)
        {
            var value = isProperty
                ? typeof(TCast).GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(data)
                : typeof(TCast).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(data);

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}

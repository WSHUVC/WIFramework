using System;
using System.Reflection;

namespace WI
{
    public static partial class ReflectionExtend
    {
        public static FieldInfo[] GetAllFields(this Type target)
        {
            return target.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        }

        public static PropertyInfo[] GetAllProperties(this Type target)
        {
            return target.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        }
    }
}
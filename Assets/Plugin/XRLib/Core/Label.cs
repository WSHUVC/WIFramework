using System;

namespace WI
{
    [AttributeUsage(AttributeTargets.Field)]
    public partial class Label : Attribute
    {
        public string name;
        public Type target;

        public Label(Type targetType, string gameObjectName)
        {
            target = targetType;
            name = gameObjectName;
        }
    }
}
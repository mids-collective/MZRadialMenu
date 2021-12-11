using System;
namespace MZRadialMenu.Attributes {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class WheelTypeAttribute : Attribute {
        public Type _T;
        public WheelTypeAttribute(Type T) {
            this._T = T;
        }

    }
}
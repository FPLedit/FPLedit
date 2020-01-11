using System;

namespace FPLedit.Shared
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
    public class TemplateSafeAttribute : Attribute
    {
    }
}
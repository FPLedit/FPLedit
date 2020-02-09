using System;

namespace FPLedit.Shared.Templating
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class TemplateSafeAttribute : Attribute
    {
    }
}
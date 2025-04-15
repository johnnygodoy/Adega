// Filters/IgnorarLicencaAttribute.cs
using System;

namespace Adega.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class IgnorarLicencaAttribute : Attribute
    {
    }
}

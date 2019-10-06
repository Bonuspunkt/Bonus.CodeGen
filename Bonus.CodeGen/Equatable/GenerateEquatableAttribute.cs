using System;
using System.Diagnostics;
using CodeGeneration.Roslyn;

namespace Bonus.CodeGen
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [CodeGenerationAttribute(typeof(EquatableGenerator))]
    [Conditional("CodeGeneration")]
    public class GenerateEquatableAttribute : Attribute
    {
    }
}
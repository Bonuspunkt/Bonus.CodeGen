using System;
using System.Diagnostics;
using CodeGeneration.Roslyn;

namespace Bonus.CodeGen
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [CodeGenerationAttribute(typeof(ImmutableGenerator))]
    [Conditional("CodeGeneration")]
    public class GenerateImmutableAttribute : Attribute
    {
    }
}
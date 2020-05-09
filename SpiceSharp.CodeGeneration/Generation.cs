using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SpiceSharp.CodeGeneration
{
    /// <summary>
    /// Properties of a class that needs auto-generated code.
    /// </summary>
    public struct Generation
    {
        /// <summary>
        /// The class
        /// </summary>
        public readonly ClassDeclarationSyntax Class;

        /// <summary>
        /// The flag set if rules need to be added in the properties.
        /// </summary>
        public readonly bool AddRules;

        /// <summary>
        /// The flag set if a quick-reference needs to be added.
        /// </summary>
        public readonly bool AddNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="Generation"/> struct.
        /// </summary>
        public Generation(ClassDeclarationSyntax @class, bool addRules, bool addNames)
        {
            Class = @class;
            AddRules = addRules;
            AddNames = addNames;
        }

        public override bool Equals(object obj)
        {
            if (obj is Generation gen)
                return Class.Equals(gen.Class);
            return false;
        }

        public override int GetHashCode() => Class.GetHashCode();

        public static bool operator ==(Generation left, Generation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Generation left, Generation right)
        {
            return !(left == right);
        }
    }
}

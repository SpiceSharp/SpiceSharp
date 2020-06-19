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

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Generation gen)
                return Class.Equals(gen.Class);
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => Class.GetHashCode();

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Generation left, Generation right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Generation left, Generation right)
        {
            return !(left == right);
        }
    }
}

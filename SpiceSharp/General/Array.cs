namespace SpiceSharp
{
    /// <summary>
    /// A helper method as a replacement for System.Array.
    /// This method does not exist in .NET Standard 1.5 (even though the docs say it does).
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public static class Array<T>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1825:Avoid zero-length array allocations.", Justification = "Compatiblity for .NET Standard 1.5")]
        private static readonly T[] _empty = [];

        /// <summary>
        /// Returns an empty array.
        /// </summary>
        /// <returns>The empty array.</returns>
        /// <remarks>
        /// The static <see cref="System.Array"/> does not contain an Empty method
        /// in .NET Standard 1.5, despite it being documented as such. That's why
        /// I rolled out a custom one.
        /// </remarks>
        public static T[] Empty() => _empty;
    }
}

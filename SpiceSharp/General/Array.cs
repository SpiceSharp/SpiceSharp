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
        private static readonly T[] _empty = new T[0];

        /// <summary>
        /// Returns an empty array.
        /// </summary>
        /// <returns>The empty array.</returns>
        public static T[] Empty() => _empty;
    }
}

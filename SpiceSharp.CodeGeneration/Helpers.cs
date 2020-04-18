using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.CodeGeneration
{
    public static class Helpers
    {
        /// <summary>
        /// Removes the quotes from a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="Exception">If the string is not quoted.</exception>
        public static string Unquote(this string value)
        {
            if (value == null || value.Length < 2 || (value[0] != value[value.Length - 1]) || (value[0] != '"' && value[0] != '\''))
                throw new Exception("String expected instead of " + (value ?? "null"));
            return value.Substring(1, value.Length - 2);
        }
    }
}

using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// The name of a circuit object
    /// </summary>
    public class CircuitIdentifier
    {
        /// <summary>
        /// Gets or sets the separator for displaying path names
        /// </summary>
        public static string Separator = ".";

        /// <summary>
        /// Gets or sets case insensitivity
        /// </summary>
        public static bool CaseInsensitive = false;

        /// <summary>
        /// Used for hashing
        /// </summary>
        protected const int Prime = 31;

        /// <summary>
        /// Gets the full path of the circuit object
        /// </summary>
        public string[] Path { get; }

        /// <summary>
        /// Gets the local name of the circuit object
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Get the hash value
        /// </summary>
        private int hash;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">The full path</param>
        public CircuitIdentifier(params string[] path)
        {
            if (path.Length == 0)
                throw new CircuitException("Empty identifier");

            // Fix case if necessary
            if (CaseInsensitive)
            {
                for (int i = 0; i < path.Length; i++)
                    path[i] = path[i].ToLowerInvariant();
            }

            Path = path;
            Name = path[path.Length - 1];

            // Compute a hash code
            hash = 1;
            for (int i = 0; i < path.Length; i++)
            {
                if (string.IsNullOrEmpty(path[i]))
                    throw new CircuitException("Empty path name");
                hash = hash * Prime + path[i].GetHashCode();
            }
        }

        /// <summary>
        /// The full path will determine the hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => hash;

        /// <summary>
        /// Test if the object equals another
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is CircuitIdentifier con)
            {
                // Check lengths
                if (Path.Length != con.Path.Length)
                    return false;

                // Check each value
                for (int i = 0; i < con.Path.Length; i++)
                {
                    if (Path[i] != con.Path[i])
                        return false;
                }
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Display the circuit object name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(Separator, Path);
        }

        /// <summary>
        /// Concatenate a name to the path
        /// </summary>
        /// <param name="a">Identifier</param>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public static CircuitIdentifier operator +(CircuitIdentifier a, string b)
        {
            string[] npath = new string[a.Path.Length + 1];
            for (int i = 0; i < a.Path.Length; i++)
                npath[i] = a.Path[i];
            npath[a.Path.Length] = b;
            return new CircuitIdentifier(npath);
        }

        /// <summary>
        /// Implicitely convert a string array to a path for a circuit object
        /// </summary>
        /// <param name="path">Path</param>
        public static implicit operator CircuitIdentifier(string[] path) => new CircuitIdentifier(path);

        /// <summary>
        /// Implicitely convert a string to a name for a circuit object
        /// </summary>
        /// <param name="name">Name</param>
        // public static implicit operator CircuitIdentifier(string name) => new CircuitIdentifier(name);
    }
}

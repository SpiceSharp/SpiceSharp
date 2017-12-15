using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// An identifier for a circuit object
    /// </summary>
    public class Identifier
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
        /// This is the last path segment
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
        public Identifier(params string[] path)
        {
            // Check inputs
            if (path.Length == 0)
                throw new CircuitException("Empty path");

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
                hash = hash * Prime + path[i].GetHashCode();
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
            if (obj is Identifier con)
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
        /// Append an identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public Identifier Grow(string id)
        {
            string[] npath = new string[Path.Length + 1];
            for (int i = 0; i < Path.Length; i++)
                npath[i] = Path[i];
            npath[Path.Length] = id;
            return new Identifier(npath);
        }

        /// <summary>
        /// Append an identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public Identifier Grow(Identifier id)
        {
            string[] npath = new string[Path.Length + id.Path.Length];
            for (int i = 0; i < Path.Length; i++)
                npath[i] = Path[i];
            for (int i = 0; i < id.Path.Length; i++)
                npath[i + Path.Length] = id.Path[i];
            return new Identifier(npath);
        }

        /// <summary>
        /// Remove the last identifier from the path
        /// </summary>
        /// <returns></returns>
        public Identifier Shrink()
        {
            // Cannot shrink any more
            if (Path.Length == 1)
                return null;

            // Remove the second last item from the path
            string[] npath = new string[Path.Length - 1];
            for (int i = 0; i < npath.Length - 1; i++)
                npath[i] = Path[i];
            npath[npath.Length - 1] = Name;
            return new Identifier(npath);
        }

        /// <summary>
        /// Implicitely convert a string array to a path for a circuit object
        /// </summary>
        /// <param name="path">Path</param>
        public static implicit operator Identifier(string[] path) => new Identifier(path);

        /// <summary>
        /// Implicitely convert a string to a name for a circuit object
        /// </summary>
        /// <param name="name">Name</param>
        public static implicit operator Identifier(string name) => new Identifier(name);
    }
}

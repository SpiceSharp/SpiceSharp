using System;
using System.Collections.Generic;
using SpiceSharp.Diagnostics;

namespace SpiceSharp
{
    /// <summary>
    /// An identifier for a circuit object
    /// </summary>
    public class Identifier
    {
        /// <summary>
        /// Gets or sets the separator for displaying path names
        /// </summary>
        public static string Separator { get; set; } = ".";

        /// <summary>
        /// Used for hashing
        /// </summary>
        protected const int Prime = 31;

        /// <summary>
        /// Gets the full path of the circuit object
        /// </summary>
        public IEnumerable<string> Elements => _idPath;

        /// <summary>
        /// Gets the number of elements in the path
        /// </summary>
        public int PathCount => _idPath.Length;

        /// <summary>
        /// Gets the local name of the circuit object
        /// This is the last path segment
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the hash value
        /// </summary>
        private readonly int _hash;

        /// <summary>
        /// The path of the identifier
        /// </summary>
        private readonly string[] _idPath;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">The full path</param>
        public Identifier(params string[] path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            // Check inputs
            if (path.Length == 0)
                throw new CircuitException("Empty path");

            _idPath = path;
            Name = path[path.Length - 1];

            // Compute a hash code
            _hash = 1;
            foreach (var part in path)
                _hash = _hash * Prime + part.GetHashCode();
        }

        /// <summary>
        /// The full path will determine the hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => _hash;

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
                if (_idPath.Length != con._idPath.Length)
                    return false;

                // Check each value
                for (int i = 0; i < con._idPath.Length; i++)
                {
                    if (_idPath[i] != con._idPath[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Display the circuit object name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(Separator, _idPath);
        }

        /// <summary>
        /// Append an identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public Identifier Grow(string id)
        {
            string[] npath = new string[_idPath.Length + 1];
            for (int i = 0; i < _idPath.Length; i++)
                npath[i] = _idPath[i];
            npath[_idPath.Length] = id;
            return new Identifier(npath);
        }

        /// <summary>
        /// Append an identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public Identifier Grow(Identifier id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            string[] npath = new string[_idPath.Length + id._idPath.Length];
            for (int i = 0; i < _idPath.Length; i++)
                npath[i] = _idPath[i];
            for (int i = 0; i < id._idPath.Length; i++)
                npath[i + _idPath.Length] = id._idPath[i];
            return new Identifier(npath);
        }

        /// <summary>
        /// Remove the last identifier from the path
        /// </summary>
        /// <returns></returns>
        public Identifier Shrink()
        {
            // Cannot shrink any more
            if (_idPath.Length == 1)
                return null;

            // Remove the second last item from the path
            string[] npath = new string[_idPath.Length - 1];
            for (int i = 0; i < npath.Length - 1; i++)
                npath[i] = _idPath[i];
            npath[npath.Length - 1] = Name;
            return new Identifier(npath);
        }

        /// <summary>
        /// Implicitely convert a string array to a path for a circuit object
        /// </summary>
        /// <param name="path">idPath</param>
        public static implicit operator Identifier(string[] path) => new Identifier(path);

        /// <summary>
        /// Implicitely convert a string to a name for a circuit object
        /// </summary>
        /// <param name="name">Name</param>
        public static implicit operator Identifier(string name) => new Identifier(name);
    }
}

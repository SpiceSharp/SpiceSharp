using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Sparse vector element
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    [Serializable]
    internal class SparseVectorElement<T> : VectorElement<T> where T : IFormattable
    {
        /// <summary>
        /// Gets or sets the index
        /// </summary>
        public new int Index
        {
            get => base.Index;
            set => base.Index = value;
        }

        /// <summary>
        /// Gets or sets the next element in the vector
        /// </summary>
        public SparseVectorElement<T> NextInVector { get; set; }

        /// <summary>
        /// Gets or sets the previous element in the vector
        /// </summary>
        public SparseVectorElement<T> PreviousInVector { get; set; }

        /// <summary>
        /// Gets the next element
        /// </summary>
        public override VectorElement<T> Next => NextInVector;

        /// <summary>
        /// Gets the previous element
        /// </summary>
        public override VectorElement<T> Previous => PreviousInVector;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index"></param>
        public SparseVectorElement(int index)
            : base(index)
        {
        }
    }
}

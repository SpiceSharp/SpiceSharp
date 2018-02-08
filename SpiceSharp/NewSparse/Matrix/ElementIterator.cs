namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// Iterator for moving through the matrix
    /// </summary>
    public class MatrixIterator<T>
    {
        /// <summary>
        /// The current element
        /// </summary>
        MatrixElement<T> matrixElement;

        /// <summary>
        /// Gets the value of the current matrix element
        /// </summary>
        public Element<T> Element { get => matrixElement?.Element; }

        /// <summary>
        /// Gets the row of the current matrix element
        /// </summary>
        public int Row { get => matrixElement.Row; }

        /// <summary>
        /// Gets the column of the current matrix element
        /// </summary>
        public int Column { get => matrixElement.Column; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="current">The current element</param>
        public MatrixIterator(MatrixElement<T> current)
        {
            matrixElement = current;
        }

        /// <summary>
        /// Move to the element left
        /// </summary>
        public void MoveLeft() => matrixElement = matrixElement.PreviousInRow;

        /// <summary>
        /// Move to the element right
        /// </summary>
        public void MoveRight() => matrixElement = matrixElement.NextInRow;

        /// <summary>
        /// Move to the element top
        /// </summary>
        public void MoveUp() => matrixElement = matrixElement.PreviousInColumn;

        /// <summary>
        /// Move to the element bottom
        /// </summary>
        public void MoveDown() => matrixElement = matrixElement.NextInColumn;

        /// <summary>
        /// Make another iterator that branches from the current matrix element
        /// </summary>
        /// <returns></returns>
        public MatrixIterator<T> Branch() => new MatrixIterator<T>(matrixElement);

        /// <summary>
        /// Branch to the left
        /// </summary>
        public MatrixIterator<T> BranchLeft() => new MatrixIterator<T>(matrixElement.PreviousInRow);

        /// <summary>
        /// Branch to the right
        /// </summary>
        public MatrixIterator<T> BranchRight() => new MatrixIterator<T>(matrixElement.NextInRow);

        /// <summary>
        /// Branch to the top
        /// </summary>
        public MatrixIterator<T> BranchTop() => new MatrixIterator<T>(matrixElement.PreviousInColumn);

        /// <summary>
        /// Branch to the bottom
        /// </summary>
        public MatrixIterator<T> BranchDown() => new MatrixIterator<T>(matrixElement.NextInColumn);
    }
}

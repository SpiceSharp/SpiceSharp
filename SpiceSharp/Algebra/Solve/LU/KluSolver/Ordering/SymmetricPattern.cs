using System;

namespace SpiceSharp.Algebra.Solve;

/// <summary>
/// The sparsity pattern of a matrix plus its transpose, with the diagonal left out.
/// This is the undirected graph that a symmetric fill-reducing ordering works on.
/// </summary>
public sealed class SymmetricPattern
{
    /// <summary>
    /// Gets the number of nodes.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets the index in <see cref="Indices"/> at which each node's neighbours start.
    /// Contains <see cref="Size"/> + 1 elements.
    /// </summary>
    public int[] Pointers { get; }

    /// <summary>
    /// Gets the neighbours of every node, grouped per node and free of duplicates.
    /// </summary>
    public int[] Indices { get; }

    private SymmetricPattern(int size, int[] pointers, int[] indices)
    {
        Size = size;
        Pointers = pointers;
        Indices = indices;
    }

    /// <summary>
    /// Builds the pattern of the union of a matrix and its transpose.
    /// </summary>
    /// <param name="size">The number of rows and columns.</param>
    /// <param name="columns">The column pointers, with <paramref name="size"/> + 1 elements.</param>
    /// <param name="rows">The row indices.</param>
    /// <returns>The symmetrized pattern.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="columns"/> or <paramref name="rows"/> is <c>null</c>.</exception>
    public static SymmetricPattern Create(int size, int[] columns, int[] rows)
    {
        columns.ThrowIfNull(nameof(columns));
        rows.ThrowIfNull(nameof(rows));
        if (size <= 0)
            return new SymmetricPattern(0, [0], []);

        // Transpose the pattern so that we can walk a node's row as easily as its column.
        int count = columns[size];
        int[] transposePointers = new int[size + 1];
        for (int p = 0; p < count; p++)
            transposePointers[rows[p]]++;
        int running = 0;
        for (int i = 0; i < size; i++)
        {
            int degree = transposePointers[i];
            transposePointers[i] = running;
            running += degree;
        }
        transposePointers[size] = running;

        int[] transposeIndices = new int[count];
        int[] fill = new int[size];
        Array.Copy(transposePointers, fill, size);
        for (int j = 0; j < size; j++)
        {
            int end = columns[j + 1];
            for (int p = columns[j]; p < end; p++)
                transposeIndices[fill[rows[p]]++] = j;
        }

        // Count the neighbours of every node, then fill them in. A marker keeps an entry
        // that appears in both the column and the row from being stored twice.
        int[] pointers = new int[size + 1];
        int[] marker = new int[size];
        for (int i = 0; i < size; i++)
            marker[i] = -1;

        int total = 0;
        for (int j = 0; j < size; j++)
        {
            int degree = 0;
            for (int p = columns[j]; p < columns[j + 1]; p++)
            {
                int i = rows[p];
                if (i != j && marker[i] != j)
                {
                    marker[i] = j;
                    degree++;
                }
            }
            for (int p = transposePointers[j]; p < transposePointers[j + 1]; p++)
            {
                int i = transposeIndices[p];
                if (i != j && marker[i] != j)
                {
                    marker[i] = j;
                    degree++;
                }
            }
            pointers[j] = degree;
            total += degree;
        }

        int offset = 0;
        for (int j = 0; j < size; j++)
        {
            int degree = pointers[j];
            pointers[j] = offset;
            offset += degree;
        }
        pointers[size] = total;

        int[] indices = new int[total];
        for (int i = 0; i < size; i++)
            marker[i] = -1;
        for (int j = 0; j < size; j++)
        {
            int write = pointers[j];
            for (int p = columns[j]; p < columns[j + 1]; p++)
            {
                int i = rows[p];
                if (i != j && marker[i] != j)
                {
                    marker[i] = j;
                    indices[write++] = i;
                }
            }
            for (int p = transposePointers[j]; p < transposePointers[j + 1]; p++)
            {
                int i = transposeIndices[p];
                if (i != j && marker[i] != j)
                {
                    marker[i] = j;
                    indices[write++] = i;
                }
            }
        }
        return new SymmetricPattern(size, pointers, indices);
    }
}

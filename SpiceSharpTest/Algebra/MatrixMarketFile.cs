using SpiceSharp.Algebra;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Numerics;

namespace SpiceSharpTest.Algebra;

/// <summary>
/// The symmetry that a Matrix Market file declares. Anything other than
/// <see cref="General"/> means only one triangle is stored and the other one has to be
/// filled in while reading.
/// </summary>
public enum MatrixMarketSymmetry
{
    /// <summary>Every entry is stored.</summary>
    General,

    /// <summary>Only the lower triangle is stored, a[j,i] equals a[i,j].</summary>
    Symmetric,

    /// <summary>Only the lower triangle is stored, a[j,i] equals -a[i,j].</summary>
    SkewSymmetric,

    /// <summary>Only the lower triangle is stored, a[j,i] is the conjugate of a[i,j].</summary>
    Hermitian
}

/// <summary>
/// A sparse matrix read from a Matrix Market (<c>.mtx</c>) file, optionally gzipped.
/// </summary>
/// <remarks>
/// <para>
/// The format is described at <see href="https://math.nist.gov/MatrixMarket/formats.html"/>.
/// Only the <c>matrix</c> object in <c>coordinate</c> format is supported, which is what the
/// matrices in <c>Algebra/Matrices</c> use. Indices are one-based in the file and stay
/// one-based here, which is also what the solvers want.
/// </para>
/// <para>
/// Entries are kept as read, in the file's order, including any duplicates and any entries
/// that are explicitly stored as zero. Loading uses <c>Add</c> rather than assignment so that
/// duplicates accumulate, which is how the format defines them and how the circuit matrices
/// this library builds behave as well.
/// </para>
/// </remarks>
public sealed class MatrixMarketFile
{
    private readonly int[] _rows;
    private readonly int[] _columns;
    private readonly double[] _values;
    private readonly double[] _imaginary;

    /// <summary>
    /// Gets the number of rows and columns. Only square matrices are read.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets the number of stored entries, after the missing triangle of a non-general
    /// matrix has been filled in.
    /// </summary>
    public int Count => _rows.Length;

    /// <summary>
    /// Gets whether the file stores complex values.
    /// </summary>
    public bool IsComplex => _imaginary != null;

    /// <summary>
    /// Gets the symmetry declared by the file.
    /// </summary>
    public MatrixMarketSymmetry Symmetry { get; }

    private MatrixMarketFile(int size, MatrixMarketSymmetry symmetry,
        int[] rows, int[] columns, double[] values, double[] imaginary)
    {
        Size = size;
        Symmetry = symmetry;
        _rows = rows;
        _columns = columns;
        _values = values;
        _imaginary = imaginary;
    }

    /// <summary>
    /// Reads a Matrix Market file. A <c>.gz</c> extension is decompressed on the fly.
    /// </summary>
    /// <param name="filename">The file to read.</param>
    /// <returns>The matrix.</returns>
    /// <exception cref="FormatException">Thrown if the file is not a square coordinate matrix.</exception>
    public static MatrixMarketFile Read(string filename)
    {
        Stream stream = File.OpenRead(filename);
        if (filename.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
            stream = new GZipStream(stream, CompressionMode.Decompress);
        using var reader = new StreamReader(stream);
        return Read(reader);
    }

    /// <summary>
    /// Reads a Matrix Market file from a reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>The matrix.</returns>
    /// <exception cref="FormatException">Thrown if the file is not a square coordinate matrix.</exception>
    public static MatrixMarketFile Read(TextReader reader)
    {
        string banner = reader.ReadLine()
            ?? throw new FormatException("The file is empty");
        string[] fields = Split(banner);
        if (fields.Length < 5 || !fields[0].Equals("%%MatrixMarket", StringComparison.OrdinalIgnoreCase))
            throw new FormatException($"Not a Matrix Market banner: '{banner}'");
        if (!fields[1].Equals("matrix", StringComparison.OrdinalIgnoreCase))
            throw new FormatException($"Unsupported object '{fields[1]}', only 'matrix' is read");
        if (!fields[2].Equals("coordinate", StringComparison.OrdinalIgnoreCase))
            throw new FormatException($"Unsupported format '{fields[2]}', only 'coordinate' is read");

        string field = fields[3].ToLowerInvariant();
        bool complex = field == "complex";
        bool pattern = field == "pattern";
        if (!complex && !pattern && field != "real" && field != "integer")
            throw new FormatException($"Unsupported field '{fields[3]}'");

        var symmetry = fields[4].ToLowerInvariant() switch
        {
            "general" => MatrixMarketSymmetry.General,
            "symmetric" => MatrixMarketSymmetry.Symmetric,
            "skew-symmetric" => MatrixMarketSymmetry.SkewSymmetric,
            "hermitian" => MatrixMarketSymmetry.Hermitian,
            _ => throw new FormatException($"Unsupported symmetry '{fields[4]}'")
        };

        // Comments, then the dimensions.
        string line;
        do
        {
            line = reader.ReadLine() ?? throw new FormatException("The file has no dimensions");
        }
        while (line.Length == 0 || line[0] == '%');

        fields = Split(line);
        if (fields.Length < 3)
            throw new FormatException($"Expected 'rows columns entries', got '{line}'");
        int size = int.Parse(fields[0], CultureInfo.InvariantCulture);
        if (int.Parse(fields[1], CultureInfo.InvariantCulture) != size)
            throw new FormatException("The matrix is not square");
        int declared = int.Parse(fields[2], CultureInfo.InvariantCulture);

        // A non-general matrix only stores one triangle, so reading it mirrors every
        // off-diagonal entry and the final count is not known up front.
        var rows = new List<int>(declared);
        var columns = new List<int>(declared);
        var values = new List<double>(declared);
        var imaginary = complex ? new List<double>(declared) : null;

        for (int k = 0; k < declared; k++)
        {
            line = reader.ReadLine() ?? throw new FormatException(
                $"Expected {declared} entries but the file ended after {k}");
            fields = Split(line);
            if (fields.Length < (pattern ? 2 : complex ? 4 : 3))
                throw new FormatException($"Could not read entry {k + 1}: '{line}'");

            int row = int.Parse(fields[0], CultureInfo.InvariantCulture);
            int column = int.Parse(fields[1], CultureInfo.InvariantCulture);
            if (row < 1 || row > size || column < 1 || column > size)
                throw new FormatException($"Entry {k + 1} at ({row},{column}) is out of range");
            double real = pattern ? 1.0 : double.Parse(fields[2], CultureInfo.InvariantCulture);
            double imag = complex ? double.Parse(fields[3], CultureInfo.InvariantCulture) : 0.0;

            rows.Add(row);
            columns.Add(column);
            values.Add(real);
            imaginary?.Add(imag);

            if (symmetry == MatrixMarketSymmetry.General || row == column)
                continue;
            rows.Add(column);
            columns.Add(row);
            switch (symmetry)
            {
                case MatrixMarketSymmetry.Symmetric:
                    values.Add(real);
                    imaginary?.Add(imag);
                    break;
                case MatrixMarketSymmetry.SkewSymmetric:
                    values.Add(-real);
                    imaginary?.Add(-imag);
                    break;
                default: // Hermitian
                    values.Add(real);
                    imaginary?.Add(-imag);
                    break;
            }
        }

        return new MatrixMarketFile(size, symmetry,
            [.. rows], [.. columns], [.. values], imaginary is null ? null : [.. imaginary]);
    }

    /// <summary>
    /// Adds every entry to a solver.
    /// </summary>
    /// <param name="solver">The solver.</param>
    /// <exception cref="InvalidOperationException">Thrown if the file is complex.</exception>
    public void LoadInto(ISparseSolver<double> solver)
    {
        if (IsComplex)
            throw new InvalidOperationException("The matrix is complex, load it into a complex solver");
        for (int k = 0; k < _rows.Length; k++)
            solver.GetElement(new MatrixLocation(_rows[k], _columns[k])).Add(_values[k]);
    }

    /// <summary>
    /// Adds every entry to a solver. A real matrix loads with zero imaginary parts.
    /// </summary>
    /// <param name="solver">The solver.</param>
    public void LoadInto(ISparseSolver<Complex> solver)
    {
        for (int k = 0; k < _rows.Length; k++)
        {
            var value = new Complex(_values[k], _imaginary is null ? 0.0 : _imaginary[k]);
            solver.GetElement(new MatrixLocation(_rows[k], _columns[k])).Add(value);
        }
    }

    /// <summary>
    /// Multiplies the matrix by a vector. Indices are one-based, so element 0 is unused.
    /// </summary>
    /// <param name="x">The vector.</param>
    /// <param name="transposed">If <c>true</c>, multiplies by the transpose instead.</param>
    /// <returns>The product.</returns>
    public double[] Multiply(double[] x, bool transposed = false)
    {
        if (IsComplex)
            throw new InvalidOperationException("The matrix is complex");
        double[] b = new double[Size + 1];
        for (int k = 0; k < _rows.Length; k++)
        {
            int row = transposed ? _columns[k] : _rows[k];
            int column = transposed ? _rows[k] : _columns[k];
            b[row] += _values[k] * x[column];
        }
        return b;
    }

    /// <summary>
    /// Multiplies the matrix by a vector. Indices are one-based, so element 0 is unused.
    /// </summary>
    /// <param name="x">The vector.</param>
    /// <param name="transposed">If <c>true</c>, multiplies by the transpose instead.</param>
    /// <returns>The product.</returns>
    public Complex[] Multiply(Complex[] x, bool transposed = false)
    {
        var b = new Complex[Size + 1];
        for (int k = 0; k < _rows.Length; k++)
        {
            int row = transposed ? _columns[k] : _rows[k];
            int column = transposed ? _rows[k] : _columns[k];
            var value = new Complex(_values[k], _imaginary is null ? 0.0 : _imaginary[k]);
            b[row] += value * x[column];
        }
        return b;
    }

    /// <summary>
    /// Multiplies the magnitudes of the entries by a vector of magnitudes. Per row this
    /// gives the sum of the magnitudes of the terms that a matrix-vector product adds
    /// together, which is the scale a residual for that row has to be measured against.
    /// </summary>
    /// <param name="magnitudes">The magnitudes of the vector, one-based.</param>
    /// <param name="transposed">If <c>true</c>, uses the transpose instead.</param>
    /// <returns>The product.</returns>
    public double[] MultiplyMagnitudes(double[] magnitudes, bool transposed = false)
    {
        double[] b = new double[Size + 1];
        for (int k = 0; k < _rows.Length; k++)
        {
            int row = transposed ? _columns[k] : _rows[k];
            int column = transposed ? _rows[k] : _columns[k];
            double magnitude = _imaginary is null
                ? Math.Abs(_values[k])
                : new Complex(_values[k], _imaginary[k]).Magnitude;
            b[row] += magnitude * magnitudes[column];
        }
        return b;
    }

    /// <summary>
    /// The largest absolute row sum of the matrix, which is the infinity norm.
    /// </summary>
    /// <param name="transposed">If <c>true</c>, uses the transpose instead.</param>
    /// <returns>The norm.</returns>
    public double InfinityNorm(bool transposed = false)
    {
        double[] ones = new double[Size + 1];
        for (int i = 1; i <= Size; i++)
            ones[i] = 1.0;
        double[] sums = MultiplyMagnitudes(ones, transposed);
        double worst = 0.0;
        for (int i = 1; i <= Size; i++)
            worst = Math.Max(worst, sums[i]);
        return worst;
    }

    private static string[] Split(string line)
        => line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
}

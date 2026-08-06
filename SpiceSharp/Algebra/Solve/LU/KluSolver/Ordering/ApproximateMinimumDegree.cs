using System;

namespace SpiceSharp.Algebra.Solve;

/// <summary>
/// Computes a fill-reducing ordering using the approximate minimum degree algorithm of
/// Amestoy, Davis and Duff. This is the ordering KLU uses by default for the diagonal
/// blocks it has to factor.
/// </summary>
/// <remarks>
/// <para>
/// Eliminating the node of smallest degree first is a good fill-reducing heuristic, but
/// keeping exact degrees up to date is the expensive part. The approximation instead keeps
/// an upper bound on each degree that is cheap to maintain, and in practice the bound is
/// tight enough that the ordering is as good as true minimum degree at a fraction of the
/// cost.
/// </para>
/// <para>
/// The graph is held in quotient form: eliminated nodes become "elements", each standing
/// for the clique its elimination created, and a node's neighbours are stored as a list of
/// elements it belongs to followed by a list of nodes it is still directly connected to.
/// That representation never needs more memory than the original graph, because the
/// cliques are named rather than expanded. On top of that, nodes whose neighbourhoods have
/// become identical are merged into one supervariable, nodes left with no neighbours
/// outside the current clique come out together with the pivot, and elements fully
/// contained in the new clique are absorbed into it.
/// </para>
/// <para>
/// Nodes are returned in the order they were eliminated. The reference implementation
/// additionally postorders the assembly tree, which improves memory locality during
/// factoring but produces the same amount of fill.
/// </para>
/// </remarks>
public static class ApproximateMinimumDegree
{
    /// <summary>
    /// Computes a fill-reducing ordering for the given sparsity pattern. The pattern does
    /// not have to be symmetric; the ordering is computed for the pattern of the matrix
    /// plus its transpose.
    /// </summary>
    /// <param name="size">The number of rows and columns.</param>
    /// <param name="columns">The column pointers, with <paramref name="size"/> + 1 elements.</param>
    /// <param name="rows">The row indices.</param>
    /// <param name="permutation">
    /// Receives the node that ends up at each position, so applying it to both the rows and
    /// the columns of the matrix keeps the diagonal in place. Needs at least
    /// <paramref name="size"/> elements.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if any argument is <c>null</c>.</exception>
    public static void Order(int size, int[] columns, int[] rows, int[] permutation)
    {
        permutation.ThrowIfNull(nameof(permutation));
        if (size <= 0)
            return;
        Order(SymmetricPattern.Create(size, columns, rows), permutation);
    }

    /// <summary>
    /// Computes a fill-reducing ordering for an already symmetrized pattern.
    /// </summary>
    /// <param name="pattern">The symmetrized pattern.</param>
    /// <param name="permutation">
    /// Receives the node that ends up at each position. Needs at least
    /// <see cref="SymmetricPattern.Size"/> elements.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if any argument is <c>null</c>.</exception>
    public static void Order(SymmetricPattern pattern, int[] permutation)
    {
        pattern.ThrowIfNull(nameof(pattern));
        permutation.ThrowIfNull(nameof(permutation));
        if (pattern.Size <= 0)
            return;
        new Worker(pattern).Order(permutation);
    }

    /// <summary>
    /// Holds the mutable state of one ordering run.
    /// </summary>
    private sealed class Worker
    {
        private const int Variable = 0;
        private const int Element = 1;
        private const int Dead = 2;

        private readonly int _size;

        /// <summary>Start of each node's list in <see cref="_list"/>, or -1 if it has none.</summary>
        private readonly int[] _start;

        /// <summary>Total length of each node's list.</summary>
        private readonly int[] _length;

        /// <summary>How many entries at the front of a node's list are elements.</summary>
        private readonly int[] _elements;

        /// <summary>How many original nodes a live node stands for; 0 for anything else.</summary>
        private readonly int[] _count;

        /// <summary>Approximate external degree of a node, or the size of an element.</summary>
        private readonly int[] _degree;

        /// <summary><see cref="Variable"/>, <see cref="Element"/> or <see cref="Dead"/>.</summary>
        private readonly int[] _state;

        /// <summary>Which node absorbed this one, or -1 if it was never absorbed.</summary>
        private readonly int[] _parent;

        /// <summary>The elimination step of a pivot, or -1 for anything not (yet) a pivot.</summary>
        private readonly int[] _step;

        /// <summary>The total number of elimination steps that were needed.</summary>
        private int _steps;

        /// <summary>Running per-element count of how much of it lies outside the new clique.</summary>
        private readonly int[] _work;
        private int _workFlag = 1;

        /// <summary>Marks the members of the clique being built.</summary>
        private readonly int[] _mark;
        private int _markFlag;

        /// <summary>Marks one node's neighbours while looking for an identical neighbourhood.</summary>
        private readonly int[] _compare;
        private int _compareFlag;

        private readonly int[] _degreeHead;
        private readonly int[] _degreeNext;
        private readonly int[] _degreePrevious;

        private readonly int[] _hash;
        private readonly int[] _hashHead;
        private readonly int[] _hashNext;

        private int[] _list;
        private int _free;

        /// <summary>Scratch used to remember a list head while compacting.</summary>
        private readonly int[] _saved;

        public Worker(SymmetricPattern pattern)
        {
            int n = _size = pattern.Size;
            _start = new int[n];
            _length = new int[n];
            _elements = new int[n];
            _count = new int[n];
            _degree = new int[n];
            _state = new int[n];
            _parent = new int[n];
            _step = new int[n];
            _work = new int[n];
            _mark = new int[n];
            _compare = new int[n];
            _degreeHead = new int[n + 1];
            _degreeNext = new int[n];
            _degreePrevious = new int[n];
            _hash = new int[n];
            _hashHead = new int[n];
            _hashNext = new int[n];
            _saved = new int[n];

            // Leave room for the element lists that elimination is going to create. They
            // reuse the space of what they absorb, but not always in place, so a compacting
            // pass reclaims the gaps when the slack runs out.
            int nonzeroes = pattern.Pointers[n];
            _list = new int[nonzeroes + (nonzeroes / 5) + n + 1];
            Array.Copy(pattern.Indices, _list, nonzeroes);
            _free = nonzeroes;

            for (int i = 0; i < n; i++)
            {
                int degree = pattern.Pointers[i + 1] - pattern.Pointers[i];
                _start[i] = degree > 0 ? pattern.Pointers[i] : -1;
                _length[i] = degree;
                _degree[i] = degree;
                _count[i] = 1;
                _state[i] = Variable;
                _parent[i] = -1;
                _step[i] = -1;
                _hashHead[i] = -1;
            }
            for (int d = 0; d <= n; d++)
                _degreeHead[d] = -1;
            for (int i = n - 1; i >= 0; i--)
                AddToDegreeList(i, _degree[i]);
        }

        public void Order(int[] permutation)
        {
            Eliminate();
            BuildPermutation(permutation);
        }

        /// <summary>
        /// Repeatedly eliminates the node of smallest approximate degree until nothing is left.
        /// </summary>
        private void Eliminate()
        {
            int n = _size;
            int eliminated = 0;
            int minimum = 0;
            int step = 0;

            while (eliminated < n)
            {
                // ----- pick the pivot -----
                int pivot = -1;
                while (minimum <= n)
                {
                    pivot = _degreeHead[minimum];
                    if (pivot >= 0)
                        break;
                    minimum++;
                }
                if (pivot < 0)
                    break;
                RemoveFromDegreeList(pivot, _degree[pivot]);

                int pivotTotal = _count[pivot];
                _count[pivot] = 0;
                eliminated += pivotTotal;

                // Advancing the marker by the size of the graph each step would overflow on
                // a large matrix, so reset it once it gets close.
                if (_workFlag > int.MaxValue - n - 2)
                {
                    Array.Clear(_work, 0, n);
                    _workFlag = 1;
                }
                if (_markFlag > int.MaxValue - 2)
                {
                    Array.Clear(_mark, 0, n);
                    _markFlag = 0;
                }
                _markFlag++;

                // ----- build the clique the pivot leaves behind -----
                // Its members are the pivot's remaining direct neighbours together with
                // everything in the elements it belongs to. Those elements are absorbed:
                // whatever they connected is now connected through the new clique instead.
                EnsureSpace(n);
                _mark[pivot] = _markFlag;

                int cliqueStart = _free;
                int cliqueSize = 0;
                if (_length[pivot] > 0)
                {
                    int listStart = _start[pivot];
                    int elementEnd = listStart + _elements[pivot];
                    int listEnd = listStart + _length[pivot];

                    for (int p = listStart; p < elementEnd; p++)
                    {
                        int element = _list[p];
                        if (_state[element] != Element)
                            continue;
                        int memberEnd = _start[element] + _length[element];
                        for (int q = _start[element]; q < memberEnd; q++)
                        {
                            int member = _list[q];
                            if (_count[member] <= 0 || _mark[member] == _markFlag)
                                continue;
                            _mark[member] = _markFlag;
                            RemoveFromDegreeList(member, _degree[member]);
                            _list[_free++] = member;
                            cliqueSize += _count[member];
                        }
                        Absorb(element, pivot);
                    }
                    for (int p = elementEnd; p < listEnd; p++)
                    {
                        int member = _list[p];
                        if (_count[member] <= 0 || _mark[member] == _markFlag)
                            continue;
                        _mark[member] = _markFlag;
                        RemoveFromDegreeList(member, _degree[member]);
                        _list[_free++] = member;
                        cliqueSize += _count[member];
                    }
                }

                _start[pivot] = cliqueStart;
                _length[pivot] = _free - cliqueStart;
                _elements[pivot] = -1;
                _degree[pivot] = cliqueSize;
                _state[pivot] = Element;

                int cliqueEnd = cliqueStart + _length[pivot];

                // ----- how much of each neighbouring element sticks out of the clique -----
                // Storing the running count offset by a flag avoids having to clear the
                // array between steps: anything below the flag is left over from before.
                int highest = _workFlag;
                for (int p = cliqueStart; p < cliqueEnd; p++)
                {
                    int member = _list[p];
                    int listStart = _start[member];
                    int elementEnd = listStart + _elements[member];
                    for (int q = listStart; q < elementEnd; q++)
                    {
                        int element = _list[q];
                        if (_state[element] != Element)
                            continue;
                        if (_work[element] < _workFlag)
                        {
                            _work[element] = _degree[element] + _workFlag;
                            if (_work[element] > highest)
                                highest = _work[element];
                        }
                        _work[element] -= _count[member];
                    }
                }

                // ----- update every member of the clique -----
                for (int p = cliqueStart; p < cliqueEnd; p++)
                {
                    int member = _list[p];
                    int members = _count[member];
                    if (members <= 0)
                        continue;

                    int listStart = _start[member];
                    int elementEnd = listStart + _elements[member];
                    int listEnd = listStart + _length[member];
                    int write = listStart;
                    int bound = 0;
                    long hash = 0;

                    // Elements entirely inside the new clique add nothing, so the clique
                    // absorbs them; the rest contribute what sticks out.
                    for (int q = listStart; q < elementEnd; q++)
                    {
                        int element = _list[q];
                        if (_state[element] != Element)
                            continue;
                        int outside = _work[element] - _workFlag;
                        if (outside <= 0)
                        {
                            Absorb(element, pivot);
                            continue;
                        }
                        bound += outside;
                        _list[write++] = element;
                        hash += element;
                    }
                    int keptElements = write - listStart;

                    // Direct neighbours that are in the clique are reachable through it now,
                    // so they can be dropped from the list.
                    int variableStart = write;
                    for (int q = elementEnd; q < listEnd; q++)
                    {
                        int other = _list[q];
                        if (_count[other] <= 0 || _mark[other] == _markFlag)
                            continue;
                        bound += _count[other];
                        _list[write++] = other;
                        hash += other;
                    }

                    if (keptElements == 0 && variableStart == write)
                    {
                        // Nothing outside the clique is left, so this node has no external
                        // degree and comes out together with the pivot.
                        _state[member] = Dead;
                        _parent[member] = pivot;
                        _count[member] = 0;
                        _start[member] = -1;
                        _length[member] = 0;
                        _elements[member] = 0;
                        cliqueSize -= members;
                        pivotTotal += members;
                        eliminated += members;
                        continue;
                    }

                    if (bound < _degree[member])
                        _degree[member] = bound;

                    // Splice the new clique in at the front, where the elements live. The new
                    // list is always at least one entry shorter than the old one, so the
                    // displaced entries stay inside the space the old list occupied.
                    _list[write] = _list[variableStart];
                    _list[variableStart] = _list[listStart];
                    _list[listStart] = pivot;
                    _length[member] = write - listStart + 1;
                    _elements[member] = keptElements + 1;
                    _start[member] = listStart;
                    _hash[member] = (int)(hash % n);
                }

                MergeIndistinguishable(cliqueStart, cliqueEnd);

                // ----- give the survivors their new degrees and drop the rest -----
                int remaining = n - eliminated;
                int kept = cliqueStart;
                for (int p = cliqueStart; p < cliqueEnd; p++)
                {
                    int member = _list[p];
                    int members = _count[member];
                    if (members <= 0)
                        continue;

                    int degree = _degree[member] + cliqueSize - members;
                    int cap = remaining - members;
                    if (degree > cap)
                        degree = cap;
                    if (degree < 0)
                        degree = 0;
                    _degree[member] = degree;
                    AddToDegreeList(member, degree);
                    if (degree < minimum)
                        minimum = degree;
                    _list[kept++] = member;
                }

                _length[pivot] = kept - cliqueStart;
                _degree[pivot] = cliqueSize;
                _step[pivot] = step++;
                if (_length[pivot] == 0)
                {
                    // An element nobody belongs to can never be referenced again.
                    _start[pivot] = -1;
                    _state[pivot] = Dead;
                }
                _free = kept;
                _workFlag = highest + 1;
            }

            // Every node should be accounted for by now, either as a pivot or through
            // whatever absorbed it. Anything left over still needs a position so that
            // building the permutation always terminates on a valid one.
            for (int i = 0; i < n; i++)
            {
                if (_step[i] < 0 && _parent[i] < 0)
                    _step[i] = step++;
            }
            _steps = step;
        }

        /// <summary>
        /// Folds together the clique members whose remaining neighbourhoods have become
        /// identical. Such nodes are interchangeable from here on, so treating them as a
        /// single node of larger weight costs nothing and shrinks the graph.
        /// </summary>
        private void MergeIndistinguishable(int cliqueStart, int cliqueEnd)
        {
            // Candidates are found by hashing the neighbour list, so only nodes that landed
            // in the same bucket ever get compared entry by entry.
            for (int p = cliqueStart; p < cliqueEnd; p++)
            {
                int member = _list[p];
                if (_count[member] <= 0)
                    continue;
                int bucket = _hash[member];
                _hashNext[member] = _hashHead[bucket];
                _hashHead[bucket] = member;
            }

            for (int p = cliqueStart; p < cliqueEnd; p++)
            {
                int member = _list[p];
                if (_count[member] <= 0)
                    continue;
                int bucket = _hash[member];
                if (_hashHead[bucket] < 0)
                    continue;

                for (int a = _hashHead[bucket]; a >= 0; a = _hashNext[a])
                {
                    if (_count[a] <= 0)
                        continue;

                    if (_compareFlag > int.MaxValue - 2)
                    {
                        Array.Clear(_compare, 0, _size);
                        _compareFlag = 0;
                    }
                    _compareFlag++;
                    int endA = _start[a] + _length[a];
                    for (int q = _start[a]; q < endA; q++)
                        _compare[_list[q]] = _compareFlag;

                    for (int b = _hashNext[a]; b >= 0; b = _hashNext[b])
                    {
                        if (_count[b] <= 0 || _length[b] != _length[a] || _elements[b] != _elements[a])
                            continue;

                        bool identical = true;
                        int endB = _start[b] + _length[b];
                        for (int q = _start[b]; q < endB; q++)
                        {
                            if (_compare[_list[q]] != _compareFlag)
                            {
                                identical = false;
                                break;
                            }
                        }
                        if (!identical)
                            continue;

                        _count[a] += _count[b];
                        _count[b] = 0;
                        _state[b] = Dead;
                        _parent[b] = a;
                        _start[b] = -1;
                        _length[b] = 0;
                        _elements[b] = 0;
                    }
                }
                _hashHead[bucket] = -1;
            }
        }

        /// <summary>
        /// Turns the record of what absorbed what into the final ordering. Every original
        /// node is charged to the elimination step of the pivot that took it out, and the
        /// nodes are then grouped by that step.
        /// </summary>
        private void BuildPermutation(int[] permutation)
        {
            int n = _size;
            int steps = _steps;
            for (int i = 0; i < n; i++)
            {
                if (_step[i] >= 0)
                    continue;
                int node = i;
                while (_step[node] < 0)
                    node = _parent[node];
                int resolved = _step[node];

                // Write the answer back along the way so that no chain is walked twice.
                node = i;
                while (_step[node] < 0)
                {
                    int next = _parent[node];
                    _step[node] = resolved;
                    node = next;
                }
            }

            int[] offsets = new int[steps + 1];
            for (int i = 0; i < n; i++)
                offsets[_step[i] + 1]++;
            for (int s = 1; s <= steps; s++)
                offsets[s] += offsets[s - 1];
            for (int i = 0; i < n; i++)
                permutation[offsets[_step[i]]++] = i;
        }

        private void Absorb(int element, int into)
        {
            _state[element] = Dead;
            _parent[element] = into;
            _start[element] = -1;
            _length[element] = 0;
            _elements[element] = 0;
            _degree[element] = 0;
        }

        private void AddToDegreeList(int node, int degree)
        {
            int next = _degreeHead[degree];
            _degreeNext[node] = next;
            _degreePrevious[node] = -1;
            if (next >= 0)
                _degreePrevious[next] = node;
            _degreeHead[degree] = node;
        }

        private void RemoveFromDegreeList(int node, int degree)
        {
            int previous = _degreePrevious[node];
            int next = _degreeNext[node];
            if (previous >= 0)
                _degreeNext[previous] = next;
            else
                _degreeHead[degree] = next;
            if (next >= 0)
                _degreePrevious[next] = previous;
        }

        /// <summary>
        /// Makes room for the given number of entries at the end of the list storage, first
        /// by reclaiming the gaps that shrinking lists have left behind and then, if that is
        /// not enough, by growing the storage.
        /// </summary>
        private void EnsureSpace(int needed)
        {
            if (_free + needed <= _list.Length)
                return;
            Compact();
            if (_free + needed > _list.Length)
                Array.Resize(ref _list, Math.Max(2 * _list.Length, _free + needed));
        }

        /// <summary>
        /// Slides all live lists to the front of the storage, closing the gaps between them.
        /// Each list is found again by temporarily replacing its first entry with a marker
        /// that cannot be mistaken for a node index.
        /// </summary>
        private void Compact()
        {
            int n = _size;
            for (int i = 0; i < n; i++)
            {
                if (_start[i] < 0 || _length[i] <= 0)
                    continue;
                _saved[i] = _list[_start[i]];
                _list[_start[i]] = -(i + 1);
            }

            int read = 0, write = 0;
            while (read < _free)
            {
                int entry = _list[read++];
                if (entry >= 0)
                    continue;
                int owner = -entry - 1;
                _list[write] = _saved[owner];
                _start[owner] = write++;
                int rest = _length[owner] - 1;
                for (int k = 0; k < rest; k++)
                    _list[write++] = _list[read++];
            }
            _free = write;
        }
    }
}

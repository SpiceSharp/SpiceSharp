namespace SpiceSharp.Algebra.Solve;

/// <summary>
/// Finds the strongly connected components of the directed graph that a sparse matrix
/// describes once its nonzero diagonal has been established by a maximum transversal.
/// The components are the diagonal blocks of the block triangular form.
/// </summary>
/// <remarks>
/// This is Tarjan's algorithm, driven by an explicit stack so that a long path cannot
/// overflow the call stack. Tarjan emits a component only after every component
/// reachable from it has already been emitted, so numbering the components in the order
/// they come out puts all edges from higher-numbered to lower-or-equal-numbered blocks.
/// That is exactly what makes the permuted matrix block <em>upper</em> triangular.
/// </remarks>
public static class StronglyConnectedComponents
{
    /// <summary>
    /// Finds the strongly connected components of the graph in which column
    /// <c>j</c> has an edge to column <c>rowMatch[i]</c> for every row <c>i</c>
    /// that column <c>j</c> has a nonzero entry in.
    /// </summary>
    /// <param name="size">The number of rows and columns of the matrix.</param>
    /// <param name="columns">The column pointers, with <paramref name="size"/> + 1 elements.</param>
    /// <param name="rows">The row indices.</param>
    /// <param name="rowMatch">
    /// For every row, the column it is matched to. Every row has to be matched, so run
    /// this only after the transversal has been completed to a full permutation.
    /// </param>
    /// <param name="order">
    /// Receives the columns grouped per component, with at least <paramref name="size"/> elements.
    /// </param>
    /// <param name="boundaries">
    /// Receives the index into <paramref name="order"/> at which each component starts,
    /// terminated by <paramref name="size"/>. Needs at least <paramref name="size"/> + 1 elements.
    /// </param>
    /// <returns>The number of components found.</returns>
    public static int Find(int size, int[] columns, int[] rows, int[] rowMatch, int[] order, int[] boundaries)
    {
        int[] index = new int[size];
        int[] low = new int[size];
        bool[] onStack = new bool[size];
        int[] stackNode = new int[size];
        int[] stackEdge = new int[size];
        int[] component = new int[size];
        for (int i = 0; i < size; i++)
            index[i] = -1;

        int counter = 0, componentTop = 0, ordered = 0, blocks = 0;

        for (int root = 0; root < size; root++)
        {
            if (index[root] >= 0)
                continue;

            int head = 0;
            stackNode[0] = root;
            stackEdge[0] = columns[root];
            index[root] = low[root] = counter++;
            component[componentTop++] = root;
            onStack[root] = true;

            while (head >= 0)
            {
                int node = stackNode[head];
                int end = columns[node + 1];
                int p = stackEdge[head];
                bool descended = false;

                while (p < end)
                {
                    int next = rowMatch[rows[p]];
                    p++;
                    if (index[next] < 0)
                    {
                        // Unvisited: go deeper, remembering where to resume here.
                        stackEdge[head] = p;
                        head++;
                        stackNode[head] = next;
                        stackEdge[head] = columns[next];
                        index[next] = low[next] = counter++;
                        component[componentTop++] = next;
                        onStack[next] = true;
                        descended = true;
                        break;
                    }
                    if (onStack[next] && index[next] < low[node])
                        low[node] = index[next];
                }

                if (descended)
                    continue;
                stackEdge[head] = p;

                if (low[node] == index[node])
                {
                    // This node roots a component: everything above it on the component
                    // stack belongs to it.
                    boundaries[blocks++] = ordered;
                    int member;
                    do
                    {
                        member = component[--componentTop];
                        onStack[member] = false;
                        order[ordered++] = member;
                    }
                    while (member != node);
                }

                head--;
                if (head >= 0)
                {
                    int parent = stackNode[head];
                    if (low[node] < low[parent])
                        low[parent] = low[node];
                }
            }
        }

        boundaries[blocks] = size;
        return blocks;
    }
}

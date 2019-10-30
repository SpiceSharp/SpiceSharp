using System.Collections.Generic;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Biasing state for <see cref="SubcircuitSimulation"/>.
    /// </summary>
    /// <seealso cref="IBiasingSimulationState" />
    public class LoadBiasingState : BiasingSimulationState, IBiasingSimulationState
    {
        private List<ElementPair> _syncPairs = new List<ElementPair>();
        private List<ElementPair> _asyncPairs = new List<ElementPair>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBiasingState"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="parameters">The parameters for the state.</param>
        public LoadBiasingState(IBiasingSimulationState parent, ParameterSetDictionary parameters)
            : base(parent)
        {
        }

        /// <summary>
        /// Notifies the state that these variables can be shared with other states.
        /// </summary>
        /// <param name="common">The common.</param>
        public override void ShareVariables(HashSet<Variable> common)
        {
            Variable[] elts = new Variable[Map.Count];
            foreach (var elt in Map)
                elts[elt.Value] = elt.Key;

            _syncPairs.Clear();
            _asyncPairs.Clear();

            // Make pairs of elements for all solver elements
            LocalSolver.Precondition((m, v) =>
            {
                var matrix = (ISparseMatrix<double>)m;
                var vector = (ISparseVector<double>)v;

                // Make pairs for all elements
                for (var r = 1; r <= LocalSolver.Size; r++)
                {
                    var mElt = matrix.GetFirstInRow(r);
                    while (mElt != null)
                    {
                        // Get the row and column variables
                        var loc = new MatrixLocation(mElt.Row, mElt.Column);
                        loc = LocalSolver.InternalToExternal(loc);
                        var rowVariable = elts[loc.Row];
                        var colVariable = elts[loc.Column];

                        // Get the local element and the parent element
                        var localElt = (Element<double>)mElt;
                        var parentElt = Parent.Solver.GetElement(Parent.Map[rowVariable], Parent.Map[colVariable]);

                        if (common.Contains(rowVariable) || common.Contains(colVariable))
                            _syncPairs.Add(new ElementPair(localElt, parentElt));
                        else
                            _asyncPairs.Add(new ElementPair(localElt, parentElt));

                        // Go to the next element
                        mElt = mElt.Right;
                    }
                }

                var vElt = vector.GetFirstInVector();
                while (vElt != null)
                {
                    // Get the row variable
                    var loc = new MatrixLocation(vElt.Index, 1);
                    loc = LocalSolver.InternalToExternal(loc);
                    var rowVariable = elts[loc.Row];

                    // Get the local element and the parent element
                    var localElt = (Element<double>)vElt;
                    var parentElt = Parent.Solver.GetElement(Parent.Map[rowVariable]);

                    if (common.Contains(rowVariable))
                        _syncPairs.Add(new ElementPair(localElt, parentElt));
                    else
                        _asyncPairs.Add(new ElementPair(localElt, parentElt));
                }
            });
        }

        /// <summary>
        /// Resets the biasing state for loading the local behaviors.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            Solution = Parent.Solution;
            OldSolution = Parent.OldSolution;
        }

        /// <summary>
        /// Apply changes locally.
        /// </summary>
        /// <returns>
        /// True if the application was succesful.
        /// </returns>
        public override bool ApplyAsynchroneously()
        {
            foreach (var pair in _asyncPairs)
                pair.Parent.Add(pair.Local.Value);
            return true;
        }

        /// <summary>
        /// Apply changes to the parent biasing state.
        /// </summary>
        public override void ApplySynchroneously()
        {
            foreach (var pair in _syncPairs)
                pair.Parent.Add(pair.Local.Value);
        }

        /// <summary>
        /// Determines whether the state solution is convergent.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is convergent; otherwise, <c>false</c>.
        /// </returns>
        public override bool CheckConvergence()
        {
            // Nothing to do here
            return true;
        }
    }
}

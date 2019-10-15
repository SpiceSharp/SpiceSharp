using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Entities.ParallelBehaviors
{
    public class ParallelSolverElements<T> where T : IFormattable
    {
        protected class LocalElement
        {
            public Element<T> Parent { get; }
            public Element<T> Local { get; }
            public LocalElement(Element<T> parent, Element<T> local)
            {
                Parent = parent.ThrowIfNull(nameof(parent));
                Local = local.ThrowIfNull(nameof(local));
            }
        }

        public void Link(ISparseSolver<T> local, IVariableSet localVariables,
            ISparseSolver<T> other, IVariableSet otherVariables,
            ISparseSolver<T> parent, IVariableSet parentVariables)
        {

        }

        public void Link(ISparseSolver<T> local, IVariableSet localVariables,
            ISparseSolver<T> parent, IVariableSet parentVariables)
        {

        }

        public void ApplyElements()
        {

        }
    }
}

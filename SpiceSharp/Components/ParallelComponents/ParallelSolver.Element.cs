using SpiceSharp.Algebra;

namespace SpiceSharp.Components.ParallelComponents
{
    public partial class ParallelSolver<T>
    {
        /// <summary>
        /// A bridging element.
        /// </summary>
        /// <seealso cref="ISparseSolver{T}" />
        protected class BridgeElement : Element<T>
        {
            private readonly Element<T> _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="BridgeElement"/> class.
            /// </summary>
            /// <param name="parent">The parent element.</param>
            public BridgeElement(Element<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <summary>
            /// Applies the value to the parent element.
            /// </summary>
            public void Apply()
            {
                _parent.Add(Value);
            }
        }
    }
}

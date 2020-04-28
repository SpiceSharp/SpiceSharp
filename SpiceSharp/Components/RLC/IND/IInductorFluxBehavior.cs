using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Describes a behavior for an inductor that allows modification of its flux.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IBranchedBehavior{T}" />
    public interface IInductorFluxBehavior<T> :
        IBranchedBehavior<T>
    {
        /// <summary>
        /// Occurs when the flux through the inductor can be updated.
        /// </summary>
        /// <remarks>
        /// This event can be used by for example <see cref="MutualInductance"/>
        /// to couple two inductors.
        /// </remarks>
        event EventHandler<UpdateFluxEventArgs> UpdateFlux;
    }
}

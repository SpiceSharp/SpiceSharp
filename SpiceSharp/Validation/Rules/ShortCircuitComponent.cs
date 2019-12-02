using SpiceSharp.Components;
using System.Collections.Generic;

namespace SpiceSharp.Validation.Rules
{
    /// <summary>
    /// An <see cref="IValidationRule"/> that checks for a short-circuited <see cref="IComponent"/>.
    /// </summary>
    /// <seealso cref="IValidationRule" />
    public class ShortCircuitComponent : IValidationRule
    {
        /// <summary>
        /// Gets the problematic components.
        /// </summary>
        /// <value>
        /// The problems.
        /// </value>
        public List<IComponent> Problems { get; } = new List<IComponent>();

        /// <summary>
        /// Gets or sets the comparer.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer { get; set; }

        /// <summary>
        /// Sets up the validation rule.
        /// </summary>
        public void Setup()
        {
            Problems.Clear();
        }

        /// <summary>
        /// Applies the specified conn.
        /// </summary>
        /// <param name="component">The entity.</param>
        public void Connected(IComponent component)
        {
            if (component.PinCount <= 1)
                return;
            var reference = component.GetNode(0);
            for (var i = 1; i < component.PinCount; i++)
            {
                if (!Comparer.Equals(reference, component.GetNode(i)))
                    return;
            }
            Problems.Add(component);
        }

        /// <summary>
        /// Finish the check by validating the results.
        /// </summary>
        public void Validate()
        {
            // TODO: Throw exception, call an event, fix the issue?
        }

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        public ICloneable Clone()
        {
            var clone = new ShortCircuitComponent();
            foreach (var component in Problems)
                clone.Problems.Add(component); // No cloning: The validation keeps the reference to the actual offender.
            return clone;
        }

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        public void CopyFrom(ICloneable source)
        {
            Problems.Clear();
            var src = (ShortCircuitComponent)source;
            foreach (var component in src.Problems)
                Problems.Add(component);
        }
    }
}

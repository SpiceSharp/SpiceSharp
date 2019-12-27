using System;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Template for a behavior.
    /// </summary>
    public abstract class Behavior : Parameterized, IBehavior
    {
        /// <summary>
        /// Gets the name of the behavior.
        /// </summary>
        /// <remarks>
        /// This should be the same name as the entity that created it.
        /// </remarks>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Behavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <remarks>
        /// The name of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected Behavior(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public override P GetProperty<P>(string name)
        {
            if (base.TryGetProperty(name, out P result))
                return result;
            return Reflection.Get<P>(this, name);
        }

        /// <summary>
        /// Tries to get the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was found; otherwise <c>false</c>.
        /// </returns>
        public override bool TryGetProperty<P>(string name, out P value)
        {
            if (base.TryGetProperty(name, out value))
                return true;
            return Reflection.TryGet(this, name, out value);
        }

        /// <summary>
        /// Creates a getter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A getter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        public override Func<P> CreatePropertyGetter<P>(string name)
        {
            var getter = base.CreatePropertyGetter<P>(name);
            if (getter == null)
                getter = Reflection.CreateGetter<P>(this, name);
            return getter;
        }
    }
}

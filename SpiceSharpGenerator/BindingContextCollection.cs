using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// A class that can resolve binding contexts for entity types.
    /// </summary>
    public class BindingContextCollection
    {
#pragma warning disable RS1024 // Compare symbols correctly
        private readonly Dictionary<INamedTypeSymbol, INamedTypeSymbol> _map = new(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly

        /// <summary>
        /// Add a binding context.
        /// </summary>
        /// <param name="target">The target entity type.</param>
        /// <param name="context">The target context type.</param>
        public void Add(INamedTypeSymbol target, INamedTypeSymbol context)
            => _map.Add(target, context);

        /// <summary>
        /// Gets the binding context for an entity type.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>The associated binding context type.</returns>
        public INamedTypeSymbol GetBindingContext(INamedTypeSymbol entityType)
        {
            INamedTypeSymbol result = null;
            while (entityType != null && !_map.TryGetValue(entityType, out result))
                entityType = entityType.BaseType;
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Class that handles all behaviours
    /// </summary>
    public class Behaviors
    {
        /// <summary>
        /// Behaviors per component type
        /// </summary>
        public static readonly Dictionary<Tuple<Type, Type>, List<Type>> Defaults = new Dictionary<Tuple<Type, Type>, List<Type>>();

        /// <summary>
        /// Register a default behaviour
        /// </summary>
        /// <param name="componentType"></param>
        /// <param name="behaviourType"></param>
        public static void RegisterBehavior(Type componentType, Type behaviourType)
        {
            // Add an entry to the Defaults
            var key = new Tuple<Type, Type>(componentType, behaviourType.BaseType);
            if (!Defaults.ContainsKey(key))
                Defaults[key] = new List<Type>();

            Defaults[key].Add(behaviourType);
        }

        /// <summary>
        /// Create an enumerable of a certain behaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ckt">Circuit</param>
        /// <param name="behaviours">The behaviours that need to be imported</param>
        public static List<T> CreateBehaviors<T>(Circuit ckt) where T : CircuitObjectBehavior
        {
            List<T> result = new List<T>();

            foreach (var obj in ckt.Objects)
            {
                // Get all component behaviours
                var allBehaviors = GetAllBehaviorsForComponent<T>(obj);

                // Add all behaviours of the type we want
                foreach (var type in allBehaviors)
                {
                    var instance = (T)Activator.CreateInstance(type);
                    (instance as ICircuitObjectBehavior)?.Setup(obj, ckt);
                    result.Add(instance);
                }
            }

            return result;
        }

        /// <summary>
        /// Get all behaviours of a certain type of a certain circuit object
        /// </summary>
        /// <typeparam name="T">The behaviour</typeparam>
        /// <param name="obj">The object</param>
        /// <returns></returns>
        private static List<Type> GetAllBehaviorsForComponent<T>(ICircuitObject obj) where T : CircuitObjectBehavior
        {
            var result = new List<Type>();
            var key = new Tuple<Type, Type>(obj.GetType(), typeof(T));

            // Find the behaviour
            if (Defaults.TryGetValue(key, out List<Type> behaviours))
                result.AddRange(behaviours);
            return result;
        }
    }
}

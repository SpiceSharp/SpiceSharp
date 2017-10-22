using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharp.Behaviours
{
    /// <summary>
    /// Class that handles all behaviours
    /// </summary>
    public class Behaviours
    {
        /// <summary>
        /// Behaviours per component type
        /// </summary>
        public static readonly Dictionary<Tuple<Type, Type>, List<Type>> Defaults = new Dictionary<Tuple<Type, Type>, List<Type>>();

        /// <summary>
        /// Register a default behaviour
        /// </summary>
        /// <param name="componentType"></param>
        /// <param name="behaviourType"></param>
        public static void RegisterBehaviour(Type componentType, Type behaviourType)
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
        public static List<T> CreateBehaviours<T>(Circuit ckt) where T : CircuitObjectBehaviour
        {
            List<T> result = new List<T>();

            foreach (var obj in ckt.Objects)
            {
                // Skip objects that do not implement any behaviours
                if (!(obj is ICircuitComponentWithBehaviours))
                    continue;

                // Get all component behaviours
                var allBehaviours = GetAllBehavioursForComponent<T>(obj);

                // Add all behaviours of the type we want
                foreach (var type in allBehaviours)
                {
                    var instance = (T)Activator.CreateInstance(type);
                    (instance as ICircuitObjectBehaviour)?.Setup(obj, ckt);
                    result.Add(instance);
                }
            }

            return result;
        }

        /// <summary>
        /// Execute the behaviour
        /// NOTE: This will not be needed in the future!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="ckt"></param>
        public static void ExecuteBehaviour<T>(ICircuitObject obj, Circuit ckt) where T: class, ICircuitObjectBehaviour
        {
            var behaviour = (obj as ICircuitComponentWithBehaviours)?.CurrentBehaviours.FirstOrDefault(b => b is T);
            if (behaviour != null)
            {
                behaviour.Execute(ckt);
            }
            else
            {
                // TODO: to remove someday when all Load, AcLoad, Noise will be moved to behaviours
                if (typeof(T).Name == "CircuitObjectBehaviorAcLoad")
                {
                    obj.GetType().GetMethod("AcLoad")?.Invoke(obj, new object[] {ckt});
                }
                if (typeof(T).Name == "CircuitObjectBehaviorDcLoad")
                {
                    obj.GetType().GetMethod("Load")?.Invoke(obj, new object[] { ckt });
                }

                if (typeof(T).Name == "CircuitObjectBehaviorNoiseLoad")
                {
                    obj.GetType().GetMethod("Noise")?.Invoke(obj, new object[] { ckt });
                }
            }
        }

        /// <summary>
        /// Get all behaviours of a certain type of a certain circuit object
        /// </summary>
        /// <typeparam name="T">The behaviour</typeparam>
        /// <param name="obj">The object</param>
        /// <returns></returns>
        private static List<Type> GetAllBehavioursForComponent<T>(ICircuitObject obj) where T : CircuitObjectBehaviour
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

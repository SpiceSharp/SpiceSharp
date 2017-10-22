using SpiceSharp.Behaviours.AcLoad;
using SpiceSharp.Behaviours.DcLoad;
using SpiceSharp.Behaviours.Noise;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Behaviours
{
    public class Behaviours
    {
        public static readonly Dictionary<string, List<Type>> Defaults = new Dictionary<string, List<Type>>();

        static Behaviours()
        {
            RegisterDefaults();
        }

        private static void RegisterDefaults()
        {
            RegisterBehaviourMapping(typeof(Resistor), typeof(ResistorLoadAcBehaviour));
            RegisterBehaviourMapping(typeof(Resistor), typeof(ResistorLoadDcBehaviour));
            RegisterBehaviourMapping(typeof(Resistor), typeof(ResistorNoiseBehaviour));

            RegisterBehaviourMapping(typeof(Diode), typeof(DiodeLoadAcBehaviour));
            RegisterBehaviourMapping(typeof(Diode), typeof(DiodeLoadDcBehaviour));
            RegisterBehaviourMapping(typeof(Diode), typeof(DiodeNoiseBehaviour));

            RegisterBehaviourMapping(typeof(Voltagesource), typeof(VoltageSourceLoadAcBehaviour));
            RegisterBehaviourMapping(typeof(Voltagesource), typeof(VoltagesourceLoadDcBehaviour));

            RegisterBehaviourMapping(typeof(Currentsource), typeof(CurrentSourceLoadAcBehaviour));
            RegisterBehaviourMapping(typeof(Currentsource), typeof(CurrentSourceLoadDcBehaviour));
        }

        public static void RegisterBehaviourMapping(Type componentType, Type behaviourType)
        {
            Type behaviourBaseType = behaviourType.BaseType;

            string key = $"{behaviourBaseType}_{componentType}";
            if (!Defaults.ContainsKey(key))
            {
                Defaults[key] = new List<Type>();
            }

            Defaults[key].Add(behaviourType);
        }

        public static void CreateBehaviours<T>(Circuit ckt, params T[] behaviourBaseTypes) 
        {
            foreach (var obj in ckt.Objects)
            {
                if (!(obj is ICircuitComponentWithBehaviours))
                {
                    continue;
                }

                var allBehaviours = GetAllBehavioursForComponent(obj, behaviourBaseTypes);
                var objects = new List<ICircuitObjectBehaviour>();

                foreach (var type in allBehaviours)
                {
                    var instance = (ICircuitObjectBehaviour) Activator.CreateInstance(type);
                    instance.Setup(obj, ckt);
                    objects.Add(instance);
                }
                ((ICircuitComponentWithBehaviours) obj).CurrentBehaviours = objects.ToArray();
            }
        }

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

        private static IEnumerable<Type> GetAllBehavioursForComponent<T>(ICircuitObject obj, params T[] behaviourBaseTypes)
        {
            var result = new List<Type>();
            foreach (var behaviourBaseType in behaviourBaseTypes)
            {
                string key = $"{behaviourBaseType}_{obj.GetType()}";
                result.AddRange(Defaults.ContainsKey(key) ? Defaults[key] : new List<Type>());
            }
            return result;
        }
    }
}

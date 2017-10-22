using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviours
{
    public static class CircuitObjectLoadBehaviours
    {
        public static void AcLoad(this ICircuitObject obj, Circuit ckt)
        {
            Behaviours.ExecuteBehaviour<CircuitObjectBehaviorAcLoad>(obj, ckt);
        }

        public static void DcLoad(this ICircuitObject obj, Circuit ckt)
        {
            Behaviours.ExecuteBehaviour<CircuitObjectBehaviorDcLoad>(obj, ckt);
        }
    }
}

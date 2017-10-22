using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviours
{
    public static class CircuitObjectNoiseBehaviours
    {
        public static void Noise(this ICircuitObject obj, Circuit ckt)
        {
            Behaviours.ExecuteBehaviour<CircuitObjectBehaviourNoise>(obj, ckt);
        }
    }
}

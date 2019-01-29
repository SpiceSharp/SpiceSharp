using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CurrentSourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent current source
    /// </summary>
    [Pin(0, "I+"), Pin(1, "I-"), IndependentSource, Connected]
    public class CurrentSource : Component
    {
        static CurrentSource()
        {
            RegisterBehaviorFactory(typeof(CurrentSource), new BehaviorFactoryDictionary
            {
                {typeof(BiasingBehavior), n => new BiasingBehavior(n)},
                {typeof(FrequencyBehavior), n => new FrequencyBehavior(n)},
                {typeof(AcceptBehavior), n => new AcceptBehavior(n)}
            });
        }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CurrentSourcePinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        public CurrentSource(string name) 
            : base(name, CurrentSourcePinCount)
        {
            // Add parameters
            ParameterSets.Add(new CommonBehaviors.IndependentBaseParameters());
            ParameterSets.Add(new CommonBehaviors.IndependentFrequencyParameters());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public CurrentSource(string name, string pos, string neg, double dc)
            : this(name)
        {
            // Add parameters
            ParameterSets.Add(new CommonBehaviors.IndependentBaseParameters(dc));
            ParameterSets.Add(new CommonBehaviors.IndependentFrequencyParameters());

            // Connect
            Connect(pos, neg);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="waveform">The Waveform-object</param>
        public CurrentSource(string name, string pos, string neg, Waveform waveform)
            : this(name)
        {
            // Add parameters
            ParameterSets.Add(new CommonBehaviors.IndependentBaseParameters(waveform));
            ParameterSets.Add(new CommonBehaviors.IndependentFrequencyParameters());

            // Connect
            Connect(pos, neg);
        }
    }
}

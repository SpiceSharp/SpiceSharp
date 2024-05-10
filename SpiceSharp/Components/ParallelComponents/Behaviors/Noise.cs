using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Linq;
using System;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="INoiseBehavior"/> for a <see cref="Parallel"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="INoiseBehavior" />
    public partial class Noise : Behavior,
        IParallelBehavior,
        INoiseBehavior
    {
        private readonly Workload _noiseInitializeWorkload, _noiseLoadWorkload, _noiseComputeWorkload;
        private BehaviorList<INoiseBehavior> _noiseBehaviors;

        /// <inheritdoc/>
        public double OutputNoiseDensity => _noiseBehaviors.Sum(nb => nb.OutputNoiseDensity);

        /// <inheritdoc/>
        public double TotalOutputNoise => _noiseBehaviors.Sum(nb => nb.TotalOutputNoise);

        /// <inheritdoc/>
        public double TotalInputNoise => _noiseBehaviors.Sum(nb => nb.TotalInputNoise);

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Noise(ParallelBindingContext context)
            : base(context)
        {
            var parameters = context.GetParameterSet<Parameters>();
            if (parameters.WorkDistributors.TryGetValue(typeof(INoiseBehavior), out var dist) && dist != null)
            {
                _noiseInitializeWorkload = new Workload(dist, parameters.Entities.Count);
                _noiseLoadWorkload = new Workload(dist, parameters.Entities.Count);
                _noiseComputeWorkload = new Workload(dist, parameters.Entities.Count);
                if (context.TryGetState(out INoiseSimulationState parent))
                    context.AddLocalState<INoiseSimulationState>(new NoiseSimulationState(parent));
            }
        }

        /// <inheritdoc/>
        public virtual void FetchBehaviors(ParallelBindingContext context)
        {
            _noiseBehaviors = context.GetBehaviors<INoiseBehavior>();
            if (_noiseInitializeWorkload != null)
            {
                foreach (var behavior in _noiseBehaviors)
                    _noiseInitializeWorkload.Actions.Add(behavior.Initialize);
            }
            if (_noiseLoadWorkload != null)
            {
                foreach (var behavior in _noiseBehaviors)
                    _noiseLoadWorkload.Actions.Add(behavior.Load);
            }
            if (_noiseComputeWorkload != null)
            {
                foreach (var behavior in _noiseBehaviors)
                    _noiseComputeWorkload.Actions.Add(behavior.Compute);
            }
        }

        /// <inheritdoc/>
        void INoiseSource.Initialize()
        {
            if (_noiseInitializeWorkload != null)
                _noiseInitializeWorkload.Execute();
            else
            {
                foreach (var behavior in _noiseBehaviors)
                    behavior.Initialize();
            }
        }

        /// <inheritdoc />
        void INoiseBehavior.Load()
        {
            if (_noiseLoadWorkload != null)
                _noiseLoadWorkload.Execute();
            else
            {
                foreach (var behavior in _noiseBehaviors)
                    behavior.Load();
            }
        }

        /// <inheritdoc/>
        void INoiseBehavior.Compute()
        {
            if (_noiseComputeWorkload != null)
                _noiseComputeWorkload.Execute();
            else
            {
                foreach (var behavior in _noiseBehaviors)
                    behavior.Compute();
            }
        }
    }
}

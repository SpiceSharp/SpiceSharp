using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components.SamplerBehaviors
{
    /// <summary>
    /// Time-dependent behavior for a <see cref="Sampler"/>.
    /// </summary>
    /// <remarks>
    /// This behavior does strictly speaking not need to implement <see cref="ITimeBehavior"/>, however if
    /// we don't then will also create the more generic sampler behavior.
    /// </remarks>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITimeBehavior"/>
    /// <seealso cref="ITruncatingBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    [BehaviorFor(typeof(Sampler)), AddBehaviorIfNo(typeof(ITruncatingBehavior))]
    public class Truncating : Behavior,
        ITimeBehavior,
        ITruncatingBehavior,
        IParameterized<Parameters>
    {
        private readonly IIntegrationMethod _method;
        private readonly IEnumerator<double> _points;
        private bool _continue;

        /// <inheritdoc />
        public Parameters Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Truncating"/> class.
        /// </summary>
        /// <param name="context">The sampler binding context.</param>
        public Truncating(SamplerBindingContext context)
            : base(context)
        {
            Parameters = context.GetParameterSet<Parameters>();
            _method = context.GetState<IIntegrationMethod>();
            _points = Parameters.Points.GetEnumerator();
            _continue = _points.MoveNext();

            // Register to the export event
            context.RegisterToExportEvent(OnExportSimulationData);

            // Find the first timepoint that is eligible for targeting
            while (_continue && _points.Current < -Parameters.MinDelta)
                _continue = _points.MoveNext();
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
        }

        /// <summary>
        /// The method that is called whenever the simulation is ready to export data.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        private void OnExportSimulationData(object sender, ExportDataEventArgs args)
        {
            // Nothing left...
            if (!_continue)
                return;

            // If time has caught up with the currently targeted point, let's export it ourselves!
            if (_method.Time > _points.Current - Parameters.MinDelta)
            {
                // Pass it through
                Parameters.Export(sender, args);

                // Find the next point that is eligible for targetting
                while (_continue && _points.Current < _method.Time + Parameters.MinDelta)
                    _continue = _points.MoveNext();
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// For our sampler, we don't care about truncation errors.
        /// </remarks>
        double ITruncatingBehavior.Evaluate() => double.PositiveInfinity;

        /// <inheritdoc/>
        /// <remarks>
        /// Here is where we have a soft-limit to our next timepoint.
        /// </remarks>
        double ITruncatingBehavior.Prepare() => _continue ? _points.Current - _method.Time : double.PositiveInfinity;
    }
}

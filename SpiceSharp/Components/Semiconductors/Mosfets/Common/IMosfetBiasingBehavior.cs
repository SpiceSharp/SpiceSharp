using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// A biasing behavior for a mosfet.
    /// </summary>
    public interface IMosfetBiasingBehavior : IBehavior,
        IParameterized<Parameters>
    {
        /// <summary>
        /// Gets the temperature-dependent properties.
        /// </summary>
        /// <value>
        /// The temperature-dependent properties.
        /// </value>
        TemperatureProperties Properties { get; }

        /// <summary>
        /// Occurs when the contributions can be updated.
        /// </summary>
        event EventHandler<MosfetContributionEventArgs> UpdateContributions;

        /// <include file='docs.xml' path='docs/members/DrainCurrent/*'/>
        [ParameterName("id"), ParameterName("cd"), ParameterInfo("Drain current")]
        double Id { get; }

        /// <include file='docs.xml' path='docs/members/BulkSourceCurrent/*'/>
        [ParameterName("ibs"), ParameterInfo("B-S junction current")]
        double Ibs { get; }

        /// <include file='docs.xml' path='docs/members/BulkDrainCurrent/*'/>
        [ParameterName("ibd"), ParameterInfo("B-D junction current")]
        double Ibd { get; }

        /// <include file='docs.xml' path='docs/members/Transconductance/*'/>
        [ParameterName("gm"), ParameterInfo("Transconductance")]
        double Gm { get; }

        /// <include file='docs.xml' path='docs/members/BulkSourceTransconductance/*'/>
        [ParameterName("gmb"), ParameterName("gmbs"), ParameterInfo("Bulk-Source transconductance")]
        double Gmbs { get; }

        /// <include file='docs.xml' path='docs/members/DrainSourceConductance/*'/>
        [ParameterName("gds"), ParameterInfo("Drain-Source conductance")]
        double Gds { get; }

        /// <include file='docs.xml' path='docs/members/BulkSourceConductance/*'/>
        [ParameterName("gbs"), ParameterInfo("Bulk-Source conductance")]
        double Gbs { get; }

        /// <include file='docs.xml' path='docs/members/BulkDrainConductance/*'/>
        [ParameterName("gbd"), ParameterInfo("Bulk-Drain conductance")]
        double Gbd { get; }

        /// <include file='docs.xml' path='docs/members/von/*'/>
        [ParameterName("von"), ParameterInfo("Turn-on voltage")]
        double Von { get; }

        /// <include file='docs.xml' path='docs/members/SaturationVoltage/*'/>
        [ParameterName("vdsat"), ParameterInfo("Saturation drain-source voltage")]
        double Vdsat { get; }

        /// <include file='docs.xml' path='docs/members/Mode/*'/>
        double Mode { get; }

        /// <include file='docs.xml' path='docs/members/GateSourceVoltage/*'/>
        [ParameterName("vgs"), ParameterInfo("Gate-Source voltage")]
        double Vgs { get; }

        /// <include file='docs.xml' path='docs/members/DrainSourceVoltage/*'/>
        [ParameterName("vds"), ParameterInfo("Drain-Source voltage")]
        double Vds { get; }

        /// <include file='docs.xml' path='docs/members/BulkSourceVoltage/*'/>
        [ParameterName("vbs"), ParameterInfo("Bulk-Source voltage")]
        double Vbs { get; }

        /// <include file='docs.xml' path='docs/members/BulkDrainVoltage/*'/>
        [ParameterName("vbd"), ParameterInfo("Bulk-Drain voltage")]
        double Vbd { get; }
    }
}

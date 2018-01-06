﻿using SpiceSharp.Components.VCVS;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.VCVS
{
    /// <summary>
    /// AC behavior for a <see cref="Components.VoltageControlledVoltagesource"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        LoadBehavior load;

        /// <summary>
        /// Nodes
        /// </summary>
        int VCVSposNode, VCVSnegNode, VCVScontPosNode, VCVScontNegNode, VCVSbranch;
        protected MatrixElement VCVSposIbrptr { get; private set; }
        protected MatrixElement VCVSnegIbrptr { get; private set; }
        protected MatrixElement VCVSibrPosptr { get; private set; }
        protected MatrixElement VCVSibrNegptr { get; private set; }
        protected MatrixElement VCVSibrContPosptr { get; private set; }
        protected MatrixElement VCVSibrContNegptr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>();
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            VCVSposNode = pins[0];
            VCVSnegNode = pins[1];
            VCVScontPosNode = pins[2];
            VCVScontNegNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            VCVSbranch = load.VCVSbranch;
            VCVSposIbrptr = matrix.GetElement(VCVSposNode, VCVSbranch);
            VCVSnegIbrptr = matrix.GetElement(VCVSnegNode, VCVSbranch);
            VCVSibrPosptr = matrix.GetElement(VCVSbranch, VCVSposNode);
            VCVSibrNegptr = matrix.GetElement(VCVSbranch, VCVSnegNode);
            VCVSibrContPosptr = matrix.GetElement(VCVSbranch, VCVScontPosNode);
            VCVSibrContNegptr = matrix.GetElement(VCVSbranch, VCVScontNegNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            VCVSposIbrptr = null;
            VCVSnegIbrptr = null;
            VCVSibrPosptr = null;
            VCVSibrNegptr = null;
            VCVSibrContPosptr = null;
            VCVSibrContNegptr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            VCVSposIbrptr.Add(1.0);
            VCVSibrPosptr.Add(1.0);
            VCVSnegIbrptr.Sub(1.0);
            VCVSibrNegptr.Sub(1.0);
            VCVSibrContPosptr.Sub(bp.VCVScoeff);
            VCVSibrContNegptr.Add(bp.VCVScoeff);
        }
    }
}

﻿using SpiceSharp.Components.CAP;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.CAP
{
    /// <summary>
    /// AC behavior for <see cref="Components.Capacitor"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary paramters and behaviors
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Nodes
        /// </summary>
        int CAPposNode, CAPnegNode;
        MatrixElement CAPposPosptr;
        MatrixElement CAPnegNegptr;
        MatrixElement CAPposNegptr;
        MatrixElement CAPnegPosptr;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="pool">Behaviors</param>
        public override void Setup(ParametersCollection parameters, BehaviorPool pool)
        {
            bp = parameters.Get<BaseParameters>();
        }

        /// <summary>
        /// Connect behavior
        /// </summary>
        /// <param name="pins"></param>
        public void Connect(params int[] pins)
        {
            CAPposNode = pins[0];
            CAPnegNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">The matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            CAPposPosptr = matrix.GetElement(CAPposNode, CAPposNode);
            CAPnegNegptr = matrix.GetElement(CAPnegNode, CAPnegNode);
            CAPnegPosptr = matrix.GetElement(CAPnegNode, CAPposNode);
            CAPposNegptr = matrix.GetElement(CAPposNode, CAPnegNode);
        }
        
        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var cstate = ckt.State;
            var val = cstate.Laplace * bp.CAPcapac.Value;

            // Load the matrix
            CAPposPosptr.Add(val);
            CAPnegNegptr.Add(val);
            CAPposNegptr.Sub(val);
            CAPnegPosptr.Sub(val);
        }
    }
}

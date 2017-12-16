﻿using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;
using SpiceSharp.Components;

namespace SpiceSharp.Behaviors.RES
{
    /// <summary>
    /// General behaviour for <see cref="Resistor"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("resistance"), SpiceInfo("Resistance", IsPrincipal = true)]
        public Parameter RESresist { get; } = new Parameter();
        [SpiceName("i"), SpiceInfo("Current")]
        public double GetCurrent(Circuit ckt)
        {
            return (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) * RESconduct;
        }
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt)
        {
            return (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) *
                (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) * RESconduct;
        }

        /// <summary>
        /// Nodes
        /// </summary>
        public int RESposNode { get; private set; }
        public int RESnegNode { get; private set; }

        /// <summary>
        /// Conductance
        /// </summary>
        public double RESconduct { get; protected set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement RESposPosPtr { get; private set; }
        protected MatrixElement RESnegNegPtr { get; private set; }
        protected MatrixElement RESposNegPtr { get; private set; }
        protected MatrixElement RESnegPosPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public LoadBehavior()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="res">Resistance</param>
        public LoadBehavior(double res)
        {
            RESresist.Set(res);
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            var res = component as Resistor;

            // If the resistance is not given, get the default from the model
            if (!RESresist.Given)
            {
                var temp = GetBehavior<TemperatureBehavior>(component);
                RESresist.Value = temp.RESresist;
                RESconduct = temp.RESconduct;
            }
            else
            {
                if (RESresist.Value == 0.0)
                    RESconduct = 1e12;
                else
                    RESconduct = 1.0 / RESresist.Value;
            }

            // Nodes
            RESposNode = res.RESposNode;
            RESnegNode = res.RESnegNode;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            RESposPosPtr = matrix.GetElement(RESposNode, RESposNode);
            RESnegNegPtr = matrix.GetElement(RESnegNode, RESnegNode);
            RESposNegPtr = matrix.GetElement(RESposNode, RESnegNode);
            RESnegPosPtr = matrix.GetElement(RESnegNode, RESposNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup()
        {
            // Remove references
            RESposPosPtr = null;
            RESnegNegPtr = null;
            RESposNegPtr = null;
            RESnegPosPtr = null;
        }

        /// <summary>
        /// Perform calculations
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            RESposPosPtr.Add(RESconduct);
            RESnegNegPtr.Add(RESconduct);
            RESposNegPtr.Sub(RESconduct);
            RESnegPosPtr.Sub(RESconduct);
        }
    }
}
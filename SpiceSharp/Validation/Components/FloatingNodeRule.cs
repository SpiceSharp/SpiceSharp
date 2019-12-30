using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IConductiveRule"/> that checks for the presence of a floating node.
    /// </summary>
    /// <seealso cref="IConductiveRule" />
    public partial class FloatingNodeRule : IConductiveRule
    {
        private readonly Graph _graph = new Graph();

        /// <summary>
        /// Gets or sets the fixed-potential node.
        /// </summary>
        /// <value>
        /// The fixed-potential node.
        /// </value>
        public Variable FixedVariable { get; }

        /// <summary>
        /// Gets the number of violations of this rule.
        /// </summary>
        /// <value>
        /// The violation count.
        /// </value>
        public virtual int ViolationCount => _graph.Count - 1;

        /// <summary>
        /// Gets the violations.
        /// </summary>
        /// <value>
        /// The violations.
        /// </value>
        public virtual IEnumerable<IRuleViolation> Violations
        {
            get
            {
                bool skipOne = FixedVariable == null;
                foreach (var group in _graph.Groups)
                {
                    if (skipOne)
                    {
                        skipOne = false;
                        continue;
                    }
                    if (!group.Contains(FixedVariable))
                        yield return new FloatingNodeRuleViolation(this, group);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatingNodeRule"/> class.
        /// </summary>
        public FloatingNodeRule()
        {
            FixedVariable = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatingNodeRule"/> class.
        /// </summary>
        /// <param name="fixedVariable">The fixed-potential variable.</param>
        public FloatingNodeRule(Variable fixedVariable)
        {
            FixedVariable = fixedVariable;
        }

        /// <summary>
        /// Determines whether this rule encountered the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the specified variable was encountered; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Variable variable) => _graph.Contains(variable);

        /// <summary>
        /// Applies the specified variables as being connected by a conductive path.
        /// </summary>
        /// <param name="subject">The rule subject.</param>
        /// <param name="variables">The variables.</param>
        public void AddPath(IRuleSubject subject, params Variable[] variables)
        {
            if (variables == null || variables.Length == 0)
                return;
            if (variables.Length == 1)
                _graph.Add(variables[0]);
            else
            {
                for (var i = 0; i < variables.Length; i++)
                {
                    for (var j = i + 1; j < variables.Length; j++)
                        _graph.Connect(variables[i], variables[j], ConductionTypes.All);
                }
            }
        }

        /// <summary>
        /// Specifies variables as being connected by a conductive path of the specified type.
        /// </summary>
        /// <param name="subject">The subject that applies the conductive paths.</param>
        /// <param name="type">The type of path between these variables.</param>
        /// <param name="variables">The variables that are connected.</param>
        public void AddPath(IRuleSubject subject, ConductionTypes type, params Variable[] variables)
        {
            if (variables == null || variables.Length == 0)
                return;
            if (variables.Length == 1)
                _graph.Add(variables[0]);
            else
            {
                for (var i = 0; i < variables.Length; i++)
                {
                    for (var j = i + 1; j < variables.Length; j++)
                        _graph.Connect(variables[i], variables[j], type);
                }
            }
        }
    }
}

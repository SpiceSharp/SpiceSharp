namespace SpiceSharp
{
    /// <summary>
    /// A base template for a parameter that can be used in parameter sets
    /// Classes not inheriting from this class might fail to be cloned correctly.
    /// </summary>
    public abstract class BaseParameter
    {
        /// <summary>
        /// Clone the parameter
        /// </summary>
        /// <returns></returns>
        public abstract BaseParameter Clone();

        /// <summary>
        /// Copy the parameter to this parameter
        /// </summary>
        /// <param name="source">Source parameter</param>
        public abstract void CopyFrom(BaseParameter source);
    }
}

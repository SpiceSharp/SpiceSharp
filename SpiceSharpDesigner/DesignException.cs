using System;

namespace SpiceSharp.Designer
{
    public class DesignException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg">The message</param>
        public DesignException(string msg)
            : base(msg)
        {
        }
    }
}

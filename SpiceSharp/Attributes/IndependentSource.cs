using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Components
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IndependentSource : Attribute
    {
    }
}

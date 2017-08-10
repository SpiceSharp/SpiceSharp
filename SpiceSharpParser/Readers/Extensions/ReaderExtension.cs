using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Parameters;
using SpiceSharp.Components;
using static SpiceSharp.Parser.SpiceSharpParserConstants;

namespace SpiceSharp.Parser.Readers.Extensions
{
    /// <summary>
    /// Some extension methods for Token-related objects
    /// </summary>
    public static class ReaderExtension
    {
        /// <summary>
        /// Get the starting line of a Token-related objects
        /// </summary>
        /// <param name="o">The object</param>
        /// <returns></returns>
        public static int BeginLine(this object o)
        {
            if (o is Token)
                return (o as Token).beginLine;
            if (o is Token[])
            {
                Token[] ts = (Token[])o;
                if (ts != null && ts.Length > 0)
                    return ts[0].beginLine;
            }
            if (o is AssignmentToken)
                return (o as AssignmentToken).Name.BeginLine();
            if (o is BracketToken)
                return (o as BracketToken).Name.BeginLine();
            if (o is AtToken)
                return (o as AtToken).Name.BeginLine();
            return -1;
        }

        /// <summary>
        /// Get the ending line of Token-related objects
        /// </summary>
        /// <param name="o">The object</param>
        /// <returns></returns>
        public static int EndLine(this object o)
        {
            if (o is Token)
                return (o as Token).endLine;
            if (o is Token[])
            {
                Token[] ts = (Token[])o;
                if (ts != null && ts.Length > 0)
                    return ts[ts.Length - 1].endLine;
            }
            if (o is AssignmentToken)
                return (o as AssignmentToken).Value.EndLine();
            if (o is BracketToken)
            {
                var b = o as BracketToken;
                if (b.Parameters.Count == 0)
                    return b.Name.EndLine();
                else
                    return b.Parameters.Last().EndLine();
            }
            if (o is AtToken)
                return (o as AtToken).Parameter.EndLine();
            return -1;
        }

        /// <summary>
        /// Get starting column of Token-related objects
        /// </summary>
        /// <param name="o">The object</param>
        /// <returns></returns>
        public static int BeginColumn(this object o)
        {
            if (o is Token)
                return (o as Token).beginColumn;
            if (o is Token[])
            {
                Token[] ts = (Token[])o;
                if (ts != null && ts.Length > 0)
                    return ts[0].beginColumn;
            }
            if (o is AssignmentToken)
                return (o as AssignmentToken).Name.BeginColumn();
            if (o is BracketToken)
                return (o as BracketToken).Name.BeginColumn();
            if (o is AtToken)
                return (o as AtToken).Name.BeginColumn();
            return -1;
        }

        /// <summary>
        /// Get ending column of Token-related objects
        /// </summary>
        /// <param name="o">The object</param>
        /// <returns></returns>
        public static int EndColumn(this object o)
        {
            if (o is Token)
                return (o as Token).endColumn;
            if (o is Token[])
            {
                Token[] ts = (Token[])o;
                if (ts != null && ts.Length > 0)
                    return ts[ts.Length - 1].endColumn;
            }
            if (o is AssignmentToken)
                return (o as AssignmentToken).Value.EndColumn();
            if (o is BracketToken)
            {
                var b = o as BracketToken;
                if (b.Parameters.Count == 0)
                    return b.Name.EndColumn();
                else
                    return b.Parameters.Last().EndColumn();
            }
            if (o is AtToken)
                return (o as AtToken).Parameter.EndColumn();
            return -1;
        }

        /// <summary>
        /// Get the image of a Token-related object
        /// </summary>
        /// <param name="o">The object</param>
        /// <returns></returns>
        public static string Image(this object o)
        {
            if (o is Token)
                return (o as Token).image;
            if (o is Token[])
            {
                var ts = o as Token[];
                string[] ps = new string[ts.Length];
                for (int i = 0; i < ps.Length; i++)
                    ps[i] = ts[i].Image();
                return string.Join(", ", ps);
            }
            if (o is AssignmentToken)
            {
                var n = o as AssignmentToken;
                return n.Name.Image() + " = " + n.Value.Image();
            }
            if (o is BracketToken)
            {
                var b = o as BracketToken;
                return b.Name.Image() + "()";
            }
            if (o is AtToken)
            {
                var at = o as AtToken;
                return "@" + at.Name.Image() + "[" + at.Parameter.Image() + "]";
            }
            return "";
        }

        /// <summary>
        /// Try to read a word from a Token-related object
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="value">The value output</param>
        /// <returns></returns>
        public static bool TryReadWord(this object o, out string value)
        {
            if (o is Token)
            {
                Token t = o as Token;

                // Read a word
                if (t.kind == WORD)
                {
                    value = t.image.ToLower();
                    return true;
                }
            }

            // Failed
            value = null;
            return false;
        }

        /// <summary>
        /// Try to read a value from a Token-related object
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="value">The output value</param>
        /// <returns></returns>
        public static bool TryReadValue(this object o, out string value)
        {
            // Standard input as a token
            if (o is Token)
            {
                Token t = o as Token;

                // Read a simple value
                if (t.kind == VALUE)
                {
                    value = t.image.ToLower();
                    return true;
                }

                // Read an expression
                if (t.kind == EXPRESSION)
                {
                    value = t.image.Substring(1, t.image.Length - 2).ToLower();
                    return true;
                }
            }

            // Failed
            value = null;
            return false;
        }

        /// <summary>
        /// Try to read an identifier from a Token-related object
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="value">The value</param>
        /// <returns></returns>
        public static bool TryReadIdentifier(this object o, out string value)
        {
            if (o is Token)
            {
                Token t = o as Token;

                // Read a word or identifier
                if (t.kind == WORD || t.kind == IDENTIFIER)
                {
                    value = t.image.ToLower();
                    return true;
                }

                // Read a value if it is pure digits
                if (t.kind == VALUE && IsNumber(t.image))
                {
                    value = t.image; // .ToLower(); Not necessary here
                    return true;
                }
            }

            // Failed
            value = null;
            return false;
        }

        /// <summary>
        /// Try to read an assigned variable from a Token-related object
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="name">The name output</param>
        /// <param name="value">The value output</param>
        /// <returns></returns>
        public static bool TryReadAssignment(this object o, out object name, out object value)
        {
            if (o is AssignmentToken)
            {
                var p = o as AssignmentToken;
                name = p.Name;
                value = p.Value;
                return true;
            }

            // Failed
            name = null;
            value = null;
            return false;
        }

        /// <summary>
        /// Try to read an assigned variable from a Token-related object
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="name">The name output</param>
        /// <param name="value">The value output</param>
        /// <returns></returns>
        public static bool TryReadAssignment(this object o, out string name, out string value)
        {
            if (o is AssignmentToken)
            {
                var p = o as AssignmentToken;
                if (p.Name.TryReadWord(out name))
                {
                    if (!p.Value.TryReadValue(out value))
                        value = p.Value.Image().ToLower();
                    return true;
                }
            }

            // Failed
            name = null;
            value = null;
            return false;
        }

        /// <summary>
        /// Try to read a (quoted) string from a Token-related object
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="value">The value</param>
        /// <returns></returns>
        public static bool TryReadString(this object o, out string value)
        {
            if (o is Token)
            {
                Token t = o as Token;

                if (t.kind == STRING)
                {
                    value = t.image; // We'll keep this case sensitive, you never know what it's used for
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Try to read a reference name from a Token-related object
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="value">The value</param>
        /// <returns></returns>
        public static bool TryReadReference(this object o, out string name)
        {
            name = null;
            if (o is Token)
            {
                Token t = o as Token;
                if (t.kind == REFERENCE)
                {
                    name = t.image.Substring(1).ToLower();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Read a literal (case insensitive)
        /// </summary>
        /// <param name="o"></param>
        /// <param name="lit"></param>
        /// <returns></returns>
        public static bool TryReadLiteral(this object o, string lit)
        {
            if (o is Token)
                return lit.ToLower() == (o as Token).image.ToLower();
            if (o is Token[])
            {
                Token[] ts = o as Token[];
                string[] tokens = new string[ts.Length];
                for (int i = 0; i < ts.Length; i++)
                    tokens[i] = ts[i].Image().ToLower();
                return lit.ToLower() == string.Join(",", tokens);
            }
            if (o is AssignmentToken)
            {
                AssignmentToken at = o as AssignmentToken;
                return lit.ToLower() == at.Name.Image().ToLower() + "=" + at.Value.Image().ToLower();
            }
            if (o is BracketToken)
            {
                BracketToken bt = o as BracketToken;
                string[] tokens = new string[bt.Parameters.Count];
                for (int i = 0; i < bt.Parameters.Count; i++)
                    tokens[i] = bt.Parameters[i].Image().ToLower();
                return lit.ToLower() == bt.Name.Image().ToLower() + "(" + string.Join(" ", tokens) + ")";
            }
            return false;
        }

        /// <summary>
        /// Read the nodes for the device
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool TryReadNodes(this CircuitComponent c, List<object> parameters, int count, int index = 0)
        {
            if (parameters.Count < index + count)
                return false;

            // Extract the nodes
            string[] nodes = new string[count];
            for (int i = index; i < index + count; i++)
            {
                string node;
                if (parameters[i].TryReadIdentifier(out node))
                    nodes[i] = node;
                else
                    return false;
            }

            // Succeeded
            c.Connect(nodes);
            return true;
        }

        /// <summary>
        /// Read a model from a Token-related object
        /// </summary>
        /// <param name="o"></param>
        /// <param name="netlist"></param>
        public static bool TryReadModel<T>(this object o, out T model, Netlist netlist) where T : CircuitModel
        {
            model = null;

            // Get the model name
            string name;
            if (!o.TryReadIdentifier(out name))
                return false;

            // Find the model in the circuit
            if (!netlist.Circuit.Components.Contains(name))
                return false;
            CircuitModel m = netlist.Circuit.Components[name] as CircuitModel;
            if (m == null)
                return false;

            // Cast the model
            if (m is T)
            {
                model = (T)m;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Try reading a bracket
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="bt">The bracket token</param>
        /// <param name="bracket">The type of bracket (default is '(', '?' for any bracket)</param>
        /// <returns></returns>
        public static bool TryReadBracket(this object o, out BracketToken bt, char bracket = '(')
        {
            bt = null;
            if (!(o is BracketToken))
                return false;
            bt = o as BracketToken;
            if (bracket == '?')
                return true;
            if (bt.Bracket != bracket)
            {
                bt = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read a value from a Token-related object
        /// </summary>
        /// <param name="o">The object</param>
        /// <returns></returns>
        public static string ReadValue(this object o)
        {
            string value;
            if (TryReadValue(o, out value))
                return value;
            throw new ParseException(o, "Value expected");
        }

        /// <summary>
        /// Read a word from a Token-related object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string ReadWord(this object o)
        {
            string value;
            if (TryReadWord(o, out value))
                return value;
            throw new ParseException(o, "Word expected");
        }

        /// <summary>
        /// Read an identifier from a Token-related object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string ReadIdentifier(this object o)
        {
            string value;
            if (TryReadIdentifier(o, out value))
                return value;
            throw new ParseException(o, "Identifier expected");
        }

        /// <summary>
        /// Read an assignment from a Token-related object
        /// </summary>
        /// <param name="o"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void ReadAssignment(this object o, out object name, out object value)
        {
            if (o.TryReadAssignment(out name, out value))
                return;
            throw new ParseException(o, "Assigned parameter expected");
        }

        /// <summary>
        /// Read an assignment from a Token-related object
        /// </summary>
        /// <param name="o"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void ReadAssignment(this object o, out string name, out string value)
        {
            if (o.TryReadAssignment(out name, out value))
                return;
            throw new ParseException(o, "PARAMETER = VALUE expected");
        }

        /// <summary>
        /// Read a (quoted) string from a Token-related object
        /// </summary>
        /// <param name="o">The object</param>
        /// <returns></returns>
        public static string ReadString(this object o)
        {
            string value;
            if (TryReadString(o, out value))
                return value;
            throw new ParseException(o, "String expected");
        }

        /// <summary>
        /// Read named parameters for a Parameterized object
        /// </summary>
        /// <param name="obj">The parameterized object</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="start">The starting index</param>
        public static void ReadParameters(this Parameterized obj, List<object> parameters, int start = 0)
        {
            for (int i = start; i < parameters.Count; i++)
            {
                string pname, pvalue;
                if (parameters[i].TryReadAssignment(out pname, out pvalue))
                    obj.Set(pname, pvalue);
                else
                    obj.Set(parameters[i].ReadWord());
            }
        }

        /// <summary>
        /// Read nodes for a component
        /// </summary>
        /// <param name="c"></param>
        /// <param name="parameters"></param>
        /// <param name="count"></param>
        /// <param name="index"></param>
        public static void ReadNodes(this CircuitComponent c, List<object> parameters, int count, int index = 0)
        {
            if (parameters.Count < index + count)
            {
                if (parameters.Count > 0)
                    throw new ParseException(parameters[parameters.Count - 1], $"{count} nodes expected");
                throw new ParseException($"Unexpected end of line, {count} nodes expected");
            }

            // Extract the nodes
            string[] nodes = new string[count];
            for (int i = index; i < index + count; i++)
                nodes[i] = parameters[i].ReadIdentifier();

            // Succeeded
            c.Connect(nodes);
        }

        /// <summary>
        /// Read a model from a Token-related object
        /// </summary>
        /// <typeparam name="T">A CircuitModel class</typeparam>
        /// <param name="o">The object</param>
        /// <param name="netlist">The netlist</param>
        /// <returns></returns>
        public static T ReadModel<T>(this object o, Netlist netlist) where T : CircuitModel
        {
            // Get the model name
            string name = o.ReadIdentifier();

            // Find the model in the circuit
            CircuitModel m = netlist.Path.FindModel(name);

            // Cast the model
            if (m is T)
                return (T)m;
            else
                throw new ParseException(o, $"The model {o.Image()} is not a {typeof(T).ToString()}");
        }

        /// <summary>
        /// Check for pure numbers
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns></returns>
        private static bool IsNumber(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsNumber(value[i]))
                    return false;
            }
            return true;
        }
    }
}

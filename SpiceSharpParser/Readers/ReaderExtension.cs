using System.Collections.Generic;
using SpiceSharp.Parameters;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using static SpiceSharp.Parser.SpiceSharpParserConstants;

namespace SpiceSharp.Parser.Readers.Extensions
{
    /// <summary>
    /// Some extension methods for Token-related objects
    /// </summary>
    public static class ReaderExtension
    {
        /// <summary>
        /// Read named parameters for a Parameterized object
        /// </summary>
        /// <param name="obj">The parameterized object</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="start">The starting index</param>
        public static void ReadParameters(this Netlist netlist, IParameterized obj, List<Token> parameters, int start = 0)
        {
            for (int i = start; i < parameters.Count; i++)
            {
                if (parameters[i].kind == TokenConstants.ASSIGNMENT)
                {
                    AssignmentToken at = parameters[i] as AssignmentToken;
                    string pname;
                    if (at.Name.kind == WORD)
                        pname = at.Name.image.ToLower();
                    else
                        throw new ParseException(parameters[i], "Invalid assignment");
                    switch (at.Value.kind)
                    {
                        case VALUE:
                            obj.Set(pname, netlist.Readers.ParseDouble(at.Value.image.ToLower()));
                            break;
                        case EXPRESSION:
                            obj.Set(pname, netlist.Readers.ParseDouble(at.Value.image.Substring(1, at.Value.image.Length - 2).ToLower()));
                            break;
                        case WORD:
                        case STRING:
                            obj.Set(pname, netlist.ParseString(at.Value));
                            break;
                        default:
                            throw new ParseException(parameters[i], "Invalid assignment");
                    }
                }
            }
        }

        /// <summary>
        /// Read nodes for a component
        /// </summary>
        /// <param name="c"></param>
        /// <param name="parameters"></param>
        /// <param name="count"></param>
        /// <param name="index"></param>
        public static void ReadNodes(this ICircuitComponent c, List<Token> parameters, int count, int index = 0)
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
            {
                if (IsNode(parameters[i]))
                    nodes[i] = parameters[i].image.ToLower();
                else
                    throw new ParseException(parameters[i], "Node expected");
            }

            // Succeeded
            c.Connect(nodes);
        }

        /// <summary>
        /// Check if the token can represent a node
        /// </summary>
        /// <param name="t">Token</param>
        /// <returns></returns>
        public static bool IsNode(Token t)
        {
            if (t.kind == WORD)
                return true;
            if (t.kind == IDENTIFIER)
                return true;
            if (IsNumber(t.image))
                return true;
            return false;
        }

        /// <summary>
        /// Check if the token can represent a (component) name
        /// </summary>
        /// <param name="t">Token</param>
        /// <returns></returns>
        public static bool IsName(Token t)
        {
            if (t.kind == WORD && char.IsLetter(t.image[0]))
                return true;
            if (t.kind == IDENTIFIER && char.IsLetter(t.image[0]))
                return true;
            return false;
        }

        /// <summary>
        /// Check if the token is a value
        /// </summary>
        /// <param name="t">Token</param>
        /// <returns></returns>
        public static bool IsValue(Token t)
        {
            if (t.kind == VALUE)
                return true;
            if (t.kind == EXPRESSION)
                return true;
            return false;
        }

        /// <summary>
        /// Read a model from a Token-related object
        /// </summary>
        /// <typeparam name="T">A CircuitModel class</typeparam>
        /// <param name="o">The object</param>
        /// <param name="netlist">The netlist</param>
        /// <returns></returns>
        public static T ReadModel<T>(this Netlist netlist, Token t) where T : ICircuitObject
        {
            // Get the model name
            switch (t.kind)
            {
                case WORD:
                case IDENTIFIER:
                    return netlist.Path.FindModel<T>(t.image.ToLower());
                default:
                    throw new ParseException(t, "Invalid model identifier");
            }
        }

        /// <summary>
        /// Parse a token to a double
        /// </summary>
        /// <param name="netlist">The netlist</param>
        /// <param name="t">The token</param>
        /// <returns></returns>
        public static double ParseDouble(this Netlist netlist, Token t)
        {
            switch (t.kind)
            {
                case VALUE:
                    return netlist.Readers.ParseDouble(t.image);
                case EXPRESSION:
                    return netlist.Readers.ParseDouble(t.image.Substring(1, t.image.Length - 2).ToLower());
                default:
                    throw new ParseException(t, "Value or expression expected");
            }
        }

        /// <summary>
        /// Parse a token to a string
        /// </summary>
        /// <param name="netlist">The netlist</param>
        /// <param name="t">The token</param>
        /// <returns></returns>
        public static string ParseString(this Netlist netlist, Token t)
        {
            switch (t.kind)
            {
                case WORD:
                    return t.image;
                case STRING:
                    return t.image.Substring(1, t.image.Length - 2);
                default:
                    throw new ParseException(t, "String expected");
            }
        }

        /// <summary>
        /// Find a model
        /// </summary>
        /// <typeparam name="T">The model class</typeparam>
        /// <param name="netlist">Netlist</param>
        /// <param name="t">Token</param>
        /// <returns></returns>
        public static T FindModel<T>(this Netlist netlist, Token t)
        {
            switch (t.kind)
            {
                case WORD:
                case IDENTIFIER:
                    return netlist.Path.FindModel<T>(t.image.ToLower());
                default:
                    throw new ParseException(t, "Invalid model name");
            }
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

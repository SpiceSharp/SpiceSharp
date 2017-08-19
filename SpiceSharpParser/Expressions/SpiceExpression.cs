using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Parser.Expressions
{
    /// <summary>
    /// A very light-weight and fast expression parser made for parsing Spice expressions
    /// It is based on Dijkstra's Shunting Yard algorithm. It is very fast for parsing expressions only once.
    /// The parser is also not very expressive for errors, so only use for relatively simple expressions.
    /// 
    /// Supported operators: +, -, *, /, %, &&, ||, !, ==, !=, <, <=, >, >=
    /// Supported functions: min, max, sqrt, abs, exp, log, log10, pow, cos, sin, tan, cosh, sinh, tanh, acos, asin, atan, atan2
    /// </summary>
    public class SpiceExpression
    {
        /// <summary>
        /// Operator
        /// </summary>
        private class Operator
        {
            public byte ID;
            public byte Precedence;
            public bool leftAssociative;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="id">The operator ID</param>
            /// <param name="precedence">The operator precedence</param>
            /// <param name="la">Is the operator left-associative?</param>
            public Operator(byte id, byte precedence, bool la)
            {
                ID = id;
                Precedence = precedence;
                leftAssociative = la;
            }
        }

        /// <summary>
        /// An operator for functions
        /// </summary>
        private class FunctionOperator : Operator
        {
            public Function Func;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="func">The function</param>
            public FunctionOperator(Function func)
                : base(ID_FUNCTION, byte.MaxValue, false)
            {
                Func = func;
            }
        }

        /// <summary>
        /// Delegate for functions
        /// </summary>
        /// <param name="output">The output stack</param>
        /// <returns></returns>
        private delegate double Function(Stack<double> output);

        /// <summary>
        /// Precedence levels
        /// </summary>
        private const byte PRE_CONDITIONAL = 1;
        private const byte PRE_CONDITIONAL_OR = 2;
        private const byte PRE_CONDITIONAL_AND = 3;
        private const byte PRE_LOGICAL_OR = 4;
        private const byte PRE_LOGICAL_XOR = 5;
        private const byte PRE_LOGICAL_AND = 6;
        private const byte PRE_EQUALITY = 7;
        private const byte PRE_RELATIONAL = 8;
        private const byte PRE_SHIFT = 9;
        private const byte PRE_ADDITIVE = 10;
        private const byte PRE_MULTIPLICATIVE = 11;
        private const byte PRE_UNARY = 12;
        private const byte PRE_PRIMARY = 13;

        /// <summary>
        /// Operator ID's
        /// </summary>
        private const byte ID_POSITIVE = 0;
        private const byte ID_NEGATIVE = 1;
        private const byte ID_NOT = 2;
        private const byte ID_ADD = 3;
        private const byte ID_SUBTRACT = 4;
        private const byte ID_MULTIPLY = 5;
        private const byte ID_DIVIDE = 6;
        private const byte ID_MODULO = 7;
        private const byte ID_EQUALS = 8;
        private const byte ID_INEQUALS = 9;
        private const byte ID_OPENCONDITIONAL = 10;
        private const byte ID_CLOSEDCONDITIONAL = 11;
        private const byte ID_CONDITIONAL_OR = 12;
        private const byte ID_CONDITIONAL_AND = 13;
        private const byte ID_LESS = 14;
        private const byte ID_LESSEQUAL = 15;
        private const byte ID_GREATER = 16;
        private const byte ID_GREATEREQUAL = 17;
        private const byte ID_LEFTBRACKET = 18;
        private const byte ID_FUNCTION = 19;

        /// <summary>
        /// Operators
        /// </summary>
        private static Operator OP_POSITIVE = new Operator(ID_POSITIVE, PRE_UNARY, false);
        private static Operator OP_NEGATIVE = new Operator(ID_NEGATIVE, PRE_UNARY, false);
        private static Operator OP_NOT = new Operator(ID_NOT, PRE_UNARY, false);

        private static Operator OP_ADD = new Operator(ID_ADD, PRE_ADDITIVE, true);
        private static Operator OP_SUBTRACT = new Operator(ID_SUBTRACT, PRE_ADDITIVE, true);
        private static Operator OP_MULTIPLY = new Operator(ID_MULTIPLY, PRE_MULTIPLICATIVE, true);
        private static Operator OP_DIVIDE = new Operator(ID_DIVIDE, PRE_MULTIPLICATIVE, true);
        private static Operator OP_MODULO = new Operator(ID_MODULO, PRE_MULTIPLICATIVE, true);
        private static Operator OP_EQUALS = new Operator(ID_EQUALS, PRE_EQUALITY, true);
        private static Operator OP_INEQUALS = new Operator(ID_INEQUALS, PRE_EQUALITY, true);
        private static Operator OP_OPENCONDITIONAL = new Operator(ID_OPENCONDITIONAL, PRE_CONDITIONAL, false);
        private static Operator OP_CLOSEDCONDITIONAL = new Operator(ID_CLOSEDCONDITIONAL, PRE_CONDITIONAL, false);
        private static Operator OP_CONDITIONAL_OR = new Operator(ID_CONDITIONAL_OR, PRE_CONDITIONAL_OR, true);
        private static Operator OP_CONDITIONAL_AND = new Operator(ID_CONDITIONAL_AND, PRE_CONDITIONAL_AND, true);
        private static Operator OP_LESS = new Operator(ID_LESS, PRE_RELATIONAL, true);
        private static Operator OP_LESSEQUAL = new Operator(ID_LESSEQUAL, PRE_RELATIONAL, true);
        private static Operator OP_GREATER = new Operator(ID_GREATER, PRE_RELATIONAL, true);
        private static Operator OP_GREATEREQUAL = new Operator(ID_GREATEREQUAL, PRE_RELATIONAL, true);
        private static Operator OP_LEFTBRACKET = new Operator(ID_LEFTBRACKET, byte.MaxValue, false);

        /// <summary>
        /// The parameters for expressions
        /// </summary>
        public Dictionary<string, double> Parameters { get; set; }

        /// <summary>
        /// Functions
        /// </summary>
        private Dictionary<string, FunctionOperator> Functions { get; } = new Dictionary<string, FunctionOperator>()
        {
            { "min", new FunctionOperator((Stack<double> output) => Math.Min(output.Pop(), output.Pop())) },
            { "max", new FunctionOperator((Stack<double> output) => Math.Max(output.Pop(), output.Pop())) },
            { "abs", new FunctionOperator((Stack<double> output) => Math.Abs(output.Pop())) },
            { "sqrt", new FunctionOperator((Stack<double> output) => Math.Sqrt(output.Pop())) },
            { "exp", new FunctionOperator((Stack<double> output) => Math.Exp(output.Pop())) },
            { "log", new FunctionOperator((Stack<double> output) => Math.Log(output.Pop())) },
            { "log10", new FunctionOperator((Stack<double> output) => Math.Log10(output.Pop())) },
            { "pow", new FunctionOperator((Stack<double> output) => {
                double b = output.Pop();
                double a = output.Pop();
                return Math.Pow(a, b);
            }) },
            { "cos", new FunctionOperator((Stack<double> output) => Math.Cos(output.Pop())) },
            { "sin", new FunctionOperator((Stack<double> output) => Math.Sin(output.Pop())) },
            { "tan", new FunctionOperator((Stack<double> output) => Math.Tan(output.Pop())) },
            { "cosh", new FunctionOperator((Stack<double> output) => Math.Cosh(output.Pop())) },
            { "sinh", new FunctionOperator((Stack<double> output) => Math.Sinh(output.Pop())) },
            { "tanh", new FunctionOperator((Stack<double> output) => Math.Tanh(output.Pop())) },
            { "acos", new FunctionOperator((Stack<double> output) => Math.Acos(output.Pop())) },
            { "asin", new FunctionOperator((Stack<double> output) => Math.Asin(output.Pop())) },
            { "atan", new FunctionOperator((Stack<double> output) => Math.Atan(output.Pop())) },
            { "atan2", new FunctionOperator((Stack<double> output) => {
                double b = output.Pop();
                double a = output.Pop();
                return Math.Atan2(a, b);
            }) }
        };

        /// <summary>
        /// Private variables
        /// </summary>
        private int i = 0;
        private string input;
        private StringBuilder sb = new StringBuilder();
        private bool infixPostfix = false;
        Stack<double> output = new Stack<double>();
        Stack<Operator> operators = new Stack<Operator>();
        private int count = 0;

        /// <summary>
        /// Parse an expression
        /// </summary>
        /// <returns></returns>
        public double Parse(string expression)
        {
            // Initialize for parsing the expression
            i = 0;
            input = expression;
            infixPostfix = false;
            output.Clear();
            operators.Clear();
            count = input.Length;

            // Parse the expression
            while (i < count)
            {
                // Skip spaces
                while (i < count && input[i] == ' ')
                    i++;

                // Parse a double
                char c = input[i];

                // Parse a binary operator
                if (infixPostfix)
                {
                    // Test for infix and postfix operators
                    infixPostfix = false;
                    switch (c)
                    {
                        case '+': PushOperator(OP_ADD); break;
                        case '-': PushOperator(OP_SUBTRACT); break;
                        case '*': PushOperator(OP_MULTIPLY); break;
                        case '/': PushOperator(OP_DIVIDE); break;
                        case '%': PushOperator(OP_MODULO); break;
                        case '=':
                            i++;
                            if (i < count && input[i] == '=')
                                PushOperator(OP_EQUALS);
                            else
                                goto default;
                            break;
                        case '!':
                            i++;
                            if (i < count && input[i] == '=')
                                PushOperator(OP_INEQUALS);
                            else
                                goto default;
                            break;
                        case '?': PushOperator(OP_OPENCONDITIONAL); break;
                        case ':':
                            // Evaluate to an open conditional
                            while (operators.Count > 0)
                            {
                                if (operators.Peek().ID == ID_OPENCONDITIONAL)
                                    break;
                                Evaluate(operators.Pop());
                            }
                            operators.Pop();
                            operators.Push(OP_CLOSEDCONDITIONAL);
                            break;
                        case '|':
                            i++;
                            if (i < count && input[i] == '|')
                                PushOperator(OP_CONDITIONAL_OR);
                            else
                                goto default;
                            break;
                        case '&':
                            i++;
                            if (i < count && input[i] == '&')
                                PushOperator(OP_CONDITIONAL_AND);
                            else
                                goto default;
                            break;
                        case '<':
                            if (i + 1 < count && input[i + 1] == '=')
                            {
                                PushOperator(OP_LESSEQUAL);
                                i++;
                            }
                            else
                                PushOperator(OP_LESS);
                            break;
                        case '>':
                            if (i + 1 < count && input[i + 1] == '=')
                            {
                                PushOperator(OP_GREATEREQUAL);
                                i++;
                            }
                            else
                                PushOperator(OP_GREATER);
                            break;

                        case ')':
                            // Evaluate until the matching opening bracket
                            while (operators.Count > 0)
                            {
                                if (operators.Peek().ID == ID_LEFTBRACKET)
                                {
                                    operators.Pop();
                                    break;
                                } else if (operators.Peek().ID == ID_FUNCTION)
                                {
                                    FunctionOperator op = (FunctionOperator)operators.Pop();
                                    output.Push(op.Func(output));
                                    break;
                                }
                                Evaluate(operators.Pop());
                            }
                            infixPostfix = true;
                            break;

                        case ',':
                            // Function argument
                            while (operators.Count > 0)
                            {
                                if (operators.Peek().ID == ID_FUNCTION)
                                    break;
                                Evaluate(operators.Pop());
                            }
                            break;
                        default:
                            throw new ParseException("Unrecognized operator");
                    }
                    i++;
                }

                // Parse a unary operator
                else
                {
                    if (c == '.' || (c >= '0' && c <= '9'))
                    {
                        output.Push(ParseDouble());
                        infixPostfix = true;
                    }

                    // Parse a parameter or a function
                    else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    {
                        sb.Clear();
                        sb.Append(input[i++]);
                        while (i < count)
                        {
                            c = input[i];
                            if ((c >= '0' && c <= '9') ||
                                (c >= 'a' && c <= 'z') ||
                                (c >= 'A' && c <= 'Z') ||
                                c == '_')
                            {
                                sb.Append(c);
                                i++;
                            }
                            else
                                break;
                        }
                        if (i < count && input[i] == '(')
                        {
                            i++;

                            // "cos(10)+sin(-1)*max(tanh(1),sinh(1))"
                            // Benchmark switch statements on function name: 1,000,000 -> 2400ms
                            // Benchmark switch on first character + if/else function name: 1,000,000 -> 2200ms
                            // Benchmark using Dictionary<>: 1,000,000 -> 2200ms -- I chose this option
                            operators.Push(Functions[sb.ToString()]);
                        }
                        else if (Parameters != null)
                        {
                            output.Push(Parameters[sb.ToString()]);
                            infixPostfix = true;
                        }
                        else
                            throw new ParseException("No parameters");
                    }

                    // Prefix operators
                    else
                    {
                        switch (c)
                        {
                            case '+': PushOperator(OP_POSITIVE); break;
                            case '-': PushOperator(OP_NEGATIVE); break;
                            case '!': PushOperator(OP_NOT); break;
                            case '(': PushOperator(OP_LEFTBRACKET); break;
                            default:
                                throw new ParseException("Unrecognized unary operator");
                        }
                        i++;
                    }
                }
            }

            // Evaluate all that is left on the stack
            while (operators.Count > 0)
                Evaluate(operators.Pop());
            if (output.Count > 1)
                throw new ParseException("Invalid expression");
            return output.Pop();
        }

        /// <summary>
        /// Evaluate operators with precedence
        /// </summary>
        /// <param name="precedence">Precedence</param>
        private void PushOperator(Operator op)
        {
            while (operators.Count > 0)
            {
                // Stop evaluation
                Operator o = operators.Peek();
                if (o.Precedence < op.Precedence || !o.leftAssociative)
                    break;
                Evaluate(operators.Pop());
            }
            operators.Push(op);
        }
        
        /// <summary>
        /// Evaluate an operator
        /// </summary>
        /// <param name="op">Operator</param>
        private void Evaluate(Operator op)
        {
            double a, b, c;
            switch (op.ID)
            {
                case ID_POSITIVE: break;
                case ID_NEGATIVE: output.Push(-output.Pop()); break;
                case ID_NOT:
                    a = output.Pop();
                    output.Push(a == 0.0 ? 1.0 : 0.0);
                    break;
                case ID_ADD: output.Push(output.Pop() + output.Pop()); break;
                case ID_SUBTRACT:
                    b = output.Pop();
                    a = output.Pop();
                    output.Push(a - b);
                    break;
                case ID_MULTIPLY: output.Push(output.Pop() * output.Pop()); break;
                case ID_DIVIDE:
                    b = output.Pop();
                    a = output.Pop();
                    output.Push(a / b);
                    break;
                case ID_MODULO:
                    b = output.Pop();
                    a = output.Pop();
                    output.Push(a % b);
                    break;
                case ID_EQUALS: output.Push(output.Pop() == output.Pop() ? 1.0 : 0.0); break;
                case ID_INEQUALS: output.Push(output.Pop() != output.Pop() ? 1.0 : 0.0); break;
                case ID_CONDITIONAL_AND:
                    b = output.Pop();
                    a = output.Pop();
                    output.Push((a != 0.0) && (b != 0.0) ? 1.0 : 0.0); break;
                case ID_CONDITIONAL_OR:
                    b = output.Pop();
                    a = output.Pop();
                    output.Push((a != 0.0) || (b != 0.0) ? 1.0 : 0.0); break;
                case ID_LESS: output.Push(output.Pop() > output.Pop() ? 1.0 : 0.0); break;
                case ID_LESSEQUAL: output.Push(output.Pop() >= output.Pop() ? 1.0 : 0.0); break;
                case ID_GREATER: output.Push(output.Pop() < output.Pop() ? 1.0 : 0.0); break;
                case ID_GREATEREQUAL: output.Push(output.Pop() <= output.Pop() ? 1.0 : 0.0); break;
                case ID_CLOSEDCONDITIONAL:
                    c = output.Pop();
                    b = output.Pop();
                    a = output.Pop();
                    output.Push(a > 0.0 ? b : c);
                    break;
                case ID_OPENCONDITIONAL: throw new ParseException("Unmatched conditional");
                default:
                   throw new ParseException("Unrecognized operator");
            }
        }

        /// <summary>
        /// Parse a double value at the current position
        /// </summary>
        /// <returns></returns>
        public double ParseDouble()
        {
            // Read integer part
            double value = 0.0;
            while (i < count && (input[i] >= '0' && input[i] <= '9'))
                value = value * 10.0 + (input[i++] - '0');

            // Read decimal part
            if (i < count && input[i] == '.')
            {
                i++;
                double mult = 1.0;
                while (i < count && (input[i] >= '0' && input[i] <= '9'))
                {
                    value = value * 10.0 + (input[i++] - '0');
                    mult = mult * 10.0;
                }
                value /= mult;
            }

            if (i < count)
            {
                // Scientific notation
                if (input[i] == 'e' || input[i] == 'E')
                {
                    i++;
                    int exponent = 0;
                    bool neg = false;
                    if (i < count && (input[i] == '+' || input[i] == '-'))
                    {
                        if (input[i] == '-')
                            neg = true;
                        i++;
                    }

                    // Get the exponent
                    while (i < count && (input[i] >= '0' && input[i] <= '9'))
                        exponent = exponent * 10 + (input[i++] - '0');

                    // Integer exponentation
                    double mult = 1.0;
                    double b = 10.0;
                    while (exponent != 0)
                    {
                        if ((exponent & 0x01) == 0x01)
                            mult *= b;
                        b *= b;
                        exponent >>= 1;
                    }
                    if (neg)
                        value /= mult;
                    else
                        value *= mult;
                        
                }
                else
                {
                    // Spice modifiers
                    switch (input[i])
                    {
                        case 't':
                        case 'T': value *= 1.0e12; i++; break;
                        case 'g':
                        case 'G': value *= 1.0e9; i++; break;
                        case 'k':
                        case 'K': value *= 1.0e3; i++; break;
                        case 'u':
                        case 'U': value /= 1.0e6; i++; break;
                        case 'n':
                        case 'N': value /= 1.0e9; i++; break;
                        case 'p':
                        case 'P': value /= 1.0e12; i++; break;
                        case 'f':
                        case 'F': value /= 1.0e15; i++; break;
                        case 'm':
                        case 'M':
                            if (i + 2 < count &&
                                (input[i + 1] == 'e' || input[i + 1] == 'E') &&
                                (input[i + 2] == 'g' || input[i + 2] == 'G'))
                            {
                                value *= 1.0e6;
                                i += 3;
                            }
                            else if (i + 2 < count &&
                                (input[i + 1] == 'i' || input[i + 1] == 'I') &&
                                (input[i + 2] == 'l' || input[i + 2] == 'L'))
                            {
                                value *= 25.4e-6;
                                i += 3;
                            }
                            else
                            {
                                value /= 1.0e3;
                                i++;
                            }
                            break;
                    }
                }

                // Any trailing letters are ignored
                while (i < count && ((input[i] >= 'a' && input[i] <= 'z') || (input[i] >= 'A' && input[i] <= 'Z')))
                    i++;
            }
            return value;
        }
    }
}

options
{
	IGNORE_CASE = true;
	STATIC = false;
}

PARSER_BEGIN(SpiceSharpExpressionParser)
namespace SpiceSharp.Parser.Expressions;
using System;
using System.Globalization;
using System.Collections.Generic;
public class SpiceSharpExpressionParser
{
	/// <summary>
	/// The parameters that this parser can read
	/// </summary>
	public Dictionary<String, Double> Parameters = new Dictionary<String, Double>();

	/// <summary>
	/// Call a function with parameters
	/// </summary>
	private Double FunctionCall(string func, double[] parameters)
	{
		double r;
		switch (func)
		{
		case "sin": if (parameters.Length != 1) throw new ParseException("Sin expects 1 parameter"); return Math.Sin(parameters[0]);
		case "cos": if (parameters.Length != 1) throw new ParseException("Cos expects 1 parameter"); return Math.Cos(parameters[0]);
		case "tan": if (parameters.Length != 1) throw new ParseException("Tan expects 1 parameter"); return Math.Tan(parameters[0]);
		case "sinh": if (parameters.Length != 1) throw new ParseException("Sinh expects 1 parameter"); return Math.Sinh(parameters[0]);
		case "cosh": if (parameters.Length != 1) throw new ParseException("Cosh expects 1 parameter"); return Math.Cosh(parameters[0]);
		case "sqrt": if (parameters.Length != 1) throw new ParseException("Sqrt expects 1 parameter"); return Math.Sqrt(parameters[0]);
		case "exp": if (parameters.Length != 1) throw new ParseException("Exp expects 1 parameter"); return Math.Exp(parameters[0]);
		case "log": if (parameters.Length != 1) throw new ParseException("Log expects 1 parameter"); return Math.Log(parameters[0]);
		case "log10": if (parameters.Length != 1) throw new ParseException("Log10 expects 1 parameter"); return Math.Log10(parameters[0]);
		case "max":
			if (parameters.Length < 2)
				throw new ParseException("Max expects at least 2 parameters");
			r = parameters[0];
			for (int i = 1; i < parameters.Length; i++)
				r = parameters[i] > r ? parameters[i] : r;
			return r;
		case "min":
			if (parameters.Length < 2)
				throw new ParseException("Min expects at least 2 parameters");
			r = parameters[0];
			for (int i = 1; i < parameters.Length; i++)
				r = parameters[i] < r ? parameters[i] : r;
			return r;
		default:
			throw new ParseException("Unrecognized function \"" + func + "\"");
		}
	}

	/// <summary>
	/// Convert a Spice-format value
	/// </summary>
	private Double ParseSpice(string s)
	{
		// Get all doubles and dots
		s = s.ToLower();
		double d;
		int i = 0;
		while (s[i] == '.' || char.IsDigit(s[i]))
			i++;
		d = double.Parse(s.Substring(0, i), CultureInfo.InvariantCulture);
		s = s.Substring(i);
		switch (s[0])
		{
		case 't': d *= 1e12; break;
		case 'g': d *= 1e9; break;
		case 'm':
			if (s.StartsWith("mil"))
				d *= 25.4e-6;
			else if (s.StartsWith("meg"))
				d *= 1e6;
			else
				d *= 1e-3;
			break;
		case 'u': d *= 1e-6; break;
		case 'n': d *= 1e-9; break;
		case 'p': d *= 1e-12; break;
		case 'f': d *= 1e-15; break;
		}
		return d;
	}
}
PARSER_END(SpiceSharpExpressionParser)

// Tokens
SKIP :
{ 
	" " | "\t"
}
TOKEN:
{
	<ADD : "+">
		| <MULTIPLY : "*">
		| <SUBTRACT : "-">
		| <DIVIDE : "/">
		| <NOT : "!">
		| <EQUALS : "==">
		| <NOTEQUALS : "!=">
		| <GREATERTHAN : ">">
		| <GREATERTHANEQUAL : ">=">
		| <SMALLERTHAN : "<">
		| <SMALLERTHANEQUAL : "<=">
		| <AND : "&&">
		| <OR : "||">
		| <COMMA : ",">
		| <MODULO : "%">
		| <QUESTION : "?">
		| <COLON : ":">
		| <RBLEFT : "(">
		| <RBRIGHT : ")">
		| <VALUE : (<DIGIT>)+ ("." (<DIGIT>)*)? | "." (<DIGIT>)+ >
		| <SCIVALUE : <VALUE> "e" ("+" | "-")? (<DIGIT>)+ >
		| <SPICEVALUE : <VALUE> ["t", "g", "m", "k", "u", "n", "p", "f"] (<LETTER>)*>
		| <STRING : "\"" (~["\"", "\\", "\n", "\r"] | "\\" (["n", "t", "b", "r", "f", "\\", "\'", "\""] | (["\n", "\r"] | "\r\n")))* "\"">
		| <WORD : <LETTER> (<CHARACTER>)*>
		| <#DIGIT : ["0" - "9"]>
		| <#LETTER : ["a" - "z"]>
		| <#CHARACTER : ["a" - "z", "0" - "9"]>
}

// Parser methods
double ParseExpression() :
{
	double r;
}
{
	r = ParseConditional()
	{
		return r;
	}
}

/// <summary>
/// Parse conditional statements
/// </summary>
double ParseConditional() :
{
	double r, a, b;
}
{
	r = ParseConditionalOr()
		(<QUESTION> a = ParseConditional() <COLON> b = ParseConditional() { r = r > 0.0 ? a : b; }) ?
	{
		return r;
	}
}

/// <summary>
/// Parse conditional OR ||
/// </summary>
double ParseConditionalOr() :
{
	double r, a;
}
{
	r = ParseConditionalAnd() (<OR> a = ParseConditionalAnd() { return (r > 0.0) || (a > 0.0) ? 1.0 : 0.0; })*
	{
		return r;
	}
}

/// <summary>
/// Parse conditional AND &&
/// </summary>
double ParseConditionalAnd() :
{
	double r, a;
}
{
	r = ParseRelational() (<AND> a = ParseRelational() { return (r > 0.0) && (a > 0.0) ? 1.0 : 0.0; })*
	{
		return r;
	}
}

/// <summary>
/// Relational operators
/// </summary>
double ParseRelational() :
{
	double r, a;
}
{
	r = ParseAdditive()
	(
		<SMALLERTHAN> a = ParseAdditive() { r = r < a ? 1.0 : 0.0; }
		| <GREATERTHAN> a = ParseAdditive() { r = r > a ? 1.0 : 0.0; }
		| <SMALLERTHANEQUAL> a = ParseAdditive() { r = r <= a ? 1.0 : 0.0; }
		| <GREATERTHANEQUAL> a = ParseAdditive() { r = r >= a ? 1.0 : 0.0; }
		| <EQUALS> a = ParseAdditive() { r = r == a ? 1.0 : 0.0; }
		| <NOTEQUALS> a = ParseAdditive() { r = r != a ? 1.0 : 0.0; }
	)*
	{
		return r;
	}
}

/// <summary>
/// Additive operators
/// </summary>
double ParseAdditive() :
{
	double r, a;
}
{
	r = ParseMultiplicative() (
		<ADD> a = ParseMultiplicative() { r = r + a; }
		| <SUBTRACT> a = ParseMultiplicative() { r = r - a; }
	)*
	{
		return r;
	}
}

/// <summary>
/// Multiplicative operators
/// </summary>
double ParseMultiplicative() :
{
	double r, a;
}
{
	// Left associative
	r = ParseUnary() (
		<MULTIPLY> a = ParseUnary() { r = r * a; }
		| <DIVIDE> a = ParseUnary() { r = r / a; }
		| <MODULO> a = ParseUnary() { r = r % a; }
	)*
	{ 
		return r;
	}
}

/// <summary>
/// Unary operators
/// </summary>
double ParseUnary() :
{
	double r;
}
{
	<SUBTRACT> r = ParsePrimary() { return -r; }
	| <ADD> r = ParsePrimary() { return r; }
	| <NOT> r = ParsePrimary() { if (r == 0.0) return 1.0; else return 0.0; }
	| r = ParsePrimary() { return r; }
}

/// <summary>
/// Primary operators
/// </summary>
double ParsePrimary() : 
{
	Token t;
	string s;
	double r;
	List<Double> pars;
}
{
	// Normal scientific notation
	t = <SCIVALUE>{ return double.Parse(t.image, CultureInfo.InvariantCulture); }

	// Spice-type notation
	| t = <SPICEVALUE>{ return ParseSpice(t.image); }

	// Value without any modifiers
	| t = <VALUE> { return double.Parse(t.image, CultureInfo.InvariantCulture); }

	| LOOKAHEAD(2) t = <WORD> { pars = new List<Double>(); } <RBLEFT>
		(r = ParseExpression() { pars.Add(r); } ("," r = ParseExpression() { pars.Add(r); })*)? <RBRIGHT>
	{
		return FunctionCall(t.image.ToLower(), pars.ToArray());
	}
	| t = <WORD>
	{
		s = t.image.ToLower();
	if (Parameters == null || !Parameters.ContainsKey(s))
		throw new ParseException("Could not find parameter \"" + t.image + "\"");
	return Parameters[s];
	}
	| <RBLEFT> r = ParseExpression() <RBRIGHT>
	{
		return r;
	}
}
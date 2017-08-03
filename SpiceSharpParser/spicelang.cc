options
{
	IGNORE_CASE = true;
	STATIC = false;
}

PARSER_BEGIN( SpiceSharpParser )
namespace SpiceSharp.Parser;
using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Parser.Readers;
public class SpiceSharpParser
{
	/// <summary>
	/// Named parameter
	/// </summary>
	public class Named
	{
		/// <summary>
		/// The name of the parameter
		/// </summary>
		public Token Name { get; }
		
		/// <summary>
		/// The value of the parameter
		/// </summary>
		public Token Value { get; }
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">The name of the parameter</param>
		/// <param name="value">The value of the parameter</param>
		public Named(Token name, Token value)
		{
			Name = name;
			Value = value;
		}
	}
	
	/// <summary>
	/// A bracketed parameter
	/// </summary>
	public class Bracketed
	{
		/// <summary>
		/// The before the bracket
		/// </summary>
		public Token Name { get; }
		
		/// <summary>
		/// The parameters between brackets
		/// </summary>
		public List<Object> Parameters { get; }
		
		/// <summary>
		/// Constructor
		/// </summary>
		public Bracketed(Token name)
		{
			Name = name;
			Parameters = new List<Object>();
		}
	}

}
PARSER_END( SpiceSharpParser )

void ParseNetlist(Netlist netlist) :
{
}
{
	(ParseSpiceLine(netlist))*
}

void ParseSpiceLine(Netlist netlist) :
{
	Token t;
	List<Object> parameters = new List<Object>();
	Object o = null;
	Reader reader = null;
}
{
	t = <WORD> (o = ParseParameter()  { parameters.Add(o); })* (<NEWLINE> | <EOF>)
		(<PLUS> (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>))*
	{
		// Find a reader that reads this line
		if ((netlist.Parse & Netlist.ParseTypes.Component) != 0)
		{
			bool found = false;
			foreach(Reader r in netlist.ComponentReaders)
			{
				if (r.Read(t, parameters, netlist))
				{
					found = true;
					break;
				}
			}
			if (!found)
				throw new ParseException("Error at line " + t.beginLine + ": Unrecognized component " + t.image);
		}
	}
	| <DOT> t = <WORD> (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>)
		(<PLUS> (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>))*
	{
		// Find a control statement reader
		if ((netlist.Parse & Netlist.ParseTypes.Control) != 0)
		{
			bool found = false;
			foreach(Reader r in netlist.ControlReaders)
			{
				if (r.Read(t, parameters, netlist))
				{
					found = true;
					break;
				}
			}
			if (!found)
				throw new ParseException("Error at line " + t.beginLine + ": Unrecognized control statement " + t.image);
		}
	}
	| <NEWLINE>
}

Object ParseParameter() :
{
	Token a, b;
	Object o = null;
	Bracketed br = null;
}
{
	LOOKAHEAD(2) a = <WORD> "=" b = <VALUE> { return new Named(a, b); }
	| LOOKAHEAD(2) a = <WORD> { br = new Bracketed(a); } "(" (o = ParseParameter() { br.Parameters.Add(o); })* ")" { return br; }
	| a = <WORD> { return a; }
	| a = <VALUE> { return a; }
	| a = <IDENTIFIER>{ return a; }
	| a = <DELIMITER> { return a; }
}

SKIP : { " " | "\t" }
SKIP : { <"*" (~["\r","\n"])* ("\n" | "\r" | "\r\n")> }
TOKEN :
{ 
	<PLUS : "+">
	| <ASTERISK : "*">
	| <DOT : ".">
	| <DELIMITER : "=" | "(" | ")">
	| <NEWLINE : "\r" | "\n" | "\r\n">
	| <VALUE : (["+","-"])? (<DIGIT>)+ ("." (<DIGIT>)*)? ("e" ("+" | "-")? (<DIGIT>)+ | ["t","g","m","k","u","n","p","f"] (<LETTER>)*)?>
	| <STRING : "\"" ( ~["\"","\\","\n","\r"] | "\\" ( ["n","t","b","r","f","\\","\'","\""] | (["\n","\r"] | "\r\n")))* "\"">
	| <WORD : <LETTER> (<CHARACTER> | "_" | ".")*>
	| <IDENTIFIER : (<CHARACTER> | "_") (<CHARACTER> | "_" | ".")*>
	| <#DIGIT : ["0"-"9"]>
	| <#LETTER : ["a"-"z"]>
	| <#CHARACTER : ["a"-"z","0"-"9"]>
}
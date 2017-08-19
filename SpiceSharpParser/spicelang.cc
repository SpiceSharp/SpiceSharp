options
{
	IGNORE_CASE = true;
	STATIC = false;
}

PARSER_BEGIN(SpiceSharpParser)
namespace SpiceSharp.Parser;
using System;
using System.Collections.Generic;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Parser.Readers.Extensions;
using SpiceSharp.Parser.Subcircuits;
public class SpiceSharpParser
{
}
PARSER_END(SpiceSharpParser)

StatementsToken ParseNetlist(Netlist netlist) :
{
	Statement st;
	StatementsToken body = new StatementsToken();
}
{
	(st = ParseSpiceLine()
	{
		if (st != null)
			body.Add(st);
	})* (<END> | <EOF>)
	{
		return body;
	}
}

Statement ParseSpiceLine() :
{
	Statement st;
	Token t, tn;
	List<Token> parameters = new List<Token>();
	StatementsToken body;
	List<Statement> statements = new List<Statement>();
}
{
	// Component definitions
	tn = <WORD> (t = ParseParameter() { parameters.Add(t); })* (<NEWLINE> | <EOF>)
		(<PLUS> (t = ParseParameter() { parameters.Add(t); })* (<NEWLINE> | <EOF>))*
	{
		return new Statement(StatementType.Component, tn, parameters);
	}

	// Subcircuit declaration
	| LOOKAHEAD(2) <DOT> tn = "SUBCKT" (t = ParseParameter() { parameters.Add(t); })* <NEWLINE>
		(<PLUS> (t = ParseParameter() { parameters.Add(t); })* <NEWLINE>)*
	{
		body = new StatementsToken();
	}
		// Read the body of the subcircuit
		(st = ParseSpiceLine() { if (st != null) body.Add(st); })*
		<ENDS> (<WORD>)? (<NEWLINE> | <EOF>)
	{
		parameters.Add(body);
		return new Statement(StatementType.Subcircuit, tn, parameters);
	}

	// Model definitions
	| LOOKAHEAD(2) <DOT> tn = "MODEL" (t = ParseParameter() { parameters.Add(t); })* (<NEWLINE> | <EOF>)
		(<PLUS> (t = ParseParameter() { parameters.Add(t); })* (<NEWLINE> | <EOF>))*
	{
		if (parameters.Count < 2)
			throw new ParseException(tn, "At least a name and model type expected", false);
		tn = parameters[0];
		parameters.RemoveAt(0);
		return new Statement(StatementType.Model, tn, parameters);
	}

	// Control statements
	| <DOT> tn = <WORD> (t = ParseParameter() { parameters.Add(t); })* (<NEWLINE> | <EOF>)
		(<PLUS> (t = ParseParameter() { parameters.Add(t); })* (<NEWLINE> | <EOF>))*
	{
		return new Statement(StatementType.Control, tn, parameters);
	}

	// Other
	| <NEWLINE> { return null; }
}

Token ParseParameter() :
{
	Token ta, tb;
	List<Token> tokens = new List<Token>();
}
{
	// Bracketted
	LOOKAHEAD(2) ta = ParseSingle() "(" (tb = ParseParameter() { tokens.Add(tb); })* ")" { ta = new BracketToken(ta, '(', tokens.ToArray()); }
	("=" tb = ParseSingle() { return new AssignmentToken(ta, tb); })?
	{ return ta; }
	| LOOKAHEAD(2) ta = ParseSingle() "[" (tb = ParseParameter() { tokens.Add(tb); })* "]" { ta = new BracketToken(ta, '[', tokens.ToArray()); }
	("=" tb = ParseSingle() { return new AssignmentToken(ta, tb); })?
	{ return ta; }
	| LOOKAHEAD(2) ta = ParseSingle() "=" tb = ParseSingle() { return new AssignmentToken(ta, tb); }
	| ta = ParseSingle() { return ta;  }
}

Token ParseSingle() :
{
	Token t;
	List<Token> ts = new List<Token>();
}
{
	(t = <WORD> | t = <VALUE> | t = <STRING> | t = <IDENTIFIER> | t = <REFERENCE> | t = <EXPRESSION>)
		{ ts.Add(t); }
	(<COMMA> (t = <WORD> | t = <VALUE> | t = <STRING> | t = <IDENTIFIER> | t = <REFERENCE> | t = <EXPRESSION>) 
		{ ts.Add(t); })*
	{
		if (ts.Count > 1)
			return new VectorToken(ts.ToArray());
		else
			return ts[0];
	}
}

SKIP : { " " | "\t" }
SKIP : { <("\n" | "\r" | "\r\n") "*" (~["\r","\n"])* > }
TOKEN :
{ 
	<PLUS : "+">
	| <ASTERISK : "*">
	| <MINUS : "-">
	| <DOT : ".">
	| <COMMA : ",">
	| <DELIMITER : "=" | "(" | ")" | "[" | "]">
	| <NEWLINE : "\r" | "\n" | "\r\n">
	| <ENDS : ".ends">
	| <END : ".end">
	| <VALUE : (["+","-"])? ((<DIGIT>)+ ("." (<DIGIT>)*)? | "." (<DIGIT>)+) ("e" ("+" | "-")? (<DIGIT>)+ | ["t","g","m","k","u","n","p","f"] (<LETTER>)*)?>
	| <STRING : "\"" ( ~["\"","\\","\n","\r"] | "\\" ( ["n","t","b","r","f","\\","\'","\""] | (["\n","\r"] | "\r\n")))* "\"">
	| <EXPRESSION : "{" (~["{","}"])+ "}">
	| <REFERENCE : "@" <WORD>>
	| <WORD : <LETTER> (<CHARACTER> | <SPECIAL>)*>
	| <IDENTIFIER : (<CHARACTER> | "_") (<CHARACTER> | <SPECIAL>)*>
	| <#DIGIT : ["0"-"9"]>
	| <#LETTER : ["a"-"z"]>
	| <#CHARACTER : ["a"-"z","0"-"9"]>
	| <#SPECIAL : ["_",".",":","!","%","#","-"]>
}
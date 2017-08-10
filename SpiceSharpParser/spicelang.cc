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
using SpiceSharp.Parser.Subcircuits;
public class SpiceSharpParser
{
	private Stack<SubcircuitDefinition> definitions = new Stack<SubcircuitDefinition>();
}
PARSER_END(SpiceSharpParser)

void ParseNetlist(Netlist netlist) :
{
	Statement st;
}
{
	(st = ParseSpiceLine()
	{
		if (st != null)
			netlist.Readers.Read(st, netlist);
	})* (<END> | <EOF>)
}

Statement ParseSpiceLine() :
{
	Statement st;
	Token t;
	List<Object> parameters = new List<Object>();
	List<Statement> body = new List<Statement>();
	Object o = null;
}
{
	// Component definitions
	t = <WORD> (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>)
		(<PLUS> (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>))*
	{
		return new Statement(StatementType.Component, t, parameters);
	}

	// Subcircuit declaration
	| LOOKAHEAD(2) <DOT> t = "SUBCKT" (o = ParseParameter() { parameters.Add(o); })* <NEWLINE>
		(<PLUS> (o = ParseParameter() { parameters.Add(o); })* <NEWLINE>)*
	{
		st = new Statement(StatementType.Subcircuit, t, parameters);
	}
		// Read the body of the subcircuit
		(st = ParseSpiceLine() { if (st != null) body.Add(st); })*
		<ENDS> (<NEWLINE> | <EOF>)
	{
		parameters.Add(body);
		return new Statement(StatementType.Subcircuit, t, parameters);
	}

	// Model definitions
	| LOOKAHEAD(2) <DOT> t = "MODEL" (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>)
		(<PLUS> (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>))*
	{
		return new Statement(StatementType.Model, t, parameters);
	}

	// Control statements
	| <DOT> t = <WORD> (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>)
		(<PLUS> (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>))*
	{
		return new Statement(StatementType.Control, t, parameters);
	}

	// Other
	| <NEWLINE> { return null; }
}

Object ParseParameter() :
{
	Object oa = null, ob = null;
	BracketToken br = null;
}
{
	// Bracketted
	LOOKAHEAD(2) oa = ParseSingle() { br = new BracketToken(oa); } "(" (oa = ParseParameter() { br.Parameters.Add(oa); })* ")" ("=" ob = ParseSingle())?
	{
		if (ob != null)
			return new AssignmentToken(br, ob);
		return br; 
	}
	| LOOKAHEAD(2) oa = ParseSingle() { br = new BracketToken(oa, '['); } "[" (oa = ParseParameter() { br.Parameters.Add(oa); })* "]" ("=" ob = ParseSingle()) ?
	{
		if (ob != null)
			return new AssignmentToken(br, ob);
		return br;
	}
	| LOOKAHEAD(2) oa = ParseSingle() "=" ob = ParseSingle()
	{
		return new AssignmentToken(oa, ob);
	}
	| oa = ParseSingle()
	{ 
		return oa; 
	}
}

Object ParseSingle() :
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
			return (Token[])(ts.ToArray());
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
	| <DIVIDE : "/">
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
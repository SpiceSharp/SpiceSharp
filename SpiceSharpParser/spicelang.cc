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
	public Boolean ParseComponents = true;
	public Boolean ParseControlStatements = true;
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
	t = <WORD> (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>)
		(<PLUS> (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>))*
	{
		if (ParseComponents)
			netlist.Readers.Read("component", t, parameters, netlist);
	}
	| <DOT> t = <WORD> (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>)
		(<PLUS> (o = ParseParameter() { parameters.Add(o); })* (<NEWLINE> | <EOF>))*
	{
		if (ParseControlStatements)
			netlist.Readers.Read("control", t, parameters, netlist);
	}
	| <NEWLINE>
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
	(t = <WORD> | t = <VALUE> | t = <STRING> | t = <IDENTIFIER> | t = <REFERENCE>)
		{ ts.Add(t); }
	(<COMMA> (t = <WORD> | t = <VALUE> | t = <STRING> | t = <IDENTIFIER> | t = <REFERENCE>) 
		{ ts.Add(t); })*
	{
		if (ts.Count > 1)
			return (Token[])(ts.ToArray());
		else
			return ts[0];
	}
}

SKIP : { " " | "\t" }
SKIP : { <"*" (~["\r","\n"])* ("\n" | "\r" | "\r\n")> }
TOKEN :
{ 
	<PLUS : "+">
	| <ASTERISK : "*">
	| <DOT : ".">
	| <COMMA : ",">
	| <DELIMITER : "=" | "(" | ")" | "[" | "]">
	| <NEWLINE : "\r" | "\n" | "\r\n">
	| <VALUE : (["+","-"])? ((<DIGIT>)+ ("." (<DIGIT>)*)? | "." (<DIGIT>)+) ("e" ("+" | "-")? (<DIGIT>)+ | ["t","g","m","k","u","n","p","f"] (<LETTER>)*)?>
	| <STRING : "\"" ( ~["\"","\\","\n","\r"] | "\\" ( ["n","t","b","r","f","\\","\'","\""] | (["\n","\r"] | "\r\n")))* "\"">
	| <REFERENCE : "@" <WORD>>
	| <WORD : <LETTER> (<CHARACTER> | <SPECIAL>)*>
	| <IDENTIFIER : (<CHARACTER> | "_") (<CHARACTER> | <SPECIAL>)*>
	| <#DIGIT : ["0"-"9"]>
	| <#LETTER : ["a"-"z"]>
	| <#CHARACTER : ["a"-"z","0"-"9"]>
	| <#SPECIAL : ["_",".",":","!","%","#","-"]>
}
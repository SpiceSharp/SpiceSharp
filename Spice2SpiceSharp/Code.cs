using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Spice2SpiceSharp
{
    public static class Code
    {
        /// <summary>
        /// Remove the comments from a piece of code
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns></returns>
        public static string RemoveComments(string code)
        {
            // Remove single line comments
            code = Regex.Replace(code, @"\/\/[^\r\n]+[\r\n]+", "");

            // Remove multiline comments
            code = Regex.Replace(code, @"\/\*([^\*]*\*)+?\/", "");
            return code;
        }

        /// <summary>
        /// Find the matching closing parenthesis
        /// </summary>
        /// <param name="content">The code</param>
        /// <param name="index">The index of the opening parenthesis</param>
        /// <returns></returns>
        public static int GetMatchingParenthesis(string content, int index)
        {
            char opening = content[index];
            char closing = '\0';
            switch (opening)
            {
                case '(': closing = ')'; break;
                case '[': closing = ']'; break;
                case '{': closing = '}'; break;
                default:
                    throw new Exception("Invalid bracket");
            }

            // Find the matching parenthesis
            bool isString = false;
            bool isSingleLineComment = false;
            bool isMultiLineComment = false;
            int level = 1;
            for (int i = index + 1; i < content.Length; i++)
            {
                char c = content[i];

                // Process opening and closing brackets
                if (!isString && !isSingleLineComment && !isMultiLineComment)
                {
                    if (c == opening)
                        level++;
                    if (c == closing)
                    {
                        level--;
                        if (level == 0)
                            return i;
                    }
                    if (c == '"')
                        isString = true;
                    else if (c == '/' && i < content.Length - 1)
                    {
                        char ni = content[i + 1];
                        if (ni == '*')
                        {
                            isMultiLineComment = true;
                            i++;
                            continue;
                        }
                        else if (ni == '/')
                        {
                            isSingleLineComment = true;
                            i++;
                            continue;
                        }
                    }
                }
                else if (isString)
                {
                    // Process strings
                    if (c == '\\')
                        i++;
                    else if (c == '"')
                        isString = false;
                    continue;
                }
                else if (isSingleLineComment)
                {
                    // Find the next newline character
                    if (c == '\r' || c == '\n')
                        isSingleLineComment = false;
                }
                else if (isMultiLineComment)
                {
                    // Find the */ part
                    if (c == '*' && i < content.Length - 1)
                    {
                        char ni = content[i + 1];
                        if (ni == '/')
                        {
                            isMultiLineComment = false;
                            i++;
                            continue;
                        }
                    }
                }
            }
            throw new Exception("Unexpected end");
        }

        /// <summary>
        /// Get the matching code block
        /// </summary>
        /// <param name="content"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetMatchingBlock(string content, int index)
        {
            int e = GetMatchingParenthesis(content, index);
            return content.Substring(index + 1, e - index - 1);
        }

        /// <summary>
        /// Extract all cases from a switch statement
        /// </summary>
        /// <param name="content">The content</param>
        /// <param name="cases">The dictionary that will contain them</param>
        public static void GetSwitchCases(string content, Dictionary<string, string> cases, int startat = 0)
        {
            // Extract the first switch statement
            var sw = new Regex(@"switch\s*\(.*?\)\s*\{");
            var sm = sw.Match(content, startat);
            string code = GetMatchingBlock(content, sm.Index + sm.Length - 1);

            // Get all case statements
            var ca = new Regex(@"case\s*(?<id>\w+)\s*:");
            var de = new Regex(@"default\s*:", RegexOptions.RightToLeft);
            var cm = ca.Match(code);
            while (cm.Success)
            {
                // We found a case statement! process it
                sm = sw.Match(code, cm.Index + 1);
                Match ncm = ca.Match(code, cm.Index + 1);

                // Find the first switch statement which could interfere
                while (sm.Success && ncm.Success && sm.Index < ncm.Index)
                {
                    int e = GetMatchingParenthesis(code, sm.Index + sm.Length - 1);
                    ncm = ca.Match(code, e + 1);
                    sm = sw.Match(code, e + 1);
                }

                // Store the case statement
                int cs = cm.Index + cm.Length;
                int ce = code.Length;
                if (ncm.Success)
                    ce = ncm.Index;
                else
                {
                    var dm = de.Match(code);
                    if (dm.Success && dm.Index > cm.Index)
                        ce = dm.Index;
                }
                cases.Add(cm.Groups["id"].Value, code.Substring(cs, ce - cs));
                cm = ncm;
            }
        }

        /// <summary>
        /// Get the parameters of a method
        /// </summary>
        /// <param name="content">The content</param>
        /// <param name="method">The name of the method</param>
        /// <param name="p">The list to which the parameters will be added</param>
        public static void GetMethodParameters(string content, List<string> p, string method = @"\w+")
        {
            Regex r = new Regex(method + @"\s*\(\s*((?<p>\w+)(\s*,\s*(?<p>\w+))*\s*)?\)");
            var m = r.Match(content);
            if (m.Success)
            {
                foreach (Capture c in m.Groups["p"].Captures)
                    p.Add(c.Value);
            }
        }

        /// <summary>
        /// Format the newlines in the code
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns></returns>
        public static string Format(string code)
        {
            // Format newlines
            code = code.Trim();
            code = Regex.Replace(code, @"(?<!\r)\n", "\r\n");
            code = Regex.Replace(code, @"\r(?!\n)", "\r\n");

            // Remove whitespaces before {
            code = Regex.Replace(code, @"[\r\n\s]*\{", Environment.NewLine + "{");

            // Newlines when doing else
            code = Regex.Replace(code, @"\}\s*else\s*\{", "}" + Environment.NewLine + "else" + Environment.NewLine + "{");

            // Remove more than 2 empty lines
            code = Regex.Replace(code, @"([ \t]*\r\n){3,}", Environment.NewLine + Environment.NewLine).Trim();

            // Format long formula's
            code = Regex.Replace(code, @"^[ \t]*[^\=\r\n\{\}]+\=[^\=;\{\}]+;", (Match m) =>
            {
                // Remove all newlines
                return Regex.Replace(m.Value, @"\s*[\r\n]+\s*", " ");
            }, RegexOptions.Multiline);

            // Format operators (one space before, one space after)
            code = Regex.Replace(code, @"[ \t]*(\/\*|\*\/|\-\>|\+\=|\-\=|\!\=|\/\=|\*\=|\&\=|\|\=|\<\=|\>=|\=\=|\&\&|\|\||\*|\/|\+|\-|\=)[ \t]*", (Match m) => " " + m.Value.Trim() + " ");
            code = Regex.Replace(code, @"[ \t]+\)", ")");
            code = Regex.Replace(code, @"\([ \t]+", "(");
            code = Regex.Replace(code, @"(?<=[\=,])[ \t]*-[ \t]*", " -");
            code = Regex.Replace(code, @"(?<=\()[ \t]*-[ \t]*", "-");
            code = Regex.Replace(code, @"[ \t]*,[ \t]*", ", ");
            code = Regex.Replace(code, @"\d+e - \d+", (Match m) => m.Value.Replace(" ", ""));
            code = code.Replace(" -> ", "->");

            // Make single line conditional statements
            Regex condr = new Regex(@"if[ \t]*\(", RegexOptions.RightToLeft);
            Match match = condr.Match(code);
            while (match.Success)
            {
                int start = match.Index + match.Length - 1;
                string pre = code.Substring(0, match.Index);
                int e = GetMatchingParenthesis(code, start);
                string post = code.Substring(e + 1);
                string cond = code.Substring(start, e - start + 1);
                cond = Regex.Replace(cond, @"\s*[\r\n]\s*", " ");
                code = pre + "if " + cond + post;

                match = match.NextMatch();
            }

            return code;
        }
    }
}

using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;

namespace SpiceSharp.CodeGeneration
{
    /// <summary>
    /// A document.
    /// </summary>
    public class Document
    {
        private readonly CompilationUnitSyntax _unit;
        private readonly ClassGenerationFinder _generated;

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public Document(string filename)
        {
            using var sr = new StreamReader(filename);

            _unit = SyntaxFactory.ParseCompilationUnit(sr.ReadToEnd());
            _generated = new ClassGenerationFinder();
            _generated.Visit(_unit);
        }

        /// <summary>
        /// Checks if this file should be processed.
        /// </summary>
        public bool ShouldGenerate => _generated.GeneratedClasses.Count > 0;

        /// <summary>
        /// Exports to the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void Export(string filename)
        {
            var rw = new ClassRewriter();

            var result = _unit;
            foreach (var c in _generated.GeneratedClasses)
                result = _unit.ReplaceNode(c, rw.Visit(c));

            using (var ws = new AdhocWorkspace())
            {
                var options = ws.Options
                    .WithChangedOption(CSharpFormattingOptions.WrappingKeepStatementsOnSingleLine, false)
                    .WithChangedOption(CSharpFormattingOptions.WrappingPreserveSingleLine, true);

                result = (CompilationUnitSyntax)Formatter.Format(result, ws, options);
            }
            using (var sw = new StreamWriter(filename))
                sw.Write(result.ToFullString());
        }
    }
}

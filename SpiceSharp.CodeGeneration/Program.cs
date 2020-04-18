using System;
using System.IO;

namespace SpiceSharp.CodeGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine("Error: Spice#.CodeGeneration did not receive arguments");
                Environment.Exit(1);
                return;
            }

            foreach (var file in Directory.GetFiles(args[0], "*.cs", SearchOption.AllDirectories))
            {
                // Overwrite the file
                var doc = new Document(file);
                if (doc.ShouldGenerate)
                {
                    Console.WriteLine("Exporting " + file);
                    doc.Export(file);
                }
            }
        }
    }
}

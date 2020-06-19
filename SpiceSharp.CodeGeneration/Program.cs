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
            else
                Console.WriteLine($"Generating files in {args[0]}");

            // Use the executable directory as the backup path
            var backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup" + DateTime.Now.ToString("yyyymmdd_hhmms"));

            // Recursively build all files
            foreach (var path in args)
            {
                foreach (var file in Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories))
                {
                    // Overwrite the file
                    var doc = new Document(file)
                    {
                        BackupPath = backupPath
                    };
                    if (doc.ShouldGenerate)
                    {
                        doc.Export(file);
                    }
                }
            }

            // Let the user know that we finished
            Console.WriteLine("Finished generating files");
        }
    }
}

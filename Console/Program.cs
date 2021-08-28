using System;
using System.IO;
using VorpalJsonCore;

namespace VorpalJsonConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var fs = new FileService();

            Console.WriteLine("Vorpal Board game name (folder name on your phone that will hold the files):");
            var projectName = Console.ReadLine();

            Console.WriteLine("Local folder path with images:");
            var dir = Console.ReadLine();

            if (!Directory.Exists(dir))
            {
                throw new Exception("Directory not found");
            }

            var files = fs.GetFileInfo(dir);
            var assets = fs.ProcessFiles(files);
            var jsonOutput = VorpalJsonBuilder.BuildJson(assets, projectName);

            fs.SaveGameDataFile(dir, jsonOutput);

            Console.WriteLine("gamedata.json created in " + dir);
            Console.ReadKey();
        }
    }
}

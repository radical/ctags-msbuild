using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CtagsMSBuildParser
{
	class MainClass
	{
		static string msbuildBinDir = "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/msbuild/14.1/bin";
		public static void Main (string [] args)
		{
			string tagsFilename = Path.Combine (Environment.CurrentDirectory, "msb-tags");
			if (args.Length == 0) {
				GenerateTagsRecursively (Environment.CurrentDirectory, tagsFilename);
			} else {
				GenerateTagsFor (args [0], tagsFilename);
			}
		}

		static void GenerateTagsRecursively (string startDir, string tagsFilename)
		{
			var gen = new MSBuildTagsGenerator ();

			foreach (var pattern in new string [] { "*proj", "*.targets", "*.props" }) {
				foreach (var file in Directory.GetFiles (startDir, pattern, SearchOption.AllDirectories)) {
					gen.ProcessFile (file);
				}
			}
			Console.WriteLine ($"Generating {tagsFilename}");
			gen.GenerateTagsFile (tagsFilename);
		}

		static void GenerateTagsFor (string filename, string tagsFilename)
		{
			var gen = new MSBuildTagsGenerator ();

			gen.ProcessFile (filename);
			Console.WriteLine ($"Generating {tagsFilename}");
			gen.GenerateTagsFile (tagsFilename);
		}
	}
}

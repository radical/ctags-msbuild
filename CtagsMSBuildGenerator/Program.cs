using System;
using System.IO;
using System.Collections.Generic;

using Mono.Options;

namespace CtagsMSBuildGenerator
{
	class MainClass
	{
		public static void Main (string [] args)
		{
			string tagsFilename = Path.Combine (Environment.CurrentDirectory, "msb-tags");
			bool recurse = false;
			bool showHelp = false;
			var p = new OptionSet () {
				{"R|recurse", v => recurse = v != null},
				{"o=|out=", v => tagsFilename = v},
				{"h|help", v => showHelp = v != null}
			};

			List<string> remaining = null;
			try {
				remaining = p.Parse (args);
			} catch (OptionException oe) {
				Console.WriteLine (oe.Message);
				return;
			}

			if (showHelp) {
				PrintUsage ();
				return;
			}

			if (recurse && remaining.Count != 0) {
				Console.WriteLine ("Use either -R or explicit filenames, but not both");
				PrintUsage ();
				return;
			}

			var gen = new MSBuildTagsGenerator (GenerateLineForVim);

			if (remaining.Count == 0) {
				FindAndProcessFiles (gen, Environment.CurrentDirectory, tagsFilename, recurse);
			} else {
				GenerateTagsFor (gen, remaining, tagsFilename);
			}

			Console.WriteLine ($"==> Generating {tagsFilename}");
			gen.GenerateTagsFile (tagsFilename);
		}

		static void FindAndProcessFiles (MSBuildTagsGenerator gen, string startDir, string tagsFilename, bool recurse)
		{
			foreach (var pattern in new string [] { "*proj", "*.targets", "*.props" }) {
				foreach (var file in Directory.GetFiles (startDir, pattern, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)) {
					gen.ProcessFile (file);
				}
			}
		}

		static void GenerateTagsFor (MSBuildTagsGenerator gen, IEnumerable<string> files, string tagsFilename)
		{
			foreach (var file in files)
				gen.ProcessFile (Path.GetFullPath(file));
		}

		static void PrintUsage ()
		{
			Console.WriteLine ("Usage: ctags-msbuild [options] <filenames>");
			Console.WriteLine ();
			Console.WriteLine ("  -R|--recurse              Look for msbuild files recursively (default: off)");
			Console.WriteLine ("  -o|--out <tags filename>  Tags file (default: msb-tags)");
		}

		static string GenerateLineForVim(string tagName, string tagFile, int lineNumber, string type, string comment)
		{
			return $"{tagName}\t{tagFile}\t{lineNumber};\"\t{type}";
		}
	}
}

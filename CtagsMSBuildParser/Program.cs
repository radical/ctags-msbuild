﻿using System;
using System.IO;
using System.Collections.Generic;

using Mono.Options;

namespace CtagsMSBuildParser
{
	class MainClass
	{
		public static void Main (string [] args)
		{
			string tagsFilename = Path.Combine (Environment.CurrentDirectory, "msb-tags");
			bool recurse = false;
			var p = new OptionSet () {
				{"R|recurse", v => recurse = v != null},
				{"o|out", v => tagsFilename = v}
			};

			var remaining = p.Parse (args);

			if (recurse && remaining.Count != 0) {
				Console.WriteLine ("Use either -R or explicit filenames, but not both");
				PrintUsage ();
				return;
			}

			var gen = new MSBuildTagsGenerator ();

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
	}
}

using System;
using System.IO;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;

namespace CtagsMSBuildGenerator
{
	// TODO
	// unknown items/props in Inputs/Outputs/DependsOn etc
	// - parallel

	public class MSBuildTagsGenerator
	{
		HashSet<string> seenProjectFiles;
		Dictionary<string, Tuple<string, string>> fullLines;
		Func<string, string, int, string, string, string> GetTagLineFunc;

		public MSBuildTagsGenerator(Func<string, string, int, string, string, string> getTagLineFunc)
		{
			GetTagLineFunc = getTagLineFunc;
			seenProjectFiles = new HashSet<string> ();
			fullLines = new Dictionary<string, Tuple<string, string>> ();
		}

		public void ProcessFile (string filename)
		{
			try {
				if (seenProjectFiles.Contains (filename))
					return;

				Console.WriteLine ($"Parsing {filename}");
				var pc = new ProjectCollection ();
				var p = pc.LoadProject (filename);

				var filesSeenHere = new HashSet<string> ();
				ParseTargets (p, filesSeenHere);
				ParseItems (p, filesSeenHere);
				ParseProperties (p, filesSeenHere);

				seenProjectFiles.UnionWith (filesSeenHere);
			} catch (Exception e) {
				Console.WriteLine ($"Error loading project {filename}: {e.Message}");
			}
		}

		public void GenerateTagsFile (string tagsFilename)
		{
			using (var sr = new StreamWriter (tagsFilename)) {
				// Write the header

				sr.WriteLine ("!_TAG_FILE_FORMAT\t2\t/extended format; --format=1 will not append ;\" to lines / ");
				//FIXME: Using 'unsorted' because vim complains that the list is not sorted if it includes tags
				//	 beginning with `_`
				sr.WriteLine ("!_TAG_FILE_SORTED\t0\t/0=unsorted, 1=sorted, 2=foldcase/");
				sr.WriteLine ("!_TAG_PROGRAM_AUTHOR\tDarren Hiebert\t/dhiebert@users.sourceforge.net/");
				sr.WriteLine ("!_TAG_PROGRAM_NAME\tExuberant Ctags\t//");
				sr.WriteLine ("!_TAG_PROGRAM_URL\thttp://ctags.sourceforge.net\t/official site/");
				sr.WriteLine ("!_TAG_PROGRAM_VERSION\t5.8\t//");

				var sorted = fullLines.Values.OrderBy (t => t.Item1).Select (t => t.Item2);
				foreach (var tup in sorted) {
					sr.WriteLine (tup);
				}
			}
		}

		void ParseProperties (Project project, HashSet<string> filesSeenHere)
		{
			foreach (var prop in project.Properties) {
				if (prop.IsReservedProperty || prop.IsReservedProperty)
					continue;

				if (prop.Xml == null)
					continue;

				if (seenProjectFiles.Contains (prop.Xml.Location.File))
					continue;

				AddTag (prop.Name, prop.Xml.Location.File, prop.Xml.Location.Line, "p", String.Empty);
				filesSeenHere.Add (prop.Xml.Location.File);
			}


		}

		void ParseItems (Project project, HashSet<string> filesSeenHere)
		{
			foreach (var item in project.Items) {
				if (item.Xml == null)
					continue;

				if (seenProjectFiles.Contains (item.Xml.Location.File))
					continue;

				AddTag (item.ItemType, item.Xml.Location.File, item.Xml.Location.Line, "i", String.Empty);
				filesSeenHere.Add (item.Xml.Location.File);
			}
		}

		void ParseTargets (Project project, HashSet<string> filesSeenHere)
		{
			foreach (var kvp in project.Targets) {
				var name = kvp.Key;
				var target = kvp.Value;

				if (target.Location == null)
					continue;

				if (seenProjectFiles.Contains (target.Location.File))
					continue;

				AddTag (target.Name, target.Location.File, target.Location.Line, "t", String.Empty);
				filesSeenHere.Add (target.Location.File);
			}

			foreach (var t in project.Targets.Values) {
				foreach (var c in t.Children) {
					var items = c as ProjectItemGroupTaskInstance;
					if (items != null) {
						foreach (var item in items.Items) {
							AddTag (item.ItemType, item.Location.File, item.Location.Line, "i", String.Empty);
							filesSeenHere.Add (item.Location.File);
						}
						continue;
					}

					var props = c as ProjectPropertyGroupTaskInstance;
					if (props != null) {
						foreach (var prop in props.Properties) {
							AddTag (prop.Name, prop.Location.File, prop.Location.Line, "p", String.Empty);
							filesSeenHere.Add (prop.Location.File);
						}
					}
				}
			}
		}

		void AddTag (string tagName, string tagFile, int lineNumber, string type, string comment)
		{
			var key = $"{type}:{tagName}:{tagFile}:{lineNumber}";
			fullLines [key] = Tuple.Create (tagName, GetTagLineFunc (tagName, tagFile, lineNumber, type, comment));
		}
	}
}
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace VUCompanion
{
	class WebUICompiler
	{
		static string modPrefix = "Mods/";
		static string currentPath = "";

		public static void Start()
		{
			currentPath = System.IO.Path.GetDirectoryName(
				System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";

			if (!File.Exists(currentPath + "vuicc.exe"))
			{
				Console.WriteLine("Missing vuicc.exe", Color.Red);
				return;
			}
			string line;
			StreamReader file = new StreamReader(currentPath + @"modlist.txt");

			while ((line = file.ReadLine()) != null)
			{
				Compile(line);
			}

			file.Close();
			Console.WriteLine("Done compiling", Color.White);
		}

		static void Compile(string modName)
		{


			bool modExists = true;
			string webUIPath = "";
			if (Directory.Exists(currentPath + modPrefix + modName + "/www"))
			{
				webUIPath = "/www";
			}
			else if (Directory.Exists(currentPath + modPrefix + modName + "/WebUI"))
			{
				webUIPath = "/WebUI";
			}
			else
			{
				modExists = false;
			}

			if (!modExists)
			{
				return;
			}

			Console.Write("Compiling WebUI for " + modName + "... ", Color.White);
			// Start the child process.
			Process p = new Process();
			// Redirect the output stream of the child process.
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.WorkingDirectory = currentPath;
			p.StartInfo.FileName = "vuicc.exe";
			p.StartInfo.Arguments = modPrefix + modName + webUIPath + " " + modPrefix + modName + "/ui.vuic";

			p.Start();
			// Do not wait for the child process to exit before
			// reading to the end of its redirected stream.
			// p.WaitForExit();
			// Read the output stream first and then wait.
			string output = p.StandardOutput.ReadToEnd();
			p.WaitForExit();

			if (p.ExitCode == 0)
			{
				Console.WriteLine("Success", Color.Green);
			}
			else
			{
				string[] result = output.Split(
					new[] { Environment.NewLine },
					StringSplitOptions.None
				);
				Console.WriteLine(result[result.Length - 2], Color.Red);
			}
		}

		public static string DString(string input)
		{
			string ret = "none";
			if (input != "")
			{
				ret = input;
			}
			return ret;
		}

		public static bool HasNumber(string input) => input.Where(x => Char.IsDigit(x)).Any();
	}
}

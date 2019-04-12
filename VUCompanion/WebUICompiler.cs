using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace VUCompanion
{
	public class WebUICompiler
	{
		private const string ModsFolderName = "Mods";
		private const string VuiccExeName = "vuicc.exe";
		private const string WwwFolderName = "/www";
		private const string WebUIFolderName = "/WebUI";
		private const string ContainerFileName = "ui.vuic";

		private static string _workingDirectory = "";

		public static void Start()
		{
			_workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";

			if (!File.Exists(_workingDirectory + VuiccExeName))
			{
				Console.WriteLine("Missing " + VuiccExeName, Color.Red);
				return;
			}

			string line;
			StreamReader file = new StreamReader(_workingDirectory + @"modlist.txt");

			while ((line = file.ReadLine()) != null)
			{
				Compile(line);
			}

			file.Close();
			Console.WriteLine("Done compiling", Color.White);
		}

		private static void Compile(string modName)
		{
			string webUIFolderName = string.Empty;

			if (Directory.Exists(Path.Combine(_workingDirectory, ModsFolderName, modName, WwwFolderName))) // www
			{
				webUIFolderName = WwwFolderName;
			}
			else if (Directory.Exists(_workingDirectory + ModsFolderName + modName + WebUIFolderName)) // WebUI
			{
				webUIFolderName = WebUIFolderName;
			}
			else
			{
				return;
			}

			Console.Write("Compiling WebUI for " + modName + "... ", Color.White);
			string webUIFolderPath = Path.Combine(ModsFolderName,
			                                      modName,
			                                      webUIFolderName);

			string compiledContainerOutputPath = Path.Combine(ModsFolderName,
			                                                  modName,
			                                                  ContainerFileName);
			// Start the child process.
			Process childProcess = new Process
			            {
				            StartInfo =
				            {
					            UseShellExecute = false,
					            RedirectStandardOutput = true,
					            WorkingDirectory = _workingDirectory,
					            FileName = VuiccExeName,
					            Arguments = webUIFolderPath + " " + compiledContainerOutputPath,
					            //Arguments = ModsFolderName + modName + webUIPath + " " + ModsFolderName + modName + "/ui.vuic"
										}
			            };

			// Redirect the output stream of the child process.
			childProcess.Start();

			// Do not wait for the child process to exit before
			// reading to the end of its redirected stream.
			// p.WaitForExit();
			// Read the output stream first and then wait.
			string output = childProcess.StandardOutput.ReadToEnd();
			childProcess.WaitForExit();

			if (childProcess.ExitCode == 0)
			{
				Console.WriteLine("Success", Color.Green);
			}
			else
			{
				string[] result = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

				Console.WriteLine(result[result.Length - 2], Color.Red);
			}
		}

		//private static string DString(string input)
		//{
		//	string ret = "none";
		//	if (input != "")
		//	{
		//		ret = input;
		//	}
		//	return ret;
		//}

		//private static bool HasNumber(string input)
		//{
		//	return input.Any(Char.IsDigit);
		//}
	}
}

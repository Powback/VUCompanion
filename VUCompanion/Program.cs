using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Shaman.Runtime;
using System.Drawing;
using System.Security.Cryptography;
using Colorful;
using Console = Colorful.Console;


namespace VUCompanion
{
	class Program
	{
		private static StyleSheet styleSheet = new StyleSheet(Color.White);

		private static string title = "VUCompanion";
		private static string titleSuffix1 = "";
		private static string titleSuffix2 = "";
		private static bool metaGathered = false;


		private static string fileName = @"C:\Program Files (x86)\VeniceUnleashed";
		private static string arguments = @"-vudebug -tracedc -updatebranch dev";
		private static string logFile = @"c:/temp/out.txt";

		private static string pTime = @"^\[\d+:\d+:\d+\]";
		private static string pInfo = @"^\[info\]";
		private static string pError = @"^\[error\].+";
		private static string pSuccess = @".+[Ss]uccess\w+.+";
		private static string pCompiling = @"Compiling .+\.\.\.";

		private static string patternGuid = @"[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}";
		private static string patternPath = @"(\w+/)+\w+";
		private static string patternNumber = @"\d+(\.\d+)?";

		private static string lastMessage = "dummy";
		private static int lastMessageCount = 0;


		private static Dictionary<string, Color> modColors = new Dictionary<string, Color>();


		static void Main(string[] args)
		{
			Console.Title = "VUCompanion";
			InitStyleSheet();
			Console.WriteLine("Starting Server", Color.White);
			WebUICompiler.Start();
			StartServer();
			Console.Title = title;
		}

		static void InitStyleSheet()
		{

			styleSheet.AddStyle(patternPath, Color.FromArgb(246, 185, 59));
			styleSheet.AddStyle(patternGuid, Color.MediumSlateBlue);


			styleSheet.AddStyle(pTime, Color.FromArgb(25,25,25));
			styleSheet.AddStyle(pInfo, Color.FromArgb(50, 50, 50));
			styleSheet.AddStyle(pError, Color.FromArgb(200, 25, 25));
			styleSheet.AddStyle(pSuccess, Color.FromArgb(25, 200, 25));



		}
		static void StartProcess()
		{
			string args = arguments;
			Process process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "cmd",
					Arguments = @"/c START /B /WAIT """" """ + fileName + @"\vu.exe" + @""" " + args,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = false
				},
			};
			process.Start();



		}

		static void ClientOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
		{
			//* Do your stuff with the output (write to console/log/StringBuilder)
			Console.WriteLine(outLine.Data);
			if (!string.IsNullOrEmpty(outLine.Data))
				Console.WriteLine(outLine.Data);
		}

		static void StartServer()
		{

			try
			{
				using (var a = ProcessUtils.RunFromRaw(fileName, fileName + @"\vu.com", arguments))
				{
					while (!a.EndOfStream)
					{
						string output = a.ReadLine();
						if (output != "")
						{
							if (!metaGathered)
							{
								FindMeta(output);
							}
							FormatAndClean(output);

						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message, Color.Red);
			}
			
		}

		static void FormatAndClean(string input)
		{
			

			var messageInfo = Regex.Match(input, @"^\[(\d+-\d+-\d+ (\d+:\d+:\d+).\d+:\d+)\] \[(\w+)\] ");
			if (messageInfo.Groups.Count < 3)
			{
				Console.WriteLine(input, Color.White);
				return;
			}

			string time = messageInfo.Groups[2].ToString();
			string type = messageInfo.Groups[3].ToString();


			string messageRaw = input.Replace(messageInfo.Groups[0].ToString(), "");
			if (messageRaw.Contains(lastMessage))
			{
				Console.SetCursorPosition(0, Console.CursorTop - 1);
				lastMessageCount++;
			}
			else
			{
				lastMessage = messageRaw;
				lastMessageCount = 0;
			}
			
			Console.WriteStyled("[" + time + "]", styleSheet);

			if (!messageRaw.Contains("[VeniceEXT]"))
			{
				if (type == "error")
				{
					Console.Write("[" + type + "] " + messageRaw, Color.FromArgb(200,25,25));
				}
				else
				{
					Console.Write("[" + type + "] " + messageRaw, Color.FromArgb(50, 50, 50));
				}
			}
			else
			{

				var message = messageRaw.Replace("[VeniceEXT] ", "");
				var modName = Regex.Match(message, @"^\[(\w+)\] ").Groups[1].ToString();
				if (modName == "")
				{
					Console.WriteStyled("[" + type + "] " + message, styleSheet);
				}
				else
				{
					
					Console.WriteStyled("[" + type + "]", styleSheet);
					Console.Write("[", Color.AliceBlue);
					Console.Write(modName, FindModColor(modName));
					Console.Write("] ", Color.AliceBlue);

					message = message.Replace("[" + modName + "] ", "");

					var moduleName = Regex.Match(message, @"^\[(\w+)\] ").Groups[1].ToString();
					if (moduleName == "")
					{
						if (message.Contains("Compiling"))
						{
							Console.Write(message, Color.FromArgb(50, 50, 50));
						}
						else if (message.Contains("Error: "))
						{
							Console.Write(message, Color.Red);

						}
						else 
						{
							Console.WriteStyled(message, styleSheet);
						}
					}
					else
					{
						message = message.Replace("[" + moduleName + "] ", "");
						Console.Write("[", Color.AliceBlue);
						Console.Write(moduleName, FindModColor(moduleName));
						Console.Write("] ", Color.AliceBlue);


						Console.WriteStyled(message, styleSheet);
					}
				}
			}

			if (lastMessageCount > 0)
			{
				Console.Write("(" + lastMessageCount + ")");
			}
			Console.WriteLine();
		}

		static void FindMeta(string input)
		{
			if (input.Contains("Initializing Venice Unleashed Server"))
			{
				titleSuffix1 = Regex.Match(input, @"Initializing Venice Unleashed Server (\(.+\))").Groups[1].ToString();
				Console.Title =  title + " " + titleSuffix1;
			};

			if (input.Contains("Successfully authenticated server with Zeus"))
			{
				titleSuffix2 = Regex.Match(input, @"Successfully authenticated server with Zeus (\(.+\))").Groups[1].ToString();
			};
			if (input.Contains("Game successfully registered with Zeus."))
			{
				Console.Title = title + " " + titleSuffix1 + " " + titleSuffix2;
			};

		}
		static Color FindModColor(string modName)
		{
			
			if (modColors.ContainsKey(modName))
			{
				return modColors[modName];
			} else { 
				var md5 = MD5.Create();
				var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(modName));
				var color = Color.FromArgb(hash[0], hash[1], hash[2]);
				modColors.Add(modName, color);
				return color;
			}
		}
	}
}

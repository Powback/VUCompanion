using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Shaman.Runtime;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using Colorful;
using Console = Colorful.Console;


namespace VUCompanion
{
	class Program
	{
		private static StyleSheet styleSheet = new StyleSheet(Color.White);

		private const string Title = "VUCompanion";
		private const string VenicePath = @"C:\Program Files (x86)\VeniceUnleashed";
		private const string Arguments = "-server -dedicated -vudebug -high120 -highResTerrain -tracedc -headless";
		//private const string LogFilePath = @"c:/temp/out.txt";

		private const string TimeStampRegex = @"^\[\d+:\d+:\d+\]";
		private const string InfoRegex = @"^\[info\]";
		private const string ErrorRegex = @"^\[error\].+";
		private const string SuccessRegex = @".+[Ss]uccess\w+.+";
		//private const string CompilingRegex = @"Compiling .+\.\.\."; // not needed?

		private const string GuidRegex = @"[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}";
		private const string PathRegex = @"(\w+/)+\w+";

		private static readonly Dictionary<string, Color> ModColors = new Dictionary<string, Color>();

		private static string _titleSuffix1 = string.Empty;
		private static string _titleSuffix2 = string.Empty;
		private static bool _metaGathered;
		private static string lastMessage = "DummyMessage";
		private static int lastMessageCount = 1;

		static void Main(string[] args)
		{
			Console.Title = "VUCompanion";
			InitStyleSheet();
			Console.WriteLine("Starting Server", Color.White);
			WebUICompiler.Start();
			StartServer();
			Console.Title = Title;
			Console.WriteLine("Done");
		}

		private static void InitStyleSheet()
		{
			styleSheet.AddStyle(PathRegex, Color.FromArgb(246, 185, 59));
			styleSheet.AddStyle(GuidRegex, Color.MediumSlateBlue);

			styleSheet.AddStyle(TimeStampRegex, Color.FromArgb(25,25,25));
			styleSheet.AddStyle(InfoRegex, Color.FromArgb(50, 50, 50));
			styleSheet.AddStyle(ErrorRegex, Color.FromArgb(200, 25, 25));
			styleSheet.AddStyle(SuccessRegex, Color.FromArgb(25, 200, 25));
		}
		private static void StartServer()
		{
			try
			{
				using (StreamReader streamReader = ProcessUtils.RunFromRaw(VenicePath, VenicePath + @"\vu.com", Arguments))
				{
					while (!streamReader.EndOfStream)
					{
						string output = streamReader.ReadLine();

						if (output != string.Empty)
						{
								if (!_metaGathered)
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

		private static void FormatAndClean(string input)
		{
			Match messageInfo = Regex.Match(input, @"^\[(\d+-\d+-\d+ (\d+:\d+:\d+).\d+:\d+)\] \[(\w+)\] ");

			if (messageInfo.Groups.Count < 3)
			{
				Console.WriteLine(input, Color.White);
				return;
			}

			string time = messageInfo.Groups[2].ToString();
			string type = messageInfo.Groups[3].ToString();

			string messageRaw = input.Replace(messageInfo.Groups[0].ToString(), string.Empty);
			if (messageRaw.Contains(lastMessage))
			{
				Console.SetCursorPosition(0, Console.CursorTop - 1);
				lastMessageCount++;
			}
			else
			{
				lastMessage = messageRaw;
				lastMessageCount = 1;
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
				string message = messageRaw.Replace("[VeniceEXT] ", string.Empty);
				string modName = Regex.Match(message, @"^\[(\w+)\] ").Groups[1].ToString();

				if (modName == string.Empty)
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
					string moduleName = Regex.Match(message, @"^\[(\w+)\] ").Groups[1].ToString();

					if (moduleName == string.Empty)
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

			if (lastMessageCount > 1)
			{
				Console.Write("(" + lastMessageCount + ")");
			}
			Console.WriteLine();
		}

		private static void FindMeta(string input)
		{
			if (input.Contains("Initializing Venice Unleashed Server"))
			{
				_titleSuffix1 = Regex.Match(input, @"Initializing Venice Unleashed Server (\(.+\))").Groups[1].ToString();
				Console.Title =  Title + " " + _titleSuffix1;
			}

			if (input.Contains("Successfully authenticated server with Zeus"))
			{
				_titleSuffix2 = Regex.Match(input, @"Successfully authenticated server with Zeus (\(.+\))").Groups[1].ToString();
			}

			if (input.Contains("Game successfully registered with Zeus."))
			{
				Console.Title = Title + " " + _titleSuffix1 + " " + _titleSuffix2;
				_metaGathered = true;
			}
		}

		private static Color FindModColor(string modName)
		{
			if (ModColors.ContainsKey(modName))
			{
				return ModColors[modName];
			}
			else
			{ 
				MD5 md5 = MD5.Create();
				byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(modName));
				Color color = Color.FromArgb(hash[0], hash[1], hash[2]);
				ModColors.Add(modName, color);
				return color;
			}
		}
	}
}

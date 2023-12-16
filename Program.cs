using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using hChat.Models;
using hChat.Utils;
using System.Text.Json;
using HLogger;

namespace hChat
{
	public class Program {
		static void Main(string[] args) {
			hLogger.SetLogOutput(hLogger.LogOutput.TerminalOnly);
			ClientWrapper.Init();


			//ClientWrapper.Connect("localhost");
			//ClientWrapper.Disconnect();
		}
	}
}
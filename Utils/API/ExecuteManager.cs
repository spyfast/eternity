using System.Collections.Generic;

namespace Eternity.Utils.API
{
	internal class ExecuteManager
	{
		private string Token;
		private List<string> Execs;
		public ExecuteManager(string token)
		{
			Token = token;
			Execs = new List<string>();
		}
		public void Add(string command)
		{
			Execs.Add(command);
			if (Execs.Count == 25)
				ForceExecute();
		}
		public void Execute()
		{
			if (Execs.Count != 0)
				ForceExecute();
		}
		private void ForceExecute()
		{
			string text = "code=";
			foreach (string current in Execs)
				text += current;
			text += "return 0;";
			Server.APIRequest("execute", text, Token);
			Execs.Clear();
		}
	}
}

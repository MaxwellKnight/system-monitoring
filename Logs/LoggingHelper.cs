namespace MonitoringAgent.Logs
{
	public static class LoggingHelper
	{
		private static readonly string LogFilePath = "Logs/agent.log";

		public static void Log(string message)
		{
			var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
			Console.WriteLine(logMessage);
			File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
		}
	}
}

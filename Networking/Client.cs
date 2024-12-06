using System.Net.Sockets;
using System.Text;
using MonitoringAgent.Logs;

namespace MonitoringAgent.Networking
{
	public static class Client
	{
		private static int _retryCount = 3;
		private static int _retryDelayMs = 2000;

		public static void SendData(string serverIp, int port, string data)
		{
			for (int attempt = 1; attempt <= _retryCount; attempt++)
			{
				try
				{
					LoggingHelper.Log($"Connecting to server {serverIp}:{port} (Attempt {attempt}/{_retryCount})");
					using var client = new TcpClient();
					client.Connect(serverIp, port);

					using var stream = client.GetStream();
					var buffer = Encoding.UTF8.GetBytes(data);
					stream.Write(buffer, 0, buffer.Length);
					LoggingHelper.Log($"Sent data: {data}");
					return; // Success - exit method
				}
				catch (SocketException ex)
				{
					LoggingHelper.Log($"Connection attempt {attempt} failed: {ex.Message}");
					if (attempt < _retryCount)
					{
						Thread.Sleep(_retryDelayMs);
					}
					else
					{
						throw;
					}
				}
			}
		}
	}
}

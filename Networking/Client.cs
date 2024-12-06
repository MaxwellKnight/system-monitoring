using System.Net.Sockets;
using System.Text;
using MonitoringAgent.Logs;

namespace MonitoringAgent.Networking
{
	public static class Client
	{
		public static void SendData(string serverIp, int port, string data)
		{
			LoggingHelper.Log($"Connecting to server {serverIp}:{port}");
			using var client = new TcpClient(serverIp, port);
			using var stream = client.GetStream();
			var buffer = Encoding.UTF8.GetBytes(data);
			stream.Write(buffer, 0, buffer.Length);
			LoggingHelper.Log($"Sent data: {data}");
		}
	}
}

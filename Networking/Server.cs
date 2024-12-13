using System.Net;
using System.Net.Sockets;
using System.Text;
using MonitoringAgent.Logs;

namespace MonitoringAgent.Networking
{
	public static class Server
	{
		private static readonly List<TcpClient> _clients = new();

		public static async Task StartServer(int port)
		{
			var listener = new TcpListener(IPAddress.Any, port);
			listener.Start();
			Console.WriteLine($"Server started on port {port}");

			while (true)
			{
				var client = await listener.AcceptTcpClientAsync();
				_clients.Add(client);
				_ = HandleClientAsync(client);
			}
		}

		private static async Task HandleClientAsync(TcpClient client)
		{
			try
			{
				using var stream = client.GetStream();
				var buffer = new byte[4096];

				while (true)
				{
					var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
					if (bytesRead == 0) break;

					var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
					LoggingHelper.Log($"Received data from {((IPEndPoint)client.Client.RemoteEndPoint!).Address}: \n{data}");
				}
			}
			catch (Exception ex)
			{
				LoggingHelper.Log($"Client error: {ex.Message}");
			}
			finally
			{
				_clients.Remove(client);
				client.Dispose();
			}
		}
	}
}

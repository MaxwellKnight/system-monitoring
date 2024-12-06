using MonitoringAgent.MetricsCollectors;
using MonitoringAgent.Networking;
using MonitoringAgent.Logs;

public class MonitoringService
{
	private readonly int _port;
	private readonly string _serverHost;
	private readonly int _collectionIntervalMs;

	public MonitoringService(string serverHost, int port, int collectionIntervalMs = 5000)
	{
		_serverHost = serverHost;
		_port = port;
		_collectionIntervalMs = collectionIntervalMs;
	}

	public async Task CollectAndSendMetrics()
	{
		while (true)
		{
			var metrics = CollectSystemMetrics();
			await SendMetricsToServer(metrics);
			await Task.Delay(_collectionIntervalMs);
		}
	}

	private string CollectSystemMetrics()
	{
		var (cpu, memory) = CPUMemoryCollector.Collect();
		var (diskGB, networkMBps) = DiskNetworkCollector.Collect();
		var (_, top10Procs) = ProcessThreadCollector.Collect();

		return $"""
            CPU: {cpu:F1}%, Memory: {memory:F0}MB
            Disk Used: {diskGB:F1}GB, Network: {networkMBps:F1}MB/s
            Top CPU Processes: {string.Join(", ", top10Procs.Select(p => $"{p.Name} ({p.CpuPercent:F1}%)"))}
            """;
	}

	private Task SendMetricsToServer(string metrics)
	{
		Client.SendData(_serverHost, _port, metrics);
		return Task.CompletedTask;
	}
}

public class Program
{
	private const int DefaultPort = 5000;

	public static async Task Main(string[] args)
	{
		var mode = args.FirstOrDefault()?.ToLower() ?? "server";

		try
		{
			await RunMode(mode);
		}
		catch (Exception ex)
		{
			LoggingHelper.Log($"Fatal error: {ex.Message}");
			Environment.Exit(1);
		}
	}

	private static async Task RunMode(string mode)
	{
		switch (mode)
		{
			case "server":
				LoggingHelper.Log("Running in Server mode...");
				await Server.StartServer(DefaultPort);
				break;

			case "client":
				LoggingHelper.Log("Running in Client mode...");
				var service = new MonitoringService("server", DefaultPort);
				await service.CollectAndSendMetrics();
				break;

			default:
				throw new ArgumentException("Invalid mode. Use 'server' or 'client'.");
		}
	}
}

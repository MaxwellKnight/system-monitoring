using MonitoringAgent.MetricsCollectors;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
	e.Cancel = true;
	cts.Cancel();
};

try
{
	while (!cts.Token.IsCancellationRequested)
	{
		var (cpu, memory) = CPUMemoryCollector.Collect();
		var (diskGB, networkMBps) = DiskNetworkCollector.Collect();
		var (sysMetrics, top10Procs) = ProcessThreadCollector.Collect();

		Console.WriteLine($"""
           [{DateTime.Now:yyyy-MM-dd HH:mm:ss}]
           System:
             CPU: {cpu:F1}%, Memory: {memory:F0}MB
             Disk Used: {diskGB:F1}GB, Network: {networkMBps:F1}MB/s
             Processes: {sysMetrics.ProcessId}, Threads: {sysMetrics.ThreadCount}

           Top CPU Processes:
           {string.Join("\n", top10Procs.Select(p => $"  {p.Name}: {p.CpuPercent:F1}% CPU, {p.WorkingSet:F0}MB RAM"))}
           """);

		await Task.Delay(5000, cts.Token);
	}
}
catch (OperationCanceledException)
{
	Console.WriteLine("Monitoring stopped.");
}

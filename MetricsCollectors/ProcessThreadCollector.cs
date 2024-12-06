using System.Diagnostics;

namespace MonitoringAgent.MetricsCollectors
{
	public static class ProcessThreadCollector
	{
		private static Dictionary<int, (DateTime StartTime, long PrevCPUTime)> _processHistory = new();

		public static (ProcessMetrics System, List<ProcessMetrics> Top10) Collect()
		{
			var processes = Process.GetProcesses();
			var now = DateTime.UtcNow;
			var metrics = new List<ProcessMetrics>();

			foreach (var p in processes)
			{
				try
				{
					var id = p.Id;
					var cpuPercent = 0.0;

					if (_processHistory.TryGetValue(id, out var history))
					{
						var cpuTime = p.TotalProcessorTime.Ticks;
						var elapsed = (now - history.StartTime).TotalSeconds;
						if (elapsed > 0)
						{
							cpuPercent = (cpuTime - history.PrevCPUTime) / (elapsed * 10000000.0) * 100;
						}
					}

					_processHistory[id] = (now, p.TotalProcessorTime.Ticks);

					metrics.Add(new ProcessMetrics
					{
						ProcessId = id,
						Name = p.ProcessName,
						ThreadCount = p.Threads.Count,
						WorkingSet = p.WorkingSet64 / 1024.0 / 1024,
						CpuPercent = Math.Min(100, Math.Max(0, cpuPercent)),
						HandleCount = p.HandleCount,
						StartTime = p.StartTime,
						Priority = p.BasePriority
					});
				}
				catch { }
			}

			_processHistory = _processHistory
				.Where(kvp => metrics.Any(m => m.ProcessId == kvp.Key))
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			var systemMetrics = new ProcessMetrics
			{
				ProcessId = 0,
				Name = "System Total",
				ThreadCount = metrics.Sum(m => m.ThreadCount),
				WorkingSet = metrics.Sum(m => m.WorkingSet),
				CpuPercent = metrics.Sum(m => m.CpuPercent),
				HandleCount = metrics.Sum(m => m.HandleCount)
			};

			var top10 = metrics
				.OrderByDescending(m => m.CpuPercent)
				.Take(10)
				.ToList();

			return (systemMetrics, top10);
		}
	}

	public class ProcessMetrics
	{
		public int ProcessId { get; set; }
		public string Name { get; set; } = string.Empty;
		public int ThreadCount { get; set; }
		public double WorkingSet { get; set; }  // MB
		public double CpuPercent { get; set; }
		public int HandleCount { get; set; }
		public DateTime StartTime { get; set; }
		public int Priority { get; set; }
	}
}

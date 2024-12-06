namespace MonitoringAgent.MetricsCollectors
{
	public static class CPUMemoryCollector
	{
		private static DateTime _lastTime = DateTime.UtcNow;
		private static TimeSpan _lastCPUTime = TimeSpan.Zero;

		public static (double cpu, double memory) Collect()
		{
			var currentTime = DateTime.UtcNow;
			var cpuTime = TimeSpan.Zero;

			foreach (var proc in System.Diagnostics.Process.GetProcesses())
			{
				try
				{
					cpuTime += proc.TotalProcessorTime;
				}
				catch { }
			}

			var elapsedTime = currentTime - _lastTime;
			var cpuUsage = (cpuTime - _lastCPUTime).TotalMilliseconds /
						  (Environment.ProcessorCount * elapsedTime.TotalMilliseconds) * 100;

			_lastTime = currentTime;
			_lastCPUTime = cpuTime;

			var totalMemory = GC.GetTotalMemory(false) / (1024 * 1024);
			return (Math.Min(100, Math.Max(0, cpuUsage)), totalMemory);
		}
	}
}

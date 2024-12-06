using System.Net.NetworkInformation;

namespace MonitoringAgent.MetricsCollectors
{
	public static class DiskNetworkCollector
	{
		private static DateTime _lastTime = DateTime.UtcNow;
		private static long _lastBytesReceived = 0;
		private static long _lastBytesSent = 0;

		public static (double diskUsedGB, double networkMBps) Collect()
		{
			var drives = DriveInfo.GetDrives();
			var totalDiskUsage = drives.Sum(d =>
			{
				try { return d.TotalSize - d.AvailableFreeSpace; }
				catch { return 0; }
			}) / (1024.0 * 1024 * 1024); // Convert to GB

			var currentTime = DateTime.UtcNow;
			var elapsedSeconds = (currentTime - _lastTime).TotalSeconds;
			var bytesReceived = NetworkInterface.GetAllNetworkInterfaces()
				.Where(nic => nic.OperationalStatus == OperationalStatus.Up)
				.Sum(nic => nic.GetIPStatistics().BytesReceived);
			var bytesSent = NetworkInterface.GetAllNetworkInterfaces()
				.Where(nic => nic.OperationalStatus == OperationalStatus.Up)
				.Sum(nic => nic.GetIPStatistics().BytesSent);

			var networkThroughput = ((bytesReceived - _lastBytesReceived) + (bytesSent - _lastBytesSent))
				/ (1024.0 * 1024) / elapsedSeconds; // Convert to MB/s

			_lastTime = currentTime;
			_lastBytesReceived = bytesReceived;
			_lastBytesSent = bytesSent;

			return (totalDiskUsage, networkThroughput);
		}
	}
}

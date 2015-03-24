using System.IO;
using System.Text;

namespace ServerUtil {
	public class Server {
		static string logPath = string.Empty;
		
		public static void Setup (string logOutputPath) {
			logPath = logOutputPath;
		}

		public static void Log(string message) {
			if (string.IsNullOrEmpty(logPath)) return;

			// file write
			using (var fs = new FileStream(
				logPath,
				FileMode.Append,
				FileAccess.Write,
				FileShare.ReadWrite)
			) {
				using (var sr = new StreamWriter(fs)) {
					sr.WriteLine("log:" + message);
				}
			}
			
		}
	}	
}
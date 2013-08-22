using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nancy.Helper
{
	public class Log
	{
		private static Log _instance;

		static Log()
		{
			_instance = new Log();
			_instance.Message();
			_instance.Start();
		}

		~Log()
		{
			Exit();
		}

		public static void Exit()
		{
			if (_instance != null)
			{
				_instance.Stop();
				_instance = null;
			}
		}

		private ConcurrentQueue<dynamic> _queue = new ConcurrentQueue<dynamic>();
		private Thread _thread;
		private string _logFileName;
		private volatile bool _isRunning;

		public static void NewLine()
		{
			Write("");
		}

		public static void Write(params string[] msg)
		{
			_instance.Message(string.Join("\t| ", msg));
		}

		public static void Error(string msg, Exception ex)
		{
			var st = new StackTrace(ex.GetBaseException(), true);
			var frame = st.GetFrames()[0];
			var line = frame.GetFileLineNumber();
			var method = (string.IsNullOrWhiteSpace(frame.GetFileName())) ? frame.GetMethod().Name : string.Join(".", Path.GetFileNameWithoutExtension(frame.GetFileName()), frame.GetMethod().Name);

			_instance.Message(msg);
			_instance.Message(" Line\t\t:" + line);
			_instance.Message(" Method\t:" + method);
			_instance.Message(" Message\t:" + ex.Message);
		}

		public static void Request(Request request)
		{
			Write(request.Method, request.Path);
		}

		public static void Response(Response response)
		{
			var status = response.StatusCode;
			if (status != HttpStatusCode.OK)
			{
				Write(status.ToString());
			}
			else
			{
				using (var memoryStream = new MemoryStream())
				{
					response.Contents.Invoke(memoryStream);
					Write(status.ToString(), Encoding.ASCII.GetString(memoryStream.GetBuffer()));
				}
			}
		}

		private void Message(string msg = "")
		{
			Console.WriteLine(msg);
			_queue.Enqueue(new { ReportedOn = DateTime.Now, Message = msg });
		}

		private void Start()
		{
			_isRunning = true;
			var logDirectory = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "logs");
			if (!Directory.Exists(logDirectory)) Directory.CreateDirectory(logDirectory);
			_logFileName = Path.Combine(logDirectory, string.Concat(DateTime.Now.ToString("yyy-MM-dd"), ".log"));
			_thread = new Thread(LogProcessor);
			_thread.IsBackground = true;
			_thread.Start();
		}

		private void EmptyQueue()
		{
			while (_queue.Any())
			{
				var entries = new List<string>();
				lock (_queue)
				{
					dynamic entry;
					while (_queue.TryDequeue(out entry))
					{
						entries.Add(string.IsNullOrWhiteSpace(entry.Message) ? string.Empty : entry.ReportedOn.ToLongTimeString() + "|" + entry.Message);
					}
					if (entries.Count > 0)
					{
						File.AppendAllLines(_logFileName, entries);
						entries.Clear();
					}
				}
			}
		}

		public void LogProcessor()
		{
			while (_isRunning)
			{
				EmptyQueue();
				if (_isRunning) Thread.Sleep(2000);
				else break;
			}
		}

		private void Stop()
		{
			EmptyQueue();
			_isRunning = false;
			if (_thread != null)
			{
				_thread.Join(5000);
				_thread.Abort();
				_thread = null;
			}
		}
	}
}

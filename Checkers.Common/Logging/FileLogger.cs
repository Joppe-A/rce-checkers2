// Copyright 2013 Cultural Heritage Agency of the Netherlands, Dutch National Military Museum and Trezorix bv
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.IO;
using System.Text;

namespace Trezorix.Checkers.Common.Logging
{
	public class FileLogger : Logger, IDisposable
	{
		private FileStream _fileStream;
		private StreamWriter _writer;
		
		// ToDo: Joppe: The dispose option is not called into anymore due to Logger not implementing IDisposable (redo this?)
		private bool _disposed;
		private readonly object _disposeLock = new object();
		

		public FileLogger(string logFileName)
		{
			_fileStream = new FileStream(logFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
			_writer = new StreamWriter(_fileStream, Encoding.UTF8);
		}

		protected override void Write(string message)
		{
			_writer.WriteLine(message);
		}

		~FileLogger()
		{
			Dispose(false);
		}

		public void Dispose() 
		{
			Dispose(true);
			GC.SuppressFinalize(this);      
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			
			lock(_disposeLock)
			{
				if (_disposed) return;
				
				if (disposing)
				{
					if (_writer != null)
					{
						Write("Log file writing stopped, log file closing.");
						_writer.Dispose();
						_fileStream.Dispose();
					}
				}
				_writer = null;
				_disposed = true;
			}
		}
	}

}

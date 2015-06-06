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

namespace Trezorix.Checkers.Common.Logging
{
	public enum LogLevel
	{
		Log,
		Info,
		Error,
		Warn
	}
	
	// ToDo: Eliminate Logger? and only use this base class?
	public abstract class Logger : ILog
	{
		private readonly LogMessageConstructor _logMessageConstructor;

		protected Logger()
		{
			_logMessageConstructor = new LogMessageConstructor();
		}

		private string ConstructLogMessage(LogLevel logLogLevel, string message)
		{
			return _logMessageConstructor.Construct(logLogLevel, message);
		}

		public void Info(string message)
		{
			Write(ConstructLogMessage(LogLevel.Info, message));
		}
		
		public void Info(string messageFormat, params string[] pars)
		{
			Info(string.Format(messageFormat, pars));
		}

		public void Error(string message)
		{
			Write(ConstructLogMessage(LogLevel.Error, message));
		}

		public void Error(string messageFormat, params string[] pars)
		{
			Error(string.Format(messageFormat, pars));
		}

		public void ErrorException(string message, Exception exception)
		{
			Write(ConstructLogMessage(LogLevel.Error, message + Environment.NewLine + exception));
			throw new Exception(message, exception);
		}

		public void Warn(string message)
		{
			Write(ConstructLogMessage(LogLevel.Warn, message));
		}

		public void Warn(string messageFormat, params string[] pars)
		{
			Warn(string.Format(messageFormat, pars));
		}

		public void WarnException(string message, Exception exception)
		{
			Write(ConstructLogMessage(LogLevel.Warn, message + Environment.NewLine + exception));
			throw new Exception(message, exception);
		}

		public void Log(string message)
		{
			Write(ConstructLogMessage(LogLevel.Log, message));
		}

		public void Log(string messageFormat, params string[] pars)
		{
			Log(string.Format(messageFormat, pars));
		}

		protected abstract void Write(string message);

	}

}

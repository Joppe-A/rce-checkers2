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
using System.Linq;

using Trezorix.Common.Meta;

namespace Trezorix.Checkers.Common.Logging
{
	public class LogMessageConstructor
	{
		private DateTime _last; // TI: Use high precision timer (stopwatch) ?

		public LogMessageConstructor()
		{
			_last = DateTime.Now;
		}

		public string Construct(LogLevel logLogLevel, string baseMessage)
		{
			var last = LastLogMessageDateTime;
			var now = Now();

			var timelabel = now.ToString(@"yyyy-MM-dd HH:mm:ss");

			TimeSpan ts = now - last;
			var timePassedLabel = ts.TotalSeconds.ToString("#00") + "." + ts.Milliseconds.ToString("000");

			_last = DateTime.Now;

			return timelabel + " " + timePassedLabel + " " + logLogLevel.AsString() + " " + baseMessage;
		}

		[TestingSeam]
		protected virtual DateTime LastLogMessageDateTime
		{
			get { return _last; }
			set { _last = value; }
		}

		[TestingSeam]
		protected virtual DateTime Now()
		{
			return DateTime.Now;
		}

		public string Construct(string message)
		{
			return Construct(LogLevel.Log, message);
		}
	}

	internal static class LogLevelRendererExtension
	{
		private readonly static int s_valuePaddingWidth;

		static LogLevelRendererExtension()
		{
			s_valuePaddingWidth = Enum.GetNames(typeof (LogLevel)).Max(s => s.Length);
		}
	
		public static string AsString(this LogLevel level)
		{
			return Enum.GetName(typeof(LogLevel), level).PadRight(s_valuePaddingWidth);
		}
	}
}
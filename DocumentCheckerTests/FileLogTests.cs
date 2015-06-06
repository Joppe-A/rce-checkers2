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
using NUnit.Framework;

using Trezorix.Checkers.Common.Logging;

namespace DocumentCheckerTests
{
	[TestFixture]
	public class LogMessageHelperTests
	{
		private LogMessageConstructorStubbed _logMsgConstructor;

		[SetUp]
		public void Setup()
		{
			_logMsgConstructor = new LogMessageConstructorStubbed();
			_logMsgConstructor.SetNow(new DateTime(2011, 3, 11, 16, 07, 30, 777));
		}

		[Test]
		public void ConstructMessage_should_add_time()
		{
			// arrange
			
			// act
			var result = _logMsgConstructor.Construct(LogLevel.Log, "logit");

			// assert
			Assert.AreEqual("2011-03-11 16:07:30", result.Substring(0, 19));
		}

		[Test]
		public void ConstructMessage_should_add_timespan_since_last_message()
		{
			// arrange
			_logMsgConstructor.SetLast(new DateTime(2011, 3, 10, 15, 05, 28, 999));

			// act
			var result = _logMsgConstructor.Construct("logit");

			// assert
			Assert.AreEqual("90122.778", result.Substring(20, 9));
		}

		[Test]
		public void ConstructMessage_with_no_LogLevel_should_include_LogLevel_log()
		{
			// arrange
			// act
			var result = _logMsgConstructor.Construct("logit");

			// assert
			Assert.AreEqual("Log", result.Substring(35, 3));
		}

		[Test]
		public void ConstructMessage_should_pad_loglevel()
		{
			// arrange
			// act
			var result = _logMsgConstructor.Construct("logit");

			// assert
			// padding to "Error", length 5
			Assert.AreEqual("Log  ", result.Substring(35, 5));
		}
	}

	public class LogMessageConstructorStubbed : LogMessageConstructor
	{
		private DateTime _now;
		
		public void SetNow(DateTime dateTime)
		{
			_now = dateTime;
		}

		protected override DateTime Now()
		{
			return _now;
		}

		public void SetLast(DateTime dateTime)
		{

			base.LastLogMessageDateTime = dateTime;
		}
	}
}

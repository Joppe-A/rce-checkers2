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
using System.IO;
using NUnit.Framework;

using Trezorix.Checkers.Common.Logging;
using Trezorix.Testing.Common.System;

namespace DocumentCheckerTests.Integration
{
	[TestFixture]
	public class FileLogIntegrationTests
	{
		private string _testFileName;
		
		[SetUp]
		[Category("Integration")]
		public void Setup()
		{
			_testFileName = FileTestHelpers.GetTestFilesDir() + @"\testlog.txt";
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(_testFileName))
			{
				File.Delete(_testFileName);
			}
		}

		[Test]
		public void Info_given_a_valid_logfile_should_write_message_to_file()
		{
			// arrange
			ILog log = new FileLogger(_testFileName);

			// act
			const string the_log_message = "[--test log file write--]";

			log.Info(the_log_message);
			
			// assert
			((FileLogger)log).Dispose();

			string result;
			using (var textStream = new StreamReader(_testFileName))
			{
				result = textStream.ReadToEnd();
			}
		
			Assert.IsTrue(result.Contains(the_log_message), "Didn't find expect string, result is: " + result);
		}

	}
}

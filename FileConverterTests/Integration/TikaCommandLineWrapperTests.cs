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
using Trezorix.Checkers.FileConverter;
using Trezorix.Testing.Common.System;

namespace FileConverterTests.Integration
{
	[TestFixture]
	[Category("System")]
	public class TikaCommandLineWrapperTests
	{
		private TikaCommandLineWrapper _converter;
		private string _targetFilePathName;

		[SetUp]
		public void Setup()
		{
			_converter = new TikaCommandLineWrapper(@"C:\Program Files\Java\jre7\bin\java.exe");
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(_targetFilePathName)) File.Delete(_targetFilePathName);
		}

		[Test]
		public void Convert_should_write_a_converted_file()
		{
			// arrange
			string sourceFile = Path.Combine(FileTestHelpers.GetTestFilesDir(), "TikaTest.docx");
			_targetFilePathName = sourceFile + "-test.xhtml";

			// act
			_converter.Convert(sourceFile, _targetFilePathName);

			// assert
			Assert.IsTrue(File.Exists(_targetFilePathName), "Converted file doesn't exist!");
		}

		[Test]
		[ExpectedException(typeof(FileDoesNotExistException))]
		public void Convert_with_non_existing_source_file_should_throw()
		{
			// arrange
			
			// act
			_converter.Convert("nonexistingfile", "sometargetpath");
			
			// assert
			
		}
	}
}

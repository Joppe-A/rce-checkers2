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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Trezorix.Checkers.ExcelXmlChecker;
using Trezorix.Testing.Common.System;

namespace ExcelXmlCheckerTests
{
	[TestFixture]
	public class ParserTests
	{
		// ToDo: test what happens when the file is bad?

		// ToDo: test no rows in file
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Ctor_with_duplicate_indices_should_throw()
		{
			// arrange
			var parser = new Parser(FileTestHelpers.GetTestFilesDir() + "\\test1.xml", 0, 0);
		}
		
		[Test]
		public void TextContent_should_match_file()
		{
			// arrange
			var parser = new Parser(FileTestHelpers.GetTestFilesDir() + "\\test1.xml", 0, 1)
			             	{
			             		HeaderRows = 1
			             	};
			
			// act
			IEnumerable<TextContent> result = parser.TextContent;
			
			// assert
			var resultSet = result.ToArray();
			Assert.AreEqual("tekst hier test GRONDMONSTER test", resultSet[0].Text);
			Assert.AreEqual("463764736", resultSet[0].Id);
			
			Assert.AreEqual("molen krui stelling", resultSet[1].Text);
			Assert.AreEqual("sdfgdf", resultSet[1].Id);
			
		}

		[Test]
		public void TextContent_with_inverted_cell_indices_should_match_file()
		{
			// arrange
			var parser = new Parser(FileTestHelpers.GetTestFilesDir() + "\\test2.xml", 1, 0)
			{
				HeaderRows = 1
			};

			// act
			IEnumerable<TextContent> result = parser.TextContent;

			// assert
			var resultSet = result.ToArray();
			Assert.AreEqual("tekst hier test test", resultSet[0].Text);
			Assert.AreEqual("463764736", resultSet[0].Id);

			Assert.AreEqual("molen krui stelling", resultSet[1].Text);
			Assert.AreEqual("sdfgdf", resultSet[1].Id);

		}

		[Test]
		public void TextContent_with_unused_columns_should_match_file()
		{
			// arrange
			var parser = new Parser(FileTestHelpers.GetTestFilesDir() + "\\test3.xml", 3, 1)
			{
				HeaderRows = 1
			};

			// act
			IEnumerable<TextContent> result = parser.TextContent;

			// assert
			var resultSet = result.ToArray();
			Assert.AreEqual("tekst hier test test", resultSet[0].Text);
			Assert.AreEqual("463764736", resultSet[0].Id);

			Assert.AreEqual("molen krui stelling", resultSet[1].Text);
			Assert.AreEqual("sdfgdf", resultSet[1].Id);

		}

		[Test]
		public void TextContent_with_missing_text_columns_should_pass()
		{
			// arrange
			var parser = new Parser(FileTestHelpers.GetTestFilesDir() + "\\test4.xml", 0, 1)
			{
				HeaderRows = 1
			};

			// act
			IEnumerable<TextContent> result = parser.TextContent;

			// assert
			var resultSet = result.ToArray();
			Assert.AreEqual(null, resultSet[0].Text);
			Assert.AreEqual("463764736", resultSet[0].Id);

			Assert.AreEqual(null, resultSet[1].Text);
			Assert.AreEqual("sdfgdf", resultSet[1].Id);

		}

		[Test]
		public void TextContent_only_parses_first_sheet()
		{
			// arrange
			var parser = new Parser(FileTestHelpers.GetTestFilesDir() + "\\test5.xml", 0, 1)
			{
				HeaderRows = 1
			};

			// act
			IEnumerable<TextContent> result = parser.TextContent;

			// assert
			var resultSet = result.ToArray();
			Assert.AreEqual(null, resultSet[0].Text);
			Assert.AreEqual("463764736", resultSet[0].Id);

			Assert.AreEqual(null, resultSet[1].Text);
			Assert.AreEqual("sdfgdf", resultSet[1].Id);

			Assert.AreEqual(2, resultSet.Length, "More results then expected, parser read from 2nd sheet?");
		}
	}
}

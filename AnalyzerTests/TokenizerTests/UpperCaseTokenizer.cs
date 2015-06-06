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
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Trezorix.Checkers.Analyzer.Tokenizers;

namespace AnalyzerTests.TokenizerTests
{

	[TestFixture]
	public class UpperCaseTokenizerTests
	{
		private UpperCaseTokenizer _uppercaser;

		[SetUp]
		public void Setup()
		{
			_uppercaser = new UpperCaseTokenizer();
		}

		[Test]
		public void Should_uppercase()
		{
			// arrange
			var input = new List<Token>()
			            	{
			            		Token.Create("test"),
								Token.Create("should be uppercase")
			            	};
			// act
			var result = _uppercaser.Tokenize(input).ToList();

			// assert
			Assert.AreEqual("TEST", result[0].Value);
			Assert.AreEqual("SHOULD BE UPPERCASE", result[1].Value);
		}

		[Test]
		public void Should_maintain_same_position()
		{
			// arrange
			const int thePosition = 5;

			// act
			var result = _uppercaser.Tokenize(new List<Token>() {Token.Create("test", thePosition)});

			// assert
			Assert.AreEqual(thePosition, result.First().Position.Start);
		}


	}
}

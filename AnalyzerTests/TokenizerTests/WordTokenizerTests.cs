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
using System.Linq;

using NUnit.Framework;

using Trezorix.Checkers.Analyzer.Tokenizers;

namespace AnalyzerTests.TokenizerTests
{
	[TestFixture]
	public class WordTokenizerTests
	{
		private WordTokenizer _wordTokenizer;

		[SetUp]
		public void Setup()
		{
			// arrange
			_wordTokenizer = new WordTokenizer();
		}

		[Test]
		[TestCase("word1 word2", Result = "word1")]
		[TestCase("word1. word2", Result = "word1")]
		[TestCase("word1, word2", Result = "word1")]
		[TestCase("word1: .word2", Result = "word1")]
		[TestCase("word1; word2", Result = "word1")]
		[TestCase("word1 word2", Result = "word1")]
		[TestCase("\"word1\" word2", Result = "word1")]
		[TestCase("'word1' word2", Result = "word1")]
		[TestCase("(word1) word2", Result = "word1")]
		[TestCase(@"word1/word2", Result = "word1")]
		public string Should_detect_whitespace(string input)
		{
			// act
			var resultSet = _wordTokenizer.Tokenize(Token.Create(input));

			// assert
			var result = resultSet.FirstOrDefault();
			Assert.IsNotNull(result, "Token set is empty, expected at least one token");
			return result.Value;
		}

		[Test]
		public void Hyphens_should_not_seperate_words()
		{
			// act
			var resultSet = _wordTokenizer.Tokenize(Token.Create(@"word1-word2"));

			// assert
			var result = resultSet.First();
			Assert.AreEqual("word1-word2", result.Value);
		}

		[Test]
		[TestCase("word", Result = "word")]
		[TestCase("word ", Result = "word")]
		[TestCase(" word", Result = "word")]
		[TestCase(" word ", Result = "word")]
		[TestCase("  word  ", Result = "word")]
		public string Should_detect_end_of_input(string input)
		{
			// arrange
			
			// act
			var result = _wordTokenizer.Tokenize(Token.Create(input));

			// assert
			return result.First().Value;
		}

		[Test]
		public void Should_detect_multiple_words()
		{
			// act
			var result = _wordTokenizer.Tokenize(Token.Create("word1; word2, word3. word4/"));

			// assert
			var resultList = result.ToList();
			Assert.AreEqual("word1", resultList[0].Value);
			Assert.AreEqual("word2", resultList[1].Value);
			Assert.AreEqual("word3", resultList[2].Value);
			Assert.AreEqual("word4", resultList[3].Value);
		}

		[Test]
		public void Should_adjust_position_relative_to_receiving_token()
		{
			// arrange
			const int theStartPosition = 5;
			
			//                             56789012345
			var inputToken = Token.Create("word1 word2", theStartPosition);

			// act
			var result = _wordTokenizer.Tokenize(inputToken);
			
			// assert
			var resultList = result.ToList();
			Assert.AreEqual(theStartPosition, resultList[0].Position.Start);
			Assert.AreEqual(11, resultList[1].Position.Start);
		}

	}
}

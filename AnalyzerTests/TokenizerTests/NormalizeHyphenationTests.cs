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

using Trezorix.Checkers.Analyzer.Tokenizers;

namespace AnalyzerTests.TokenizerTests
{
	[TestFixture]
	public class HyphenationNormalizerTests
	{
		private readonly char[] newLineChars = Environment.NewLine.ToCharArray();

		[Test]
		public void one_split_word()
		{
			// arrange
			const string inputtext = 
@"Zin met woord-
afbreking.";

			var tokenizer = new HypenationNormalizer();

			var input = CreateInputText(inputtext);

			// act
			var result = tokenizer.Tokenize(input);
		
			// assert
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual("Zin met woordafbreking.", result.First().Value);
		}

		[Test]
		public void no_split_word()
		{
			// arrange
			const string inputtext =
@"Sentence with no line split words.";

			var tokenizer = new HypenationNormalizer();

			var input = CreateInputText(inputtext);
			
			// act
			var result = tokenizer.Tokenize(input);

			// assert
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual("Sentence with no line split words.", result.First().Value);
		}

		private IEnumerable<Token> CreateInputText(string inputtext)
		{
			return inputtext.Split(newLineChars, StringSplitOptions.RemoveEmptyEntries).Select( text => Token.Create(text)).ToArray();
		}

		[Test]
		public void candidate_split_word_without_next_line()
		{
			// arrange
			const string inputtext =
@"line with split token but no next line-";

			var tokenizer = new HypenationNormalizer();
			var input = CreateInputText(inputtext);

			// act
			var result = tokenizer.Tokenize(input);

			// assert
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual("line with split token but no next line-", result.First().Value);
		}

		[TestCase(
@"Zin met woord-
afbreking. Zin met woord-
afbreking.", Result = "Zin met woordafbreking. Zin met woordafbreking.")]
		[TestCase(@"Zin met woord-
afbreking. Zin met woord-
afbreking. Zin met woord-
afbreking.", Result = "Zin met woordafbreking. Zin met woordafbreking. Zin met woordafbreking.")]
		[TestCase(@"Zin met woord-
afbreking. Zin met woord-
afbreking. Zin met woord-
afbreking. Volgende zin 
zonder woordafbreking", Result = "Zin met woordafbreking. Zin met woordafbreking. Zin met woordafbreking. Volgende zin ")]
		public string multiple_split_words(string chunk)
		{
			var tokenizer = new HypenationNormalizer();
			var input = CreateInputText(chunk);

			// act
			var result = tokenizer.Tokenize(input);

			// assert
			return result.First().Value;
		}
	}
}

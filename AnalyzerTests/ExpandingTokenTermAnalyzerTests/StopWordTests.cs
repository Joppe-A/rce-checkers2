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
using System.Text;

using Moq;

using NUnit.Framework;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.Analyzer.Matchers;

namespace AnalyzerTests.ExpandingTokenTermAnalyzerTests
{
	[TestFixture]
	public class StopWordTests
	{
		[Test]
		public void When_considering_single_token_should_not_query_stopwords()
		{
			// arrange
			var mockMatcher = new Mock<IExpandingTokenMatcher>();

			var textAnalyzer = new ExpandingTokenTermAnalyzerBuilder()
			                   {
			                   		ExpandingTokenMatcher = mockMatcher.Object,
									StopWords = new StopWords()
									            {
									            	new StopWord() { Word = "DE", Language = "dut" },
													new StopWord() { Word = "EEN", Language = "dut" }
									            }
			                   }.Build();


			// act
			textAnalyzer.Analyse("de aap liep langs een hek schreef de groene");

			// assert
			mockMatcher.Verify(m => m.Match("DE"), Times.Once());
			mockMatcher.Verify(m => m.Match("EEN"), Times.Once());
		}

		[Test]
		public void When_considering_multiple_tokens_should_include_stopwords()
		{
			// arrange
			var mockMatcher = new Mock<IExpandingTokenMatcher>(MockBehavior.Strict);

			mockMatcher.Setup(m => m.Match("DE")).Returns(TokenMatch.CreateFullAndPartial(() => new List<ConceptTerm>()));
			mockMatcher.Setup(m => m.Match("DE AAP")).Returns(TokenMatch.CreatePartial());
			mockMatcher.Setup(m => m.Match("DE AAP DE")).Returns(TokenMatch.CreatePartial());
			
			var dummyFullMatch = TokenMatch.CreateFull(() => new List<ConceptTerm>());
			mockMatcher.Setup(m => m.Match("DE AAP DE MENS")).Returns(dummyFullMatch);
			
			var textAnalyzer = new ExpandingTokenTermAnalyzerBuilder()
			{
				ExpandingTokenMatcher = mockMatcher.Object,
				StopWords = new StopWords()
								{
									new StopWord() { Word = "DE", Language = "dut" },
								}
			}.Build();

			// act
			textAnalyzer.Analyse("de aap de mens");

			// assert
			mockMatcher.Verify(m => m.Match("DE"), Times.Exactly(2)); // twice: once for overlap mapping and once while matching
			mockMatcher.Verify(m => m.Match("DE AAP"), Times.Once());
			mockMatcher.Verify(m => m.Match("DE AAP DE"), Times.Once());
			mockMatcher.Verify(m => m.Match("DE AAP DE MENS"), Times.Once());
		}

		[Test]
		public void Stop_word_overlapping_hits_should_be_flagged_as_such()
		{
			// arrange
			var mockMatcher = new Mock<IExpandingTokenMatcher>(MockBehavior.Strict);

			var dummyFullMatch = TokenMatch.CreateFull(() => new List<ConceptTerm>());
			mockMatcher.Setup(m => m.Match("AAP")).Returns(dummyFullMatch);

			var textAnalyzer = new ExpandingTokenTermAnalyzerBuilder()
			{
				ExpandingTokenMatcher = mockMatcher.Object,
				StopWords = new StopWords()
								{
									new StopWord() { Word = "AAP", Language = "dut" },
								}
			}.Build();

			// act
			var result = textAnalyzer.Analyse("aap");

			// assert
			var tokenHit = result.SingleOrDefault(th => th.Value == "AAP");
			Assert.IsNotNull(tokenHit, "Missing expected hit");

			var hit = textAnalyzer.TextMatches[tokenHit.Value];
			Assert.IsTrue(hit.IsPotentialStopWord, "Hit not flagged as stop word.");
		}

		[Test]
		public void Stop_word_overlapping_hits_should_register_the_relevant_languages()
		{
			// arrange
			var mockMatcher = new Mock<IExpandingTokenMatcher>(MockBehavior.Strict);

			var dummyFullMatch = TokenMatch.CreateFull(() => new List<ConceptTerm>());
			mockMatcher.Setup(m => m.Match("AAP")).Returns(dummyFullMatch);

			var textAnalyzer = new ExpandingTokenTermAnalyzerBuilder()
			{
				ExpandingTokenMatcher = mockMatcher.Object,
				StopWords = new StopWords()
								{
									new StopWord() { Word = "AAP", Language = "dut" },
									new StopWord() { Word = "AAP", Language = "fr" },
									new StopWord() { Word = "AAP", Language = "ger" },
								}
			}.Build();

			// act
			var result = textAnalyzer.Analyse("aap");

			// assert
			mockMatcher.Verify(m => m.Match("AAP"), Times.Exactly(2));

			var tokenHit = result.SingleOrDefault(th => th.Value == "AAP");
			Assert.IsNotNull(tokenHit, "Missing expected hit");

			var hit = textAnalyzer.TextMatches[tokenHit.Value];
			Assert.IsTrue(hit.IsPotentialStopWord, "Hit not flagged as stop word.");
			Assert.IsTrue(hit.StopWordLanguages.Any(l => l == "dut"), "Missing expected stopword language dut.");
			Assert.IsTrue(hit.StopWordLanguages.Any(l => l == "ger"), "Missing expected stopword language ger.");
			Assert.AreEqual(3, hit.StopWordLanguages.Count(), "More languages then expected.");
		}
	}
}

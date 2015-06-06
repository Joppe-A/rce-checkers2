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

using Moq;

using NUnit.Framework;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.Analyzer.Matchers;

namespace AnalyzerTests.ExpandingTokenTermAnalyzerTests
{
	public class SimpleStubTokenMatcher : IExpandingTokenMatcher
	{
		public TokenMatch Match(string token)
		{
			switch (token)
			{
				case "DE":
				case "DE GROENE":
					return TokenMatch.CreatePartial();

				case "DE GROENE DRAAK":
					return TokenMatch.CreateFull(() => new HashSet<ConceptTerm>()
					                             	{
					                             		new ConceptTerm("skoskey", "3", "c3", "", "", "", "", "", "", ""),
					                             	});

				case "DE GROENE DRAAK HEEFT":
					return null;
			}
			return null;
		}

	}

	public class TwoSequencialMatchesStubTokenMatcher : IExpandingTokenMatcher
	{
		public TokenMatch Match(string token)
		{
			switch (token)
			{
				case "DE":
				case "DE GROENE":
					return TokenMatch.CreatePartial();

				case "DE GROENE DRAAK":
					return TokenMatch.CreateFull(() => new HashSet<ConceptTerm>()
					                             	{
					                             		new ConceptTerm("skoskey", "3", "", "", "", "","", "", "", "")
					                             	});

				case "DE GROENE DRAAK OOK":
					return null;
				case "OOK":
					return TokenMatch.CreatePartial();

				case "OOK TANDEN":
					return TokenMatch.CreateFull(() => new HashSet<ConceptTerm>()
					                             	{
					                             		new ConceptTerm("skoskey", "6", "", "", "","", "", "", "", "")
					                             	});

			}
			return null;
		}
	}

	public class ComplexStubTokenMatcher : IExpandingTokenMatcher
	{
		public TokenMatch Match(string token)
		{
			switch (token)
			{
				case "HEEFT":
				case "HEEFT DE":
					return TokenMatch.CreatePartial();
				case "HEEFT DE GROENE":
					return null;
				case "DE":
					return TokenMatch.CreatePartial();
				case "DE GROENE":
					return TokenMatch.CreateFullAndPartial(() => new HashSet<ConceptTerm>()
					                             	{
					                             		new ConceptTerm("skoskey", "1", "", "", "","", "", "", "", "")
					                             	});
				case "DE GROENE AMSTERDAMMER":
					return TokenMatch.CreateFull(() => new HashSet<ConceptTerm>()
					                             	{
					                             		new ConceptTerm("skoskey", "2", "", "", "","", "", "", "", "")
					                             	});
				case "DE GROENE DRAAK":
					return TokenMatch.CreateFullAndPartial(() => new HashSet<ConceptTerm>()
					                             	{
					                             		new ConceptTerm("skoskey", "3", "", "", "","", "", "", "", "")
					                             	});
				case "DE GROENE DRAAK OOK":
					return TokenMatch.CreatePartial();
				case "DE GROENE DRAAK OOK TANDEN":
					// no further hits, should fallback to 3
					return null;
				case "TANDEN":
					return TokenMatch.CreateFullAndPartial(() => new HashSet<ConceptTerm>()
					                             	{
					                             		new ConceptTerm("skoskey", "6", "", "","", "", "", "", "", "")
					                             	});
			}
			return null;
		}
	}

	[TestFixture]
	public class AnalyzerTests
	{
		[Test]
		public void Analyze_no_matching_terms()
		{
			// arrange
			var termAnalyzer = ExpandingTokenTermAnalyzerBuilder.BuildDefault();

			const string searchPhrase = @"heeft de groene draak ook tanden?";

			// act
			var results = termAnalyzer.Analyse(searchPhrase);

			// assert
			Assert.IsTrue(results.Count() == 0);
		}

		[Test]
		public void Analyze_de_groene_draak_complex()
		{
			// arrange
			var termAnalyzer = new ExpandingTokenTermAnalyzerBuilder()
							   {
								   ExpandingTokenMatcher = new ComplexStubTokenMatcher()
							   }.Build();

			const string searchPhrase = @"heeft de groene draak ook tanden?";

			// act
			var results = termAnalyzer.Analyse(searchPhrase).ToList();

			// assert
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual("DE GROENE DRAAK", results[0].Value);
			Assert.AreEqual("TANDEN", results[1].Value);
		}

		[Test]
		public void Analyze_de_groene_draak_complex_multi()
		{
			// arrange
			var termAnalyzer = new ExpandingTokenTermAnalyzerBuilder()
							   {
								   ExpandingTokenMatcher = new ComplexStubTokenMatcher()
							   }.Build();

			const string searchPhrase = "de groene hebben over de groene draak geschreven in de groene amsterdammer";

			// act
			var results = termAnalyzer.Analyse(searchPhrase);

			// assert
			Assert.AreEqual(3, results.Count());
			var resultHits = results.ToList();
			Assert.AreEqual("DE GROENE", resultHits[0].Value);
			Assert.AreEqual("DE GROENE DRAAK", resultHits[1].Value);
			Assert.AreEqual("DE GROENE AMSTERDAMMER", resultHits[2].Value);
		}

		[Test]
		public void Analyze_multiple_term_hits_should_return_all()
		{
			// arrange
			var termAnalyzer = new ExpandingTokenTermAnalyzerBuilder()
							   {
								   ExpandingTokenMatcher = new ComplexStubTokenMatcher()
							   }.Build();

			const string searchPhrase = "de groene hebben over de groene geschreven in de groene";

			// act
			var results = termAnalyzer.Analyse(searchPhrase);

			// assert
			Assert.AreEqual(3, results.Where(th => th.Value == "DE GROENE").Count(), "Incorrect number of concepts");
		}

		[Test]
		public void Analyze_one_term_with_multiple_matching_concepts_should_return_correct_number_of_concepts()
		{
			// arrange

			var matcher = new Mock<IExpandingTokenMatcher>();
			matcher.Setup(m => m.Match("DE")).Returns(TokenMatch.CreatePartial());
			matcher.Setup(m => m.Match("DE GROENE")).Returns(TokenMatch.CreateFull(() => new HashSet<ConceptTerm>
			                                                                             {
			                                                                             	new ConceptTerm("skoskey","1", "c1", "", "","", "", "", "", ""),
			                                                                             	new ConceptTerm("skoskey","2", "c2", "", "","", "", "", "", ""),
			                                                                             	new ConceptTerm("skoskey","3", "c3", "", "","", "", "", "", "")
			                                                                             }));
			var termAnalyzer = new ExpandingTokenTermAnalyzerBuilder()
							   {
								   ExpandingTokenMatcher = matcher.Object
							   }.Build();

			const string searchPhrase = "de groene hebben over de groene geschreven in de groene";

			// act
			var results = termAnalyzer.Analyse(searchPhrase);

			// assert
			Assert.AreEqual(3, termAnalyzer.TextMatches["DE GROENE"].ConceptTerms.Count());
		}

		[Test]
		public void Analyze_de_groene_draak_simpel()
		{
			// arrange
			var builder = new ExpandingTokenTermAnalyzerBuilder()
						  {
							  ExpandingTokenMatcher = new SimpleStubTokenMatcher()
						  };

			var termAnalyzer = builder.Build();

			const string text = @"heeft de groene draak ook tanden?";

			// act
			var results = termAnalyzer.Analyse(text);

			// assert
			Assert.AreEqual(1, results.Count(), "Missing hits");
			Assert.AreEqual("DE GROENE DRAAK", results.First().Value);
		}

		[Test]
		public void Analyze_term_sequence()
		{
			// arrange
			var termAnalyzer = new ExpandingTokenTermAnalyzerBuilder()
							   {
								   ExpandingTokenMatcher = new TwoSequencialMatchesStubTokenMatcher()
							   }.Build();

			const string searchPhrase = @"heeft de groene draak ook tanden?";

			// act
			var results = termAnalyzer.Analyse(searchPhrase);

			// assert
			Assert.AreEqual(2, results.Count());
			var resultHits = results.ToList();
			Assert.AreEqual("DE GROENE DRAAK", resultHits[0].Value);
			Assert.AreEqual("OOK TANDEN", resultHits[1].Value);
		}
	}
}
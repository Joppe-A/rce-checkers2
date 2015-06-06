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
	[TestFixture]
	public class TermEnricherTests
	{
		[Test]
		public void Analyze_should_return_enriched_term_hits()
		{
			// arrange
			const string searchPhrase = "de groene hebben over de groene draeck geschreven in de groene";
			
			var matcher = new Mock<IExpandingTokenMatcher>();
			matcher.Setup(m => m.Match("DE")).Returns(TokenMatch.CreatePartial());
			matcher.Setup(m => m.Match("DE GROENE")).Returns(TokenMatch.CreatePartial());
			matcher.Setup(m => m.Match("DE GROENE DRAECK"))
				.Returns(TokenMatch.CreateFull(
						() => new HashSet<ConceptTerm>
							{
								{ new EnrichedConceptTerm("skoskey", "1", "", "", "", "somePrefLabel", "broaderid", "broaderlabel", "", "somewordgroup", "the_domaim") }
							}
						)
				);

			var termAnalyzer = new ExpandingTokenTermAnalyzerBuilder()
			                   {
			                   		ExpandingTokenMatcher = matcher.Object
			                   }.Build();
			
			// act
			var result = termAnalyzer.Analyse(searchPhrase);

			// assert
			Assert.AreEqual(1, result.Count(), "Expected only one result");
			var deGroeneDraeck = result.SingleOrDefault(r => r.Value == "DE GROENE DRAECK");
			Assert.NotNull(deGroeneDraeck, "No hit found for enriched term de groene draeck");
			var textMatches = termAnalyzer.TextMatches[deGroeneDraeck.Value];
			Assert.IsInstanceOf(typeof(EnrichedConceptTerm), textMatches.ConceptTerms.Single(), "ConceptTerm has the wrong type");
		}
	}

}
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
using System.Xml.Linq;
using NUnit.Framework;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.Analyzer.Tokenizers;
using Trezorix.Checkers.DocumentChecker;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.Checkers.DocumentChecker.Processing;
using Trezorix.Checkers.DocumentChecker.Processing.Fragmenters;
using Trezorix.Checkers.DocumentCheckerApp.Helpers;
using System.Collections.Generic;

namespace DocumentCheckerAppTests
{
	[TestFixture]
	public class ResultXHTMLRendererTests
	{
		private ResultXHTMLRenderer _renderer;
		private List<ConceptTerm> _singleConceptTerm;

		[SetUp]
		public void Setup()
		{
			var conceptTerm = new ConceptTerm("skoskey", "the_concept_term_id", "the_concept_id", "value", "language", "conceptPrefLabel", "broaderid", "broaderlabel", "source", "type");
			_singleConceptTerm = new List<ConceptTerm>()
							   {
								{ conceptTerm }
							   }; 
		}

		[Test]
		public void Render()
		{
			// arrange
			var analysisResult = new AnalysisResult()
								 {
									FragmentTokenMatches = new List<FragmentTokenMatches>()
												   { 
													  new FragmentTokenMatches() 
													  {
														  TokenMatches = new List<Token>{ Token.Create("TERM", 6) }, 
														  Fragment = new Fragment("hello TERM bye", "/*[1]/*[1]/text()[1]") 
													  }
												   },
									TextMatches = new TextMatches() 
												  {
														new TextMatch("TERM")
														{ 
															ConceptTerms = _singleConceptTerm,
														}
												  }
								 };

			const string input =
				@"<div>
  <div>hello TERM bye</div>
</div>";
			_renderer = new ResultXHTMLRenderer(XDocument.Parse(input).Root, analysisResult);

			// act
			var result = _renderer.Render();

			// assert
			const string expectation =
@"<div>
  <div>hello <span class=""textmatch"" data-concept=""the_concept_id"" data-skossource=""skoskey"">TERM</span> bye</div>
</div>";
			
			Assert.AreEqual(expectation, result.ToString());

		}

		[Test]
		public void Render_textmatch_with_multiple_term_hits_should_span_one_concept()
		{
			// arrange
			IEnumerable<ConceptTerm> multipleConceptTerms = new List<ConceptTerm>
															{
																new ConceptTerm("skoskey", "concept_term_1", "concept_1", "value", "language", "conceptPrefLabel", "broaderid", "broaderlabel", "source", "type"),
																new ConceptTerm("skoskey", "concept_term_2", "concept_1", "value", "language", "conceptPrefLabel", "broaderid", "broaderlabel","source", "type")
															};
			var analysisResult = new AnalysisResult()
			{
				FragmentTokenMatches = new List<FragmentTokenMatches>()
										{ 
											new FragmentTokenMatches() 
												{
													TokenMatches = new List<Token>{ Token.Create("TERM", 6) }, 
													Fragment = new Fragment("hello TERM bye", "/*[1]/*[1]/text()[1]") 
												}
										},
				TextMatches = new TextMatches() 
								{
									new TextMatch("TERM")
									{ 
										ConceptTerms = multipleConceptTerms,
									}
								}
			};

			const string input =
				@"<div>
  <div>hello TERM bye</div>
</div>";
			_renderer = new ResultXHTMLRenderer(XDocument.Parse(input).Root, analysisResult);

			// act
			var result = _renderer.Render();

			// assert
			const string expectation =
@"<div>
  <div>hello <span class=""textmatch"" data-concept=""concept_1"" data-skossource=""skoskey"">TERM</span> bye</div>
</div>";

			Assert.AreEqual(expectation, result.ToString());

		}

		[Test]
		public void Render_textmatch_with_multiple_concept_hits_should_span_multiple_concepts()
		{
			// arrange
			IEnumerable<ConceptTerm> multipleConceptTerms = new List<ConceptTerm>
															{
																new ConceptTerm("skoskey", "concept_term_1", "concept_1", "value", "language", "conceptPrefLabel", "broaderid", "broaderlabel","source", "type"),
																new ConceptTerm("skoskey", "concept_term_1b", "concept_1", "value", "language", "conceptPrefLabel", "broaderid", "broaderlabel","source", "type"),
																new ConceptTerm("skoskey", "concept_term_2", "concept_2", "value", "language", "conceptPrefLabel", "broaderid", "broaderlabel","source", "type"),
																new ConceptTerm("skoskey", "concept_term_2b", "concept_2", "value", "language", "conceptPrefLabel", "broaderid", "broaderlabel","source", "type"),
															};
			var analysisResult = new AnalysisResult()
			{
				FragmentTokenMatches = new List<FragmentTokenMatches>()
												   { 
													  new FragmentTokenMatches() 
													  {
														  TokenMatches = new List<Token>{ Token.Create("TERM", 6) }, 
														  Fragment = new Fragment("hello TERM bye", "/*[1]/*[1]/text()[1]") 
													  }
												   },
				TextMatches = new TextMatches() 
												  {
													new TextMatch("TERM")
													{ 
														ConceptTerms = multipleConceptTerms,
													}
												  }
			};

			const string input =
				@"<div>
  <div>hello TERM bye</div>
</div>";
			_renderer = new ResultXHTMLRenderer(XDocument.Parse(input).Root, analysisResult);

			// act
			var result = _renderer.Render();

			// assert
			const string expectation =
@"<div>
  <div>hello <span class=""textmatch"" data-concept=""concept_2"" data-skossource=""skoskey""><span class=""textmatch"" data-concept=""concept_1"" data-skossource=""skoskey"">TERM</span></span> bye</div>
</div>";

			Assert.AreEqual(expectation, result.ToString());
		}

		[Test]
		public void Render_two_terms_in_one_textnode()
		{
			// arrange
			var analysisResult = new AnalysisResult()
			{
									FragmentTokenMatches = new List<FragmentTokenMatches>()
												   { 
													  new FragmentTokenMatches() 
													  {
														  TokenMatches = new List<Token>
																		 {
																			Token.Create("TERM1", 0),
																			Token.Create("TERM2", 10)
																		 }, 
														  Fragment = new Fragment("TERM1 and TERM2", "/*[1]/*[1]/text()[1]") 
													  }
												   },
									TextMatches = new TextMatches() 
												  {
													new TextMatch("TERM1")
													{ 
														ConceptTerms = _singleConceptTerm,
													},
													new TextMatch("TERM2")
													{ 
														ConceptTerms = _singleConceptTerm,
													}
												  }
								 };

			const string input =
				@"<div>
  <div>TERM1 and TERM2</div>
</div>";
			_renderer = new ResultXHTMLRenderer(XDocument.Parse(input).Root, analysisResult);

			// act
			var result = _renderer.Render();

			// assert
			const string expectation =
@"<div>
  <div><span class=""textmatch"" data-concept=""the_concept_id"" data-skossource=""skoskey"">TERM1</span> and <span class=""textmatch"" data-concept=""the_concept_id"" data-skossource=""skoskey"">TERM2</span></div>
</div>";

			Assert.AreEqual(expectation, result.ToString());

		}

		[Test]
		public void Render_two_fragments()
		{
			// arrange
			
			var analysisResult = new AnalysisResult()
			{
				FragmentTokenMatches = new List<FragmentTokenMatches>()
												   { 
													  new FragmentTokenMatches() 
													  {
														  TokenMatches = new List<Token>
																		 {
																			Token.Create("TERM", 0)
																		 }, 
														  Fragment = new Fragment("TERM ", "/*[1]/*[1]/text()[1]") 
													  },
													  new FragmentTokenMatches()
													  {
															TokenMatches = new List<Token>
																		 {
																			Token.Create("TERM", 5)
																		 },
															Fragment = new Fragment(" and TERM", "/*[1]/*[1]/text()[2]")
													  }
												   },
				TextMatches = new TextMatches() 
												  {
													new TextMatch("TERM")
													{ 
														ConceptTerms = _singleConceptTerm,
													}
												  }
			};

			const string input =
@"<div>
  <div>TERM <span>andere node</span> and TERM</div>
</div>";
			_renderer = new ResultXHTMLRenderer(XDocument.Parse(input).Root, analysisResult);

			// act
			var result = _renderer.Render();

			// assert
			const string expectation =
@"<div>
  <div><span class=""textmatch"" data-concept=""the_concept_id"" data-skossource=""skoskey"">TERM</span> <span>andere node</span> and <span class=""textmatch"" data-concept=""the_concept_id"" data-skossource=""skoskey"">TERM</span></div>
</div>";

			Assert.AreEqual(expectation, result.ToString());

		}
		
		// ToDo: How does Render fail when the input doesn't match the fragments at all?
		
	}
}

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
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.Analyzer.Tokenizers;
using Trezorix.Checkers.ExcelXmlChecker;
using Trezorix.Checkers.ExcelXmlChecker.Export;
using Trezorix.Testing.Common.System;

namespace ExcelXmlCheckerTests
{
	[TestFixture]
	public class ExporterTests
	{
		private AnalysisResult _analysisResult;
		private Exporter _exporter;

		[SetUp]
		public void Setup()
		{
			var textMatches = new TextMatches();
			textMatches.Add(new TextMatch("textterm1") { ConceptTerms = new List<ConceptTerm>() { new ConceptTerm() { ConceptId = "1", ConceptLabel = "concept 1" } } });
			textMatches.Add(new TextMatch("textterm2") { ConceptTerms = new List<ConceptTerm>() { new ConceptTerm() { ConceptId = "1", ConceptLabel = "concept 1" } } });
			textMatches.Add(new TextMatch("textterm3") { ConceptTerms = new List<ConceptTerm>() { new ConceptTerm() { ConceptId = "2", ConceptLabel = "concept 2" } } });

			_analysisResult = new AnalysisResult { TextMatches = textMatches };
			_analysisResult.AddRowMatches("row1", new List<Token>()
			                                      	{
			                                      		Token.Create("textterm1"), 
														Token.Create("textterm2"),
														Token.Create("textterm3")
			                                      	}
													, "concept 1 term");

			_analysisResult.AddRowMatches("row2", new List<Token>() { Token.Create("textterm3") }, "concept 2 term");

			_exporter = new Exporter();
		}
		
		// ToDo: this test is not exhaustive, testing multiple aspects in one test

		[Test]
		public void PrepareExportModel_returns_list_of_row_Ids_containing_concatenated_concept_hit_data()
		{
			// arrange
			
			// act
			var result = _exporter.PrepareExportModel(_analysisResult);

			// assert
			Assert.AreEqual(2, result.Rows.Count);
			Assert.AreEqual("row1", result.Rows[0].SourceUri);
			Assert.AreEqual("row2", result.Rows[1].SourceUri);

			Assert.AreEqual("concept 1|concept 2", result.Rows[0].Literals);
			Assert.AreEqual("concept 2", result.Rows[1].Literals);
		}

		[Test]
		[Category("Integration")]
		public void Export_should_write_file()
		{
			// arrange
			
			var outputFile = FileTestHelpers.GetTestDataDir() + "\\result.xml";

			// act
			_exporter.Export(outputFile, _analysisResult);

			// assert
			Assert.IsTrue(File.Exists(outputFile));
		}

		
	}
}

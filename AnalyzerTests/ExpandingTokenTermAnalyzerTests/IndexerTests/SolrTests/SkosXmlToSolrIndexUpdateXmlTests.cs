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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Moq;
using NUnit.Framework;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.Analyzer.Indexes;
using Trezorix.Checkers.Analyzer.Indexes.Solr;
using Trezorix.Checkers.Analyzer.Indexes.Solr.SkosXmlTransformer;
using Trezorix.Checkers.Analyzer.Matchers;
using Trezorix.Checkers.Common.Logging;

// ToDo: Branch out this class, too many different tests

namespace AnalyzerTests.ExpandingTokenTermAnalyzerTests.IndexTests
{
	[TestFixture]
	public class SkosXmlToSolrIndexUpdateXmlTests
	{
		private XmlReader _skosXmlInput;
		private string _solrUrl;
		private SkosXmlToSolrIndexUpdateXml _skosToSolr;
		private Mock<ITermEnricher> _mockTermEnricher;
		private readonly ILog _dummyLogger = new Mock<ILog>().Object;

		[SetUp]
		public void Setup()
		{
			_solrUrl = "http://localhost:8080/checker2solr";
		}

		public void SetupAbraAlbaSkosXml()
		{
			string xmlInput =
				@"<nodeList xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#"" xmlns:rna=""http://www.rnaproject.org/data/"" xmlns:skos=""http://www.w3.org/2004/02/skos/core#"" xmlns:rnax=""http://www.rnaproject.org/data/rnax/"" contentItemUri=""http://www.rnaproject.org/data/49cb6c6d-8b85-4677-beee-9c582e7a4b31"" contentItemName=""Zygoribatula propinquus"" rna:numberOfChildren=""0"">
<skos:Concept rdf:about=""http://www.rnaproject.org/data/370a46a5-fb86-4763-b24d-72e698cf9891"">
<skos:prefLabel xml:lang=""sci"">Abra alba</skos:prefLabel>
<skos:prefLabel xml:lang=""dut"">witte dunschaal</skos:prefLabel>
<skos:altLabel xml:lang=""dut"">BoneInfo.vondsttype.486</skos:altLabel>
<skos:broader rdf:resource=""http://www.rnaproject.org/data/2d94b527-62ca-42ad-a69c-39caf5f5b5d9""/>
</skos:Concept></nodeList>";

			SetupSkosToSolr(xmlInput);
		}

		private void SetupDeGroeneSkosXml()
		{
			string xmlInput =
				@"<nodeList xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#"" xmlns:rna=""http://www.rnaproject.org/data/"" xmlns:skos=""http://www.w3.org/2004/02/skos/core#"" xmlns:rnax=""http://www.rnaproject.org/data/rnax/"" contentItemUri=""http://www.rnaproject.org/data/49cb6c6d-8b85-4677-beee-9c582e7a4b31"" contentItemName=""Zygoribatula propinquus"" rna:numberOfChildren=""0"">
<skos:Concept rdf:about=""http://www.rnaproject.org/data/370a46a5-fb86-4763-b24d-72e698cf9a891"">
<skos:prefLabel xml:lang=""sci"">De groene Amsterdammer</skos:prefLabel>
<skos:prefLabel xml:lang=""dut"">De groene Amsterdammer</skos:prefLabel>
<skos:altLabel xml:lang=""dut"">De groene</skos:altLabel>
<skos:broader rdf:resource=""http://www.rnaproject.org/data/2d94b527-62la-a42ad-a69c-39caf5f5b5d9""/>
</skos:Concept>
<skos:Concept rdf:about=""http://www.rnaproject.org/data/370a46a6-fb86-4763-b24da72e698ff9891"">
<skos:prefLabel xml:lang=""sci"">De groene draak</skos:prefLabel>
<skos:prefLabel xml:lang=""dut"">De groene draak</skos:prefLabel>
<skos:broader rdf:resource=""http://www.rnaproject.org/data/2d94b527-62ca-42ad-a6a9c-39caf5f5b5d9""/>
</skos:Concept>
<skos:Concept rdf:about=""http://www.rnaproject.org/data/370a46b5-afb86-4763-b24d-72e698cf9z91"">
<skos:prefLabel xml:lang=""sci"">De groene</skos:prefLabel>
<skos:prefLabel xml:lang=""dut"">De groene</skos:prefLabel>
<skos:broader rdf:resource=""http://www.rnaproject.org/data/2d94b527-62ca-42aad-a69c-39xaf5f5b5d9""/>
</skos:Concept>
</nodeList>";

			SetupSkosToSolr(xmlInput);
		}

		private void SetupSkosToSolr(string xmlInput)
		{
			_skosXmlInput = new XmlTextReader(new StringReader(xmlInput));

			_mockTermEnricher = new Mock<ITermEnricher>();

			_skosToSolr = new SkosXmlToSolrIndexUpdateXml(_mockTermEnricher.Object);
		}

		[Test]
		[Category("Integration")]
		public void CreateSolrIndexUpdateXml_given_a_SkosXml_file_should_feed_terms_to_indexupdatefile()
		{
			// arrange
			SetupAbraAlbaSkosXml();

			// act
			var resultDoc = _skosToSolr.CreateSolrIndexUpdateXml("id", "dictionarycollection", _skosXmlInput);
	
			// assert
			var docDump = resultDoc.ToString();
			var result = resultDoc.XPathSelectElements(@"add/doc[field[@name='doctype'] = 'partial']");
			
			Assert.IsTrue(result.Any(), "Solr xml missing partial_term fields: " + docDump);

			AssertHasPartial(result, "ABRA", docDump);

			AssertHasPartial(result, "WITTE", docDump);

			AssertHasPartial(result, "BONEINFO", docDump);
			AssertHasPartial(result, "BONEINFO VONDSTTYPE", docDump);

			result = resultDoc.XPathSelectElements("add/doc[field[@name='doctype' and .='full']]");
			
			Assert.IsTrue(result.Any(), "Solr xml missing full_term fields: " + docDump);

			AssertHasFull(result, "ABRA ALBA", docDump);
			AssertHasFull(result, "WITTE DUNSCHAAL", docDump);
			AssertHasFull(result, "BONEINFO VONDSTTYPE 486", docDump);

		}

		[Test]
		[Category("Integration")]
		public void CreateSolrIndexUpdateXml_given_a_SkosXml_file_should_index_concept_names_based_on_language()
		{
			// arrange
			SetupAbraAlbaSkosXml();

			// act
			var resultDoc = _skosToSolr.CreateSolrIndexUpdateXml("id", "dictionarycollection", _skosXmlInput);

			// assert
			var docDump = resultDoc.ToString();
			var result = resultDoc.XPathSelectElements(@"add/doc[field[@name='doctype'] = 'full']");

			Assert.IsTrue(result.Any(), "Solr xml missing full term fields: " + docDump);

			//var resultNode = result.Any(
			//    e => (string)e.XPathSelectElement("field[@name='literal_form']") == "BoneInfo.vondsttype.486" 
			//        && "field[@name='skos_concept_label']") == "witte dunschaal" 
			//    );

			//resultNode.
			Assert.Inconclusive();

		}
		[Test]
		[Category("Integration")]
		public void CreateSolrIndexUpdateXml_given_a_SkosXml_file_should_feed_terms_to_indexupdatefile_including_term_enrichments()
		{
			// arrange
			SetupDeGroeneSkosXml();

			_mockTermEnricher.Setup(te => te.Enrich("DE GROENE DRAAK", "dut", null)).Returns(new List<TermEnrichment>()
				{
					new TermEnrichment(null, null, 2) { Terms = new List<string>()
						{ "DE VLIEGENDE GROENE DRAECK", "HET GROENE DRAAKJE", "DE GROENE DRAECK" }
					}
				}
			);

			// act
			var resultDoc = _skosToSolr.CreateSolrIndexUpdateXml(null, null, _skosXmlInput);
			
			// assert
			var docDump = resultDoc.ToString();
			
			var result = resultDoc.XPathSelectElements(@"add/doc[field[@name='doctype'] = 'partial']");
			
			Assert.IsTrue(result.Any(), "Solr xml missing partial_term fields: " + docDump);

			AssertHasPartial(result, "DE", docDump);
			AssertHasPartial(result, "DE GROENE", docDump);
			AssertHasPartial(result, "DE VLIEGENDE", docDump);
			AssertHasPartial(result, "DE VLIEGENDE GROENE", docDump);
			AssertHasPartial(result, "HET", docDump);
			AssertHasPartial(result, "HET GROENE", docDump);

			result = resultDoc.XPathSelectElements("add/doc[field[@name='doctype' and .='full']]");
			
			Assert.IsTrue(result.Any(), "Solr xml missing full_term fields: " + docDump);

			AssertHasTermEnrichment(result, "DE VLIEGENDE GROENE DRAECK", docDump);
			AssertHasTermEnrichment(result, "HET GROENE DRAAKJE", docDump);
			AssertHasTermEnrichment(result, "DE GROENE DRAECK", docDump);
		}

		private static void AssertHasPartial(IEnumerable<XElement> elements, string partial, string fullDoc)
		{
			AssertHasTerm(elements, partial, "partial", fullDoc);
		}

		private static void AssertHasTerm(IEnumerable<XElement> elements, string term, string docType, string fullDoc)
		{
			var result = elements.Any(
				e => (string)e.XPathSelectElement("field[@name='term']") == term 
					&& (string)e.XPathSelectElement("field[@name='doctype']") == docType
				);
			Assert.IsTrue(result, string.Format("No '{0}' term value {1} found in result set.\r\n{2}", docType, term, fullDoc));
		}

		private static void AssertHasFull(IEnumerable<XElement> elements, string full, string fullDoc)
		{
			AssertHasTerm(elements, full, "full", fullDoc);
		}

		private static void AssertHasTermEnrichment(IEnumerable<XElement> elements, string term, string fullDoc)
		{
			var result = elements.Any(
				e => (string)e.XPathSelectElement("field[@name='term']") == term
					&& (string)e.XPathSelectElement("field[@name='type']") == "TermEnrichment"
				);
			Assert.IsTrue(result, string.Format("No 'TermEnrichment' term value {0} found in result set.\r\n{1}", term, fullDoc));
		}

		[Test]
		[Category("System")]
		public void SolrQuery()
		{
			//ToDo: Not repeatable, ABRA ALBA has a partial form now after changing the skos data

			// arrange
			SetupAbraAlbaSkosXml();

			var doc = _skosToSolr.CreateSolrIndexUpdateXml("id", "dictionarycollection", _skosXmlInput);

			var solrIndexer = new SolrIndex(_solrUrl);

			solrIndexer.Update(doc);
			solrIndexer.Commit();

			var solrIndex = new SolrExpandingTokenMatcher(new SolrIndex(_solrUrl));

			// act
			var result = solrIndex.Match("ABRA ALBA");

			// assert
			Assert.IsNotNull(result);
			Assert.AreEqual(MatchType.FullAndPartial, result.MatchType);
			var terms = result.TermMatches.ToList();

			Assert.IsTrue(terms.Any(tm => tm.Id == "http://www.rnaproject.org/data/370a46a5-fb86-4763-b24d-72e698cf9891_1"), "Didn't find expected termid");
		}

		[Test]
		[Category("System")]
		public void IndexSkos_De_groene_Amsterdammer_De_groene_draak_De_groene()
		{
			// arrange
			SetupDeGroeneSkosXml();

			var doc = _skosToSolr.CreateSolrIndexUpdateXml("id", "dictionarycollection", _skosXmlInput);
			var solrIndexer = new SolrIndex(_solrUrl);
			
			solrIndexer.Update(doc);
			solrIndexer.Commit();

			const string text = @"De groene wisten niet dat de groene draak wel eens in de de groene amsterdammer was beschreven.";
			
			var analyzer = new ExpandingTermAnalyzer(new SolrExpandingTokenMatcher(new SolrIndex(_solrUrl)), null, _dummyLogger);
			
			// act
			var result = analyzer.Analyse(text);

			// assert
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Any(t => t.Value == "DE GROENE"));
			Assert.IsTrue(result.Any(t => t.Value == "DE GROENE DRAAK"));
			Assert.IsTrue(result.Any(t => t.Value == "DE GROENE AMSTERDAMMER"));
			
			// ToDo: Test more cases
		}

		[Test]
		[Category("System")]
		public void IndexSkos_with_TermMatcher_should_return_dictionary_alternatives()
		{
			// arrange
			SetupDeGroeneSkosXml();

			_mockTermEnricher.Setup(te => te.Enrich("DE GROENE", "dut", It.IsAny<string>())).Returns(new List<TermEnrichment>()
				{
					new TermEnrichment(null, null, 1) { Terms = new List<string>()
						{ "DE SOCIALISTEN" }
					},
					new TermEnrichment(null, null, 3) { Terms = new List<string>()
						{ "DE GROENE AMSTERDAMMER" }
					}
				}
			);

			_mockTermEnricher.Setup(te => te.Enrich("DE GROENE DRAAK", "dut", It.IsAny<string>())).Returns(new List<TermEnrichment>()
				{
					new TermEnrichment(null, null,2) { Terms = new List<string>()
						{ "DE VLIEGENDE GROENE DRAECK", "HET GROENE DRAAKJE", "DE GROENE DRAECK" }
					}
				}
			);

			_mockTermEnricher.Setup(te => te.Enrich("DE GROENE AMSTERDAMMER", "dut", It.IsAny<string>())).Returns(new List<TermEnrichment>()
				{
					new TermEnrichment(null, null, 3) { Terms = new List<string>()
						{ "WEEKBLAD DE GROENE AMSTERDAMMER", "DE GROENE AMSTERDAMMER", "DE GROENE" }
					}
				}
			);

			var doc = _skosToSolr.CreateSolrIndexUpdateXml("id", "dictionarycollection", _skosXmlInput);
			
			var solrIndexer = new SolrIndex(_solrUrl);
			solrIndexer.Update(doc);
			solrIndexer.Commit();

			const string text = @"De socialisten wisten niet dat de Groene Draeck wel eens in weekblad de groene amsterdammer was beschreven.";

			var analyzer = new ExpandingTermAnalyzer(new SolrExpandingTokenMatcher(new SolrIndex(_solrUrl), new MatcherFilter(){ Language = "dut" }), null, _dummyLogger);
			
			// act
			var result = analyzer.Analyse(text);

			// assert
			var resultSet = result.ToList();
			
			Assert.AreEqual(3, resultSet.Count);
			Assert.AreEqual("DE SOCIALISTEN", resultSet[0].Value);
			Assert.AreEqual("DE GROENE DRAECK", resultSet[1].Value);
			Assert.AreEqual("WEEKBLAD DE GROENE AMSTERDAMMER", resultSet[2].Value);

			// ToDo: Test more cases (multiple dictionarycollections, multiple hits within one dictionarycollection or within collectionless)
		}

	}
}
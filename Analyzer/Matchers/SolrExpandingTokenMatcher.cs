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
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Trezorix.Checkers.Analyzer.Indexes.Solr;

namespace Trezorix.Checkers.Analyzer.Matchers
{
	public class SolrExpandingTokenMatcher : IExpandingTokenMatcher
	{
		private const string ENRICHED_TERM_TYPE = "enrichedterm";
		private readonly SolrIndex _solrIndex;
		
		private NameValueCollection _skosSourceInclusionFilter;

		public SolrExpandingTokenMatcher(SolrIndex solrIndex)
		{
			_solrIndex = solrIndex;
			CreateSolrQueryFromFilter(new MatcherFilter());
		}

		public SolrExpandingTokenMatcher(SolrIndex solrIndex, MatcherFilter filter)
		{
			_solrIndex = solrIndex;
			CreateSolrQueryFromFilter(filter);
		}

		private void CreateSolrQueryFromFilter(MatcherFilter filter)
		{
			string searchFilter = string.Empty;
			if (filter.Language != null)
			{
				searchFilter = string.Format("+xml_lang:{0} ", filter.Language);
			}

			_skosSourceInclusionFilter = new NameValueCollection(filter.SkosSources.Count() + 1);

			if (filter.SkosSources.Any())
			{
				var filterKeysString = string.Join(" OR ", filter.SkosSources.Select(s => @"""" + s + @""""));
				// Note: partial terms are not associated with any skos source (to allow sharing), so allowing partial results in filter
				searchFilter += string.Format("+(skos_key:({0}) OR doctype:partial)", filterKeysString);
			}

			_skosSourceInclusionFilter.Add("fq", searchFilter);
		}

		public TokenMatch Match(string token)
		{
			// parse Xml; retrieve partials & full count
			XDocument matched = SolrSelectTermHitCount(token);

			var fullMatches = matched.XPathSelectElement("//int[@name='full']");

			bool haveFullMatches = false;
			int rowCount = 0;
			if (fullMatches != null)
			{
				rowCount = (int)fullMatches;
				if (rowCount > 0)
				{
					haveFullMatches = true;
				}
			}

			var partialMatches = matched.XPathSelectElement("//int[@name='partial']");

			bool havePartials = partialMatches != null && (int)partialMatches > 0;

			if (havePartials)
			{
				if (!haveFullMatches)
				{
					return TokenMatch.CreatePartial();	
				}

				return TokenMatch.CreateFullAndPartial(() => DefferedFullResult(token, rowCount));
			}
			if (haveFullMatches)
			{
				return TokenMatch.CreateFull(() => DefferedFullResult(token, rowCount));
			}
			return null;
		}

		private IEnumerable<ConceptTerm> DefferedFullResult(string token, int rowCount)
		{
			var result = SolrSelectFullTerms(token, rowCount);

			foreach (var solrDoc in result.XPathSelectElements("//doc"))
			{
				// ToDo: Performance: We should pass the XML node by node (using Xmlreader) and pick up relevant values, instead of the other way around
				var type = solrDoc.XPathSelectElement("str[@name='type']").Value;

				string skosSourceKey = solrDoc.XPathSelectElement("str[@name='skos_key']").Value;

				string id = solrDoc.XPathSelectElement("str[@name='id']").Value;
				string language = solrDoc.XPathSelectElement("str[@name='xml_lang']").Value;
				string source = solrDoc.XPathSelectElement("str[@name='source']").Value;
				string conceptId = solrDoc.XPathSelectElement("str[@name='skos_concept']").Value;
				string literal = solrDoc.XPathSelectElement("str[@name='literal_form']").Value;
				string conceptLabel = solrDoc.XPathSelectElement("str[@name='skos_concept_label']").Value;
				string broaderId = solrDoc.XPathSelectElement("str[@name='skos_broader']").Value;
				string broaderLabel = solrDoc.XPathSelectElement("str[@name='skos_broader_label']").Value;
				
				switch(type.ToLower())
				{
					case ENRICHED_TERM_TYPE :
						
						string wordGroup = solrDoc.XPathSelectElement("str[@name='wordgroup']").Value;
						string dictionaryCollection = solrDoc.XPathSelectElement("str[@name='dictionarycollection']").Value;

						yield return new EnrichedConceptTerm(skosSourceKey, 
											id,
											conceptId,
											literal,
											language,
											conceptLabel,
											broaderId,
											broaderLabel,
											source,
											wordGroup,
											dictionaryCollection);
						break;
					default:

						yield return new ConceptTerm(skosSourceKey,
						                               id,
						                               conceptId,
						                               literal,
						                               language,
						                               conceptLabel,
						                               broaderId,
						                               broaderLabel,
						                               source,
						                               type);
						break;
				}
			}
			
		}

		private XDocument SolrSelectFullTerms(string token, int rowCount)
		{
			// TI: Could make a Solr search provider so we can skip some of these parameters

			/* http://localhost:8080/checker2solr/select/?
			 * q=term%3A%22DE+GROENE+AMSTERDAMMER%22
			 * &version=2.2
			 * &start=0
			 * &rows=0
			 * &indent=on
			*/

			var searchParameters = new NameValueCollection()
			                       	{
			                       		{"q", "full_term:\"" + token + "\""},
			                       		{"rows", rowCount.ToString()}
									};

			if (_skosSourceInclusionFilter != null)
			{
				searchParameters.Add(_skosSourceInclusionFilter);
			}
			
			return _solrIndex.SolrSelectXml(searchParameters);
		}

		private XDocument SolrSelectTermHitCount(string token)
		{
			// TI: Could make a Solr search provider so we can skip some of these parameters

			/* http://localhost:8080/checker2solr/select/?
			 * q=term%3A%22DE+GROENE+AMSTERDAMMER%22
			 * &version=2.2
			 * &start=0
			 * &rows=0
			 * &indent=on
			 * &facet=true
			 * &facet.field=doctype
			*/

			var searchParameters = new NameValueCollection()
			                       	{
			                       		{"q", "term:\"" + token + "\"" },
			                       		{"rows", "0"},
			                       		{"facet", "true"},
			                       		{"facet.field", "doctype"}
			                       	};

			if (_skosSourceInclusionFilter != null)
			{
				searchParameters.Add(_skosSourceInclusionFilter);
			}

			return _solrIndex.SolrSelectXml(searchParameters);
		}

	}

}

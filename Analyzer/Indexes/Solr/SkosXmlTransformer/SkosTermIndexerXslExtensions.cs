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
using System.Xml.Linq;
using System.Xml.XPath;
using Trezorix.Checkers.Analyzer.Tokenizers;

namespace Trezorix.Checkers.Analyzer.Indexes.Solr.SkosXmlTransformer
{
	public class SkosTermIndexerXslExtensions
	{
		private readonly ISet<PartialItem> _partialCache = new HashSet<PartialItem>();
			
		private readonly ITermEnricher _termEnricher;
		private readonly string _dictionaryCollectionName;
			
		private XslExtensionResultNodeSet _resultXslExtensionResultNodeSet;
		private string _language;
		private string _source;
		
		private readonly object _locker = new object();

		public SkosTermIndexerXslExtensions(string dictionaryCollectionName, ITermEnricher termEnricher)
		{
			_termEnricher = termEnricher;
			_dictionaryCollectionName = dictionaryCollectionName;
		}

		// used from Xslt
		public XPathNavigator Term(string term, string language, string source)
		{
			// ToDo: Extract a term processor class?

			if (string.IsNullOrWhiteSpace(term))
			{
				return new XDocument().CreateNavigator();
			}

			// prevent reentrance, using field based state that is used per call to this method.
			// ToDo: Why do we need fields?
			lock(_locker)
			{
				string[] termTokens = TermTokens(term);
				string normalizedTerm = String.Join(" ", termTokens);

				_source = source;
				_language = language;
				_resultXslExtensionResultNodeSet = new XslExtensionResultNodeSet();

				// add tokenized term itself
				ProcessFullterm(termTokens, normalizedTerm);

				// add term enrichment unless the term starts with a capital letter (it's assumed to be a name then)
				if (!Char.IsUpper(term[0]))
				{
					var filteredWordGroups = _termEnricher.Enrich(normalizedTerm, language, _dictionaryCollectionName);
					ProcessEnrichments(filteredWordGroups);
				}

				return _resultXslExtensionResultNodeSet.Navigator();
			}
			
		}

		private void ProcessFullterm(IEnumerable<string> termTokens, string tokenizedTerm)
		{
			_resultXslExtensionResultNodeSet.CreateFullTerm(tokenizedTerm);
				
			// partials for full term
			AnalyzePartialsForTokenizedTerm(termTokens);
		}

		private void ProcessEnrichments(IEnumerable<TermEnrichment> filteredWordGroups)
		{
			if (filteredWordGroups != null)
			{
				foreach (var enrichment in filteredWordGroups)
				{
					foreach (var group in enrichment.Terms)
					{
						string[] termTokens = TermTokens(group);
						string tokenizedEnrichment = String.Join(" ", termTokens);

						_resultXslExtensionResultNodeSet.CreateEnrichedTerm(enrichment, tokenizedEnrichment);

						// partials for term enrichment
						AnalyzePartialsForTokenizedTerm(termTokens);
					}
				}
			}
		}

		private static string[] TermTokens(string term)
		{
			var tokenizer = new TermTokenizer(term);
					
			return tokenizer.Tokens().Select(tt => tt.Value).ToArray();
		}

		private void AnalyzePartialsForTokenizedTerm(IEnumerable<string> termTokens)
		{
			
			var partials = CreatePartials(termTokens);
				
			foreach (var partial in partials)
			{
				var candidatePartial = new PartialItem()
				                       	{
				                       		language = _language,
				                       		term = partial,
											source = _source
				                       	};

				if (_partialCache.Contains(candidatePartial))
				{
					// the partials are registered from longest to smallest, if we have this partial in cache
					// we also have to smaller fragments already
					break;
				}
				_partialCache.Add(candidatePartial);

				_resultXslExtensionResultNodeSet.CreatePartial(partial);
			}
		}

		private struct PartialItem
		{
			public string term;
			public string source;
			public string language;
		}

		// Protected for testing access
		protected IEnumerable<string> CreatePartials(IEnumerable<string> tokens)
		{
			string[] termTokens = tokens.ToArray();
			var termExpansionTokenCount = tokens.Count();

			// we generate from large to small, this order is more efficient for partial registration
			for (int expandTill = termExpansionTokenCount - 1; expandTill > 0; expandTill--)
			{
				var expandedTokens = string.Join(" ", termTokens, 0, expandTill);
				yield return expandedTokens;
			}
		}

		
	}
}
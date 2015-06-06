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
using System.Xml.XPath;

namespace Trezorix.Checkers.Analyzer.Indexes.Solr.SkosXmlTransformer
{
	public class XslExtensionResultNodeSet
	{
		// FI: Pass these constants as Xslt variables to keep things DRY?
		private const string PARTIAL_TERM_ELEMENT_NAME = "partial";
		private const string PARTIAL_TERM_VALUE_ELEMENT_NAME = "partialterm";

		private const string ENRICHED_TERM_ELEMENT_NAME = "termenrichment";
		private const string ENRICHED_TERM_VALUE_ELEMENT_NAME = "enrichedterm";
		private const string ENRICHED_TERM_DICTIONARYCOLLECTION_VALUE_ELEMENT_NAME = "dictionarycollection";
		private const string ENRICHED_TERM_WORDGROUP_VALUE_ELEMENT_NAME = "wordgroup";

		private const string FULL_TERM_ELEMENT_NAME = "full";
		private const string FULL_TERM_VALUE_ELEMENT_NAME = "term";
		
		private readonly XElement _root;
		private readonly XDocument _doc;

		public XslExtensionResultNodeSet()
		{
			_root = new XElement("root");
			_doc = new XDocument();
			_doc.AddFirst(_root);
		}

		public XPathNavigator Navigator()
		{
			return _doc.CreateNavigator();
		}

		public void CreateFullTerm(string tokenizedTerm)
		{
			_root.Add(new XElement(FULL_TERM_ELEMENT_NAME,
						new XElement(FULL_TERM_VALUE_ELEMENT_NAME, tokenizedTerm)
						)
				);
		}

		public void CreateEnrichedTerm(TermEnrichment filteredWordGroup, string tokenizedEnrichment)
		{
			_root.Add(new XElement(ENRICHED_TERM_ELEMENT_NAME,
									new XElement(ENRICHED_TERM_VALUE_ELEMENT_NAME, tokenizedEnrichment),
									new XElement(ENRICHED_TERM_WORDGROUP_VALUE_ELEMENT_NAME, filteredWordGroup.GroupId),
									new XElement(ENRICHED_TERM_DICTIONARYCOLLECTION_VALUE_ELEMENT_NAME, filteredWordGroup.DictionaryCollectionName))
				);
		}

		public void CreatePartial(string term)
		{
			_root.Add(new XElement(PARTIAL_TERM_ELEMENT_NAME,
								   new XElement(PARTIAL_TERM_VALUE_ELEMENT_NAME, term))
				);
		}
	}
}
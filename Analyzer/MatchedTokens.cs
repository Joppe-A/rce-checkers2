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
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Trezorix.Checkers.Analyzer
{
	[CollectionDataContract(ItemName = "TextMatch", Name = "TextMatches")]
	public class TextMatches : KeyedCollection<string, TextMatch>
	{
		internal TextMatch RegisterTokenHit(string text, IEnumerable<ConceptTerm> conceptTerms, IEnumerable<string> stopWordLanguages)
		{
			var added = new TextMatch(text)
			            {
			            	ConceptTerms = conceptTerms,
			            	StopWordLanguages = stopWordLanguages
			            };

			Add(added);

			return added;
		}

		protected override string GetKeyForItem(TextMatch item)
		{
			return item.MatchedText;
		}
	}

	[DataContract]
	public class TextMatch
	{
		[DataMember]
		public string MatchedText { get; private set; }

		private List<ConceptTerm> _conceptTerms;

		[DataMember]
		public IEnumerable<ConceptTerm> ConceptTerms
		{
			get { return _conceptTerms; }
			set { _conceptTerms = value.ToList(); }
		}

		private List<string> _stopWordLanguages;

		[DataMember]
		public IEnumerable<string> StopWordLanguages
		{
			get { return _stopWordLanguages; }
			set { _stopWordLanguages = value.ToList(); }
		}

		public bool IsPotentialStopWord
		{
			get { return StopWordLanguages.Count() > 0; }
		}
		
		public TextMatch(string matchedText)
		{
			MatchedText = matchedText;
		}

	}

}

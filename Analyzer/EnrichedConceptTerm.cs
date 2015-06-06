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
using System.Runtime.Serialization;

namespace Trezorix.Checkers.Analyzer
{
	[DataContract]
	public class EnrichedConceptTerm : ConceptTerm
	{
		[DataMember]
		public string WordGroup { get; set; }

		[DataMember]
		public string DictionaryCollectionName { get; private set; }

		public EnrichedConceptTerm(string skosSourceKey, string id, string conceptId, string literal, string language, string conceptLabel, string broaderId, string broaderLabel, string source, string wordGroup, string dictionaryCollectionName)
			: base(skosSourceKey, id, conceptId, literal, language, conceptLabel, broaderId, broaderLabel, source, "enrichedterm")
		{
			WordGroup = wordGroup;
			DictionaryCollectionName = dictionaryCollectionName;
		}

	}
}
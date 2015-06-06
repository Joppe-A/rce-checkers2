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

namespace Trezorix.Checkers.Analyzer.Indexes
{
	public class TermEnrichment : IEquatable<TermEnrichment>
	{
		public TermEnrichment(string dictionaryCollectionName, string dictionary, long identifier)
		{
			DictionaryCollectionName = dictionaryCollectionName;
			Dictionary = dictionary;
			GroupId = identifier;
		}

		private List<string> _terms = new List<string>();

		public long GroupId { get; set; }

		public string Dictionary { get; set; }

		public string DictionaryCollectionName { get; set; }

		public IEnumerable<string> Terms
		{
			get { return _terms; }
			set
			{
				_terms = value.ToList();
			}
		}

		public void AddTerm(string term)
		{
			if (_terms == null)
			{
				_terms = new List<string>();
			}
				
			_terms.Add(term);
		}

		public bool Equals(TermEnrichment other)
		{
			bool isEqual = ((GroupId == other.GroupId) && (DictionaryCollectionName == other.DictionaryCollectionName));
			return isEqual;
		}
	}
}

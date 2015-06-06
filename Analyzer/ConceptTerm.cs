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
using System.Runtime.Serialization;

namespace Trezorix.Checkers.Analyzer
{
	[DataContract]
	public class ConceptTerm : IEquatable<ConceptTerm>
	{
		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public string ConceptId { get; set; }

		[DataMember]
		public string SkosSourceKey { get; set; }

		[DataMember]
		public string Literal { get; set; }

		[DataMember]
		public string ConceptLabel { get; set; }

		[DataMember]
		public string BroaderId { get; set; }

		[DataMember]
		public string BroaderLabel { get; set; }
		
		[DataMember]
		public string Language { get; set; }

		[DataMember]
		public string Source { get; set; }
		
		[DataMember]
		public string Type { get; set; }

		public ConceptTerm()
		{
			; // needed for serialization
		}

		public ConceptTerm(string skosSourceKey, string id, string conceptId, string literal, string language, string conceptLabel, string broaderId, string broaderLabel, string source, string type)
		{
			Id = id;
			ConceptId = conceptId;
			Literal = literal;
			Language = language;
			ConceptLabel = conceptLabel;
			BroaderId = broaderId;
			BroaderLabel = broaderLabel;
			Source = source;
			Type = type;
			SkosSourceKey = skosSourceKey;
		}

		public bool Equals(ConceptTerm other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.Id == Id;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (ConceptTerm)) return false;
			return Equals((ConceptTerm) obj);
		}
		
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}
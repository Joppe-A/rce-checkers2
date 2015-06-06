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

namespace Trezorix.Checkers.DocumentChecker.SkosSources
{
	[DataContract]
	public class SkosSource 
	{
		public const string solr_feed_artifact_key = "solr_feed";
		
		public SkosSource()
		{
			TermEnricherSettings = new TermEnricherSettings();
		}
		
		[DataMember]
		public string Key { get; set; }

		[DataMember]
		public string Label { get; set; }

		[DataMember]
		public SkosSourceState Status { get; set; }

		[DataMember]
		public string LastError { get; set; }

		[DataMember]
		public TermEnricherSettings TermEnricherSettings { get; set; }

		[DataMember]
		public string SourceUri { get; set; }
	}

	public enum SkosSourceState
	{
		Storing,
		Stored,
		SkosProcessing,
		SkosProcessed,
		Indexing,
		Indexed,
		RetrievalError
	}
}
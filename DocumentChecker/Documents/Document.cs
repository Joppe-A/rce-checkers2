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
using System.Runtime.Serialization;

namespace Trezorix.Checkers.DocumentChecker.Documents
{
	[DataContract]
	public class Document
	{
		public Document(string sourceUri)
		{
			if (string.IsNullOrEmpty(sourceUri)) throw new ArgumentNullException("sourceUri");
		}

		[DataMember]
		public string SourceUri { get; set; }

		[DataMember]
		public DocumentState Status { get; set; }

		[DataMember]
		public string LastError { get; set; }

		[DataMember]
		public string AppliedProfileKey { get; set; }

		[DataMember]
		public string JobLabel { get; set; }

		// FI: These are stored in artifact files, let the document repository set these properties in a lazy way?
		[IgnoreDataMember]
		public ReviewResult ReviewResult { get; set; }

		[IgnoreDataMember]
		public AnalysisResult AnalysisResult { get; set; }

		[IgnoreDataMember]
		public string ConversionResult { get; set; }
		


	}
}

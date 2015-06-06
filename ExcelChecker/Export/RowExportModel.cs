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
using System.Linq;
using System.Runtime.Serialization;
using Trezorix.Checkers.DocumentChecker.Documents;

namespace Trezorix.Checkers.ExcelXmlChecker.Export
{
	[DataContract]
	public class RowExportModel
	{
		private ReviewResult _reviewResult = new ReviewResult();
		
		[DataMember]
		public string URIs
		{
			// ToDo: May as well use the private setter from the constructor
			get { return String.Join("|", _reviewResult.Select(rr => rr.Id)); }
			private set { }
		}

		[DataMember]
		public string SkosSourceKeys
		{
			get { return String.Join("|", _reviewResult.Select(rr => rr.SkosSourceKey)); }
			private set { }
		}

		[DataMember]
		public string Literals
		{
			get { return String.Join("|", _reviewResult.Select(rr => rr.Literal)); }
			private set { }
		}

		[DataMember]
		public string SourceUri { get; set; }

		[DataMember]
		public string CellText { get; set; }

		public void SetAnalysisResult(ReviewResult result)
		{
			_reviewResult = result;
		}
	}
}
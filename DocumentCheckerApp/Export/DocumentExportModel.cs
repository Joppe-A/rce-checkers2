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
using System.Runtime.Serialization;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.Checkers.DocumentCheckerApp.Controllers;

namespace Trezorix.Checkers.DocumentCheckerApp.Export
{
	[DataContract]
	public class DocumentExportModel
	{
		private readonly ReviewResult _reviewResult = new ReviewResult();
		
		[DataMember]
		public DateTime ModificationDate { get; set; }

		[DataMember]
		public DateTime CreationDate { get; set; }

		[DataMember]
		public string FileName { get; set; }

		[DataMember]
		public string JobLabel { get; set; }

		[DataMember]
		[IgnoreAsColumnAttribute]
		public IEnumerable<SkosSourceResult> SkosSourceResults
		{
			get 
			{
				return _reviewResult.Select(skosSource => new SkosSourceResult(skosSource.SkosSourceKey, _reviewResult.Where(r => r.SkosSourceKey == skosSource.SkosSourceKey))).ToList();
			}
		}

		[DataMember]
		public string SourceUri { get; set; }

		public DocumentExportModel(ReviewResult result)
		{
			_reviewResult = result;
		}
	}
}
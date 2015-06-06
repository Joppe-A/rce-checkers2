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
using System.Web.Script.Serialization;

using Trezorix.Checkers.DocumentCheckerApp.Helpers;
using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.DocumentCheckerApp.Models.Documents
{
	public class DocumentModel
	{
		[ScriptIgnore]
		private readonly Resource<DocumentChecker.Documents.Document> _document;

		public DocumentModel(Resource<DocumentChecker.Documents.Document> documentResource)
		{
			_document = documentResource;
		}

		public string Id
		{
			get { return Document.Id; }
		}

		public string SourceUri
		{
			get { return Document.Entity.SourceUri; }
		}

		public string Status
		{
			get { return (_document.Entity.Status).AsText(); }
		}

		public string JobLabel
		{
			get { return _document.Entity.JobLabel; }
		}

		[ScriptIgnore]
		public Resource<DocumentChecker.Documents.Document> Document
		{
			get { return _document; }
		}

		public string ModificationDate
		{
			get { return _document.ModificationDate.ToString(); }
		}
		
		public string CreationDate
		{
			get { return _document.CreationDate.ToString(); }
		}

		public string AppliedProfileName
		{
			get { return _document.Entity.AppliedProfileKey; }
		}
		
	}
}
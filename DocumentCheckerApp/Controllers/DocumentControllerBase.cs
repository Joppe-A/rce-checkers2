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
using System.Web;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.DocumentCheckerApp.Controllers
{
	public abstract class DocumentControllerBase : CheckersBaseController
	{
		private readonly IDocumentRepository _repository;
		
		protected DocumentControllerBase(IDocumentRepository resourceRepository)
		{
			_repository = resourceRepository;
		}

		protected IDocumentRepository Repository
		{
			get { return _repository; }
		}

		protected Resource<Document> FetchDocument(string id)
		{
			var document = _repository.Get(id);
			if (document == null) throw new HttpException(404, "No document found carrying Id " + id);

			return document;
		}

		protected static Artifact FetchArtifact(Resource<Document> document, string key)
		{
			var cd = document.Artifacts.SingleOrDefault(a => a.Key == key);
			if (cd == null)
			{
				throw new HttpException(404, String.Format("No artifact '{0}' found for document {1}", key, document.Id));
			}
			return cd;
		}

		protected static AnalysisResult LoadAnalysisResults(Artifact af)
		{
			var persister = new ArtifactPersister<AnalysisResult>();

			return persister.Revive(af.FilePathName);
		}

		protected static bool HasAnalysisCompleted(DocumentState documentState)
		{
			return documentState >= DocumentState.RenderingAnalysisResult;
		}

		protected static bool IsReadyForAnalysis(DocumentState status)
		{
			return status == DocumentState.Converted || HasAnalysisCompleted(status);
		}
	}
}
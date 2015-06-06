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
using System.Net;
using System.Web;
using System.Web.Mvc;
using Trezorix.Checkers.Analyzer.Indexes.Solr;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.Checkers.DocumentChecker.Jobs;
using Trezorix.Checkers.DocumentChecker.Processing;
using Trezorix.Checkers.DocumentCheckerApp.Models.Documents;
using Trezorix.Checkers.DocumentCheckerApp.Models.Shared;
using Trezorix.Checkers.FileConverter;
using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.DocumentCheckerApp.Controllers
{
	public class BasicDocumentController : DocumentController
	{
		public BasicDocumentController(IDocumentRepository resourceRepository, SolrIndex solrIndex, IJobRepository jobRepository, Func<IFileConverter> fileConverterCreator)
			: base(resourceRepository, solrIndex, jobRepository, fileConverterCreator)
		{
			
		}

		public BasicDocumentController() : base()
		{
			
		}

		public override ActionResult Index()
		{
			ViewData.Model = Repository.All();

			return View();
		}

		public override ActionResult Details(string id)
		{
			Resource<Document> doc = FetchDocument(id);
			ViewData.Model = new DocumentModel(doc);

			return View();
		}

		public ActionResult Upload()
		{
			return View();
		}

		public ActionResult GetConversion(string id)
		{
			Resource<Document> document = FetchDocument(id);

			if (!IsConversionComplete(document.Entity.Status))
			{
				throw new HttpException((int)HttpStatusCode.PreconditionFailed,
										"Document converter not done converting yet, please check status before attempting downloads of document conversions.");
			}

			var cd = FetchArtifact(document, DocumentConverter.CONVERSION_ARTIFACT_KEY);

			var contentType = cd.ContentType ?? "text/html";

			return File(cd.FilePathName, contentType);
		}
		
		private static bool IsConversionComplete(DocumentState status)
		{
			return status >= DocumentState.Converted;
		}

		public ActionResult Artifact(string id, string key)
		{
			Resource<Document> document = FetchDocument(id);

			var cd = FetchArtifact(document, key);

			var contentType = cd.ContentType ?? "application/text";

			return File(cd.FilePathName, contentType);
		}

		[HttpPost]
		public ActionResult ReAnalyse(string id, string jobLabel)
		{
			Resource<Document> document = FetchDocument(id);

			if (!HasAnalysisCompleted(document.Entity.Status) && document.Entity.Status != DocumentState.ProcessingFailed)
			{
				throw new HttpException((int)HttpStatusCode.PreconditionFailed,
										"Document processor is has not completed analysis, it may still be busy doing so. Can't re-analyze yet.");
			}

			var job = FetchJob(jobLabel);

			ReanalyseDocument(document, job);

			return RedirectToAction("Index");
		}


		[ActionName("Delete")]
		[HttpGet]
		public ActionResult DeleteConfirm(string id)
		{
			const string controllerName = "Document";
			var model = new DeleteConfirmViewModel()
			{
				Id = id,
				EntityName = "document",
				CancelLink = new ActionLinkModel()
				{
					Action = "Index",
					Controller = controllerName
				},
				DeletePostLink = new ActionLinkModel()
				{
					Action = "Delete",
					Controller = controllerName
				}
			};

			return View("DeleteConfirm", model);
		}
	}
}

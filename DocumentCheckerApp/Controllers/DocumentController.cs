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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Web.Mvc;
using Trezorix.Checkers.Analyzer.Indexes.Solr;
using Trezorix.Checkers.Analyzer.Matchers;
using Trezorix.Checkers.FileConverter;
using Trezorix.Common.Meta;
using Trezorix.Checkers.DocumentChecker;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.Checkers.DocumentChecker.Jobs;
using Trezorix.Checkers.DocumentChecker.Processing;
using Trezorix.Checkers.DocumentCheckerApp.Helpers;
using Trezorix.Checkers.DocumentCheckerApp.Models.Documents;
using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.DocumentCheckerApp.Controllers
{
	public class DocumentController : DocumentControllerBase
	{
		private readonly SolrIndex _solrIndex;

		private readonly IJobRepository _jobRepository;
		private Func<IFileConverter> _fileConverterCreator;

		public DocumentController() : this(new DocumentRepository(InstanceConfig.Current.DocumentRepositoryPath),
											new SolrIndex(InstanceConfig.Current.SolrIndexUrl),
											new JobRepository(InstanceConfig.Current.JobRepositoryPath),
											() => new TikaCommandLineWrapper(SystemConfig.JavaVMExecutableFilePathName)
										)
		{
			;
		}

		public DocumentController(IDocumentRepository resourceRepository, SolrIndex solrIndex, IJobRepository jobRepository, Func<IFileConverter> fileConverterCreator)
			: base(resourceRepository)
		{
			if (resourceRepository == null) throw new ArgumentNullException("resourceRepository");
			if (solrIndex == null) throw new ArgumentNullException("solrIndex");
			if (fileConverterCreator == null) throw new ArgumentNullException("fileConverterCreator");
			
			_fileConverterCreator = fileConverterCreator;
			_jobRepository = jobRepository;
			
			_solrIndex = solrIndex;
		}

		protected internal IJobRepository JobRepository
		{
			get { return _jobRepository; }
		}

		public ActionResult Home()
		{
			return View();
		}
		
		public virtual ActionResult Index()
		{
			return Json(Repository.All(), JsonRequestBehavior.AllowGet);
		}

		public ActionResult Get(string id)
		{
			Resource<Document> doc = FetchDocument(id);
			ViewData.Model = new DocumentModel(doc);

			return Json(new DocumentModel(doc), JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetByJobLabel(string jobLabel)
		{
			if (jobLabel == null) throw new HttpException("Null label");

			var documents = Repository.GetByJobLabel(jobLabel);
			//if (documents == null) throw new HttpException(404, "No document found carrying Id " + id);
			
			IEnumerable<DocumentModel> model = documents.Where(d => d.Entity.Status == DocumentState.Reviewed).Select(d => new DocumentModel(d));

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult Upload(HttpPostedFileBase file, string jobLabel)
		{
			if(file == null)
			{
				return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest, "File data missing.");
			}
			
			var job = FetchJob(jobLabel);
			if (job == null) return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest, "No job found with label: " + jobLabel);

			var fileName = Path.GetFileName(file.FileName);
			Resource<Document> document = Repository.Create(new Document(file.FileName), fileName);
			
			document.Entity.AppliedProfileKey = ActiveProfile.Instance().Key;
			document.Entity.JobLabel = job.Label;

			document.Entity.SourceUri = file.FileName;
			document.Entity.Status = DocumentState.Uploaded;

			Repository.Update(document);

			file.SaveAs(document.FilePathName());

			document.Entity.Status = DocumentState.Stored;
			Repository.Update(document);

			ProcessDocument(document, job);

			var cs = new ExtJsDocumentCreationResultJsonModel(document.Id);

			// ExtJS: Content type must be "text/html" because of fake 'ajax submit' by using an iFrame!
			return Json(cs, @"text/html"); 
		}

		[HttpPost]
		public ActionResult DownloadWebpage(string url, string jobLabel)
		{
			if (string.IsNullOrEmpty(url))
			{
				return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest, "url is missing.");
			}

			var job = FetchJob(jobLabel);
			if (job == null) return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest, "No job found with label: " + jobLabel);

			Uri uri;
			if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
			{
				return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest, "url is invalid.");
			}

			Resource<Document> document = Repository.Create(new Document(url), "webpage.html");

			document.Entity.AppliedProfileKey = ActiveProfile.Instance().Key;
			document.Entity.JobLabel = job.Label;

			document.Entity.SourceUri = url;
			document.Entity.Status = DocumentState.Downloading;

			Repository.Update(document);

			DownloadWebPage(document, uri);

			document.Entity.Status = DocumentState.Stored;
			Repository.Update(document);

			ProcessDocument(document, job);

			var cs = new ExtJsDocumentCreationResultJsonModel(document.Id);

			// ExtJS: Content type must be "text/html" because of fake 'ajax submit' by using an iFrame!
			return Json(cs, @"text/html");
		}

		private void DownloadWebPage(Resource<Document> document, Uri uri)
		{
			var request = WebRequest.Create(uri);

			request.Method = "GET";
			request.Timeout = 20000;
			var response = request.GetResponse();

			using (var responseStream = response.GetResponseStream())
			{
				string filePathName = document.FilePathName();
				StreamToFile(filePathName, responseStream);
			}
		}

		// ToDo: This method has a number of copies, make a util / service class for it?
		[TestingSeam]
		protected virtual void StreamToFile(string filePathName, Stream stream)
		{
			using (var file = new FileStream(filePathName, FileMode.CreateNew, FileAccess.Write))
			{
				stream.CopyTo(file);
			}
		}

		protected Job FetchJob(string label)
		{
			var job = JobRepository.GetByLabel(label).Entity;
			if (job == null) throw new HttpException(404, "No job found labeled: " + label);

			return job;
		}

		protected void ProcessDocument(Resource<Document> document, Job job)
		{
			DocumentProcessor processor = CreateProcessor(job);
			Task.Factory.StartNew(() => processor.Process(document));
		}

		protected void ReanalyseDocument(Resource<Document> document, Job job)
		{
			DocumentProcessor processor = CreateProcessor(job);
			Task.Factory.StartNew(() => processor.ReAnalyse(document));
		}
		
		private DocumentProcessor CreateProcessor(Job job)
		{
			var profile = ActiveProfile.Instance();

			IExpandingTokenMatcher matcher = new SolrExpandingTokenMatcher(_solrIndex, 
				new MatcherFilter()
					{
						Language = "dut", 
						SkosSources = job.SkosSourceSelection
					});

			return new DocumentProcessor(Repository, profile.StopWords, matcher, _fileConverterCreator());
		}

		public ActionResult Status(string id)
		{
			Resource<Document> doc = FetchDocument(id);

			return Json(new DocumentStatusJsonModel
								{
									status = doc.Entity.Status,
									statusText = doc.Entity.Status.AsText(),
									conversionError = doc.Entity.LastError
								}, JsonRequestBehavior.AllowGet);
		}

		public virtual ActionResult Details(string id)
		{
			Resource<Document> doc = FetchDocument(id);

			return Json(doc, JsonRequestBehavior.AllowGet);
		}
		
		[HttpPost]
		public ActionResult Analyse(string id, string jobLabel)
		{
			Resource<Document> document = FetchDocument(id);

			if (!IsReadyForAnalysis(document.Entity.Status))
			{
				throw new HttpException((int)HttpStatusCode.PreconditionFailed,
										"Document is currently being analyzed or conversion failed. Check document status.");
			}

			var job = FetchJob(jobLabel);

			CreateProcessor(job).ReAnalyse(document);

			return RedirectToAction("Index");
		}

		public ActionResult AnalysisResult(string id)
		{
			Resource<Document> document = FetchDocument(id);

			if (!HasAnalysisCompleted(document.Entity.Status)) 
			{
				return new HttpStatusCodeResult((int)HttpStatusCode.PreconditionFailed,
										"Document has not been analyzed, please check status before attempting result download.");
			}

			var cd = FetchArtifact(document, DocumentAnalyzer.ANALYSER_HITS_ARTIFACT_KEY);

			AnalysisResult hits = LoadAnalysisResults(cd);
			return Json(hits, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult Delete(string id)
		{
			Repository.Delete(id);

			return ExtJsSuccess();
		}

		[HttpPost]
		public ActionResult DeleteSet(IEnumerable<string> documents)
		{
			foreach (var docId in documents)
			{
				Repository.Delete(docId);
			}

			return ExtJsSuccess();
		}
	}
}

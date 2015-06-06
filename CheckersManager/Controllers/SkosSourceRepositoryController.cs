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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Trezorix.Checkers.Analyzer.Indexes;
using Trezorix.Checkers.Analyzer.Indexes.Solr;
using Trezorix.Checkers.DocumentChecker.SkosSources;

using Trezorix.Checkers.ManagerApp.Helpers;
using Trezorix.Checkers.ManagerApp.Models.Shared;
using Trezorix.Checkers.ManagerApp.Models.SkosSourceRepository;
using Trezorix.Checkers.ManagerApp.Services;

using Trezorix.Common.Meta;
using Trezorix.ResourceRepository;
using Trezorix.Treazure.API;

namespace Trezorix.Checkers.ManagerApp.Controllers
{
	public class SkosSourceRepositoryController : Controller
	{
		private readonly ISkosSourceRepository _repository;
		private readonly SolrIndex _solrIndex;
		private readonly SkosProcessor _skosProcessor;

		public SkosSourceRepositoryController() 
			: this(new SkosSourceRepository(ManagerAppConfig.SkosSourceRepositoryPath),
					new SolrIndex(ManagerAppConfig.SolrIndexUrl),
					new SkosProcessor())
		{
			
		}

		public SkosSourceRepositoryController(ISkosSourceRepository skosRepository, SolrIndex solrIndex, SkosProcessor skosProcessor)
		{
			_repository = skosRepository;
			_skosProcessor = skosProcessor;
			_solrIndex = solrIndex;
		}

		[TestingSeam]
		public virtual IEnumerable<string> DictionaryCollections
		{
			get
			{
				if (string.IsNullOrWhiteSpace(ManagerAppConfig.DictionaryConnectionString))
				{
					return Enumerable.Empty<string>();
				}
				
				return new Dictionaries(ManagerAppConfig.DictionaryConnectionString)
					.GetAllDictionaryCollections()
					.Select(d => d.Name)
					.ToList();
			}
		}

		[TestingSeam]
		public virtual ITermEnricher CreateTermEnricher
		{
			get
			{
				if (string.IsNullOrWhiteSpace(ManagerAppConfig.DictionaryConnectionString))
				{
					return new NullEnricher();
				}
				return new SearchWords(ManagerAppConfig.DictionaryConnectionString);
			}
		}

		[HttpPost]
		public ActionResult RecreateSolrFeed(string id)
		{
			var skos = _repository.Get(id);
			if (skos == null)
			{
				return HttpNotFound("No Skos source with Key: " + id);
			}

			if (skos.Entity.Status < SkosSourceState.Stored)
			{
				return HttpNotFound("Skos source is missing a skos data file.");
			}

			_skosProcessor.Process(skos, _repository);

			_solrIndex.Commit();

			TempData["OperationResult"] = "Solr feed recreated succesfully";
			return RedirectToAction("Index");

		}

		[HttpPost]
		public ActionResult SendToSolrIndex(string id)
		{
			var skos = _repository.Get(id);
			if (skos == null)
			{
				return HttpNotFound("No Skos source with Key: " + id);
			}

			var solrFeedArtifact = skos.Artifacts.SingleOrDefault(a => a.Key == SkosSource.solr_feed_artifact_key);
			if (solrFeedArtifact == null)
			{
				return HttpNotFound("No solr update feed artifact found for skos source: " + id);
			}

			SendSolrFeed(solrFeedArtifact.FilePathName);
			_solrIndex.Commit();

			TempData["OperationResult"] = "Skos file send to Solr succesfully";
			return RedirectToAction("Index");
		}

		private void SendSolrFeed(string filePathName)
		{
			XDocument doc;
			using (var skosFileStream = new FileStream(filePathName, FileMode.Open))
			{
				doc = XDocument.Load(skosFileStream);
			}
			_solrIndex.Update(doc);
		}

		[HttpPost]
		public ActionResult SendAllToSolrIndex()
		{
			Task.Factory.StartNew(() =>
					{
						Debug.WriteLine("Clearing SOLR");
						_solrIndex.Clear();

						foreach (var solrFeedArtifact in from skos in _repository.All()
															where skos.Entity.Status == SkosSourceState.SkosProcessed
															select skos.Artifacts.Single(a => a.Key == SkosSource.solr_feed_artifact_key))
						{
							var filePathName = solrFeedArtifact.FilePathName;
							SendSolrFeed(filePathName);
						}
						Debug.WriteLine("All feeds fed to SOLR.");
						_solrIndex.Commit();
					});
			
			TempData["OperationResult"] = "Sending skos repository content to Solr has started.";
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult RecreateAllSolrFeeds()
		{
			Task.Factory.StartNew(() =>
					{
						foreach (var skosResource in from skos in _repository.All()
			                      						where skos.Entity.Status > SkosSourceState.Stored
			                      						select skos)
						{
							Debug.WriteLine("Creating solr feed for " + skosResource.Entity.Label);
							_skosProcessor.Process(skosResource, _repository);
							Debug.WriteLine("Feed completed.");
						}
						Debug.WriteLine("All feeds created");
					});

			TempData["OperationResult"] = "Solr feed recreation has started.";
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult ClearSolrIndex()
		{
			_solrIndex.Clear();
			_solrIndex.Commit();

			TempData["OperationResult"] = "Clear command send to Solr.";
			return RedirectToAction("Index");
		}

		public ActionResult Skos(string id)
		{
			var skos = _repository.Get(id);
			if (skos == null)
			{
				return HttpNotFound("No Skos source with Key: " + id);
			}
			
			return new FilePathResult(skos.FilePathName(), @"application/xml");
		}

		public ActionResult SolrFeed(string id)
		{
			var skos = _repository.Get(id);
			if (skos == null)
			{
				return HttpNotFound("No Skos source with Key: " + id);
			}

			var solrFeedArtifact = skos.Artifacts.SingleOrDefault(a => a.Key == SkosSource.solr_feed_artifact_key);
			if (solrFeedArtifact == null)
			{
				return HttpNotFound("No solr update feed artifact found for skos source: " + id);
			}

			return File(solrFeedArtifact.FilePathName, @"application/xml");
		}

		public ActionResult Index()
		{
			SkosSourceIndexModel indexModel = CreateIndexModel();

			ViewData.Model = indexModel;
			
			return View();
		}

		private SkosSourceIndexModel CreateIndexModel()
		{
			var sources = _repository.All();
			return new SkosSourceIndexModel(sources, DictionaryCollections);
		}

		[HttpGet]
		public ActionResult Download()
		{
			string newId = Guid.NewGuid().ToString();
			ViewData.Model = new SkosSourceEditModel(DictionaryCollections.GetSelectListItemsForDictionaryCollectionList())
			                 {
			                 	Key = newId
			                 };

			return View("Download");
		}

		[HttpGet]
		public ActionResult Upload()
		{
			string newId = Guid.NewGuid().ToString();
			ViewData.Model = new SkosSourceEditModel(DictionaryCollections.GetSelectListItemsForDictionaryCollectionList())
			                 {
			                 	Key = newId
			                 };

			return View("Upload");
		}

		// ToDo: remove duplication between this method and the download version
		[HttpPost]
		public ActionResult Upload(SkosSourceEditModel skosSourceModel, HttpPostedFileBase indexFile)
		{
			ModelCheckSkosSourceId(skosSourceModel.Key);

			if (!ModelState.IsValid)
			{
				skosSourceModel.DictionaryCollections = DictionaryCollections.GetSelectListItemsForDictionaryCollectionList();
				return View("Upload", skosSourceModel);
			}
			
			if (indexFile == null)
			{
				return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest, "File data missing.");
			}
			
			var fileName = Path.GetFileName(indexFile.FileName);

			skosSourceModel.SourceUri = indexFile.FileName;

			var newSkosSourceEntity = new SkosSource();

			skosSourceModel.Update(newSkosSourceEntity);
			
			var skosSource = _repository.Create(newSkosSourceEntity, fileName);

			skosSource.Entity.Status = SkosSourceState.Storing;
			
			_repository.Update(skosSource);

			indexFile.SaveAs(skosSource.FilePathName());

			_skosProcessor.Process(skosSource, _repository);
			
			TempData["OperationResult"] = "File uploaded and stored successfully";
			return RedirectToAction("Index");
		}

		private void ModelCheckSkosSourceId(string key)
		{
			if (String.IsNullOrEmpty(key))
			{
				ViewData.ModelState.AddModelError("", "Please set a key for this Skos source.");
			}

			
			if (_repository.HasKey(key))
			{
				ViewData.ModelState.AddModelError("", "This skos source key already exists (" + key + "), pick a unique key.");
			}
		}

		[HttpPost]
		public ActionResult Download(SkosSourceEditModel skosSourceModel)
		{
			ModelCheckSkosSourceId(skosSourceModel.Key);

			if (!ModelState.IsValid)
			{
				return DownloadView(skosSourceModel);
			}

			var newSkosSourceEntity = new SkosSource();

			skosSourceModel.Update(newSkosSourceEntity);

			var document = _repository.Create(newSkosSourceEntity, "downloaded_skos.xml");

			document.Entity.Status = SkosSourceState.Storing;
			_repository.Update(document);

			try
			{
				DownloadSkosSource(document, skosSourceModel.SourceUri);
			}
			catch (Exception ex)
			{
				document.Entity.Status = SkosSourceState.RetrievalError;
				document.Entity.LastError = "Downloading of the skos source failed." + ex.Message;
				_repository.Update(document);

				TempData["OperationResult"] = document.Entity.LastError;

				return RedirectToAction("Index");
			}
			
			_skosProcessor.Process(document, _repository);

			TempData["OperationResult"] = "File downloaded and stored successfully under id: " + document.Id;
			
			return RedirectToAction("Index");
		}

		private ViewResult DownloadView(SkosSourceEditModel skosSourceModel)
		{
			skosSourceModel.DictionaryCollections = DictionaryCollections.GetSelectListItemsForDictionaryCollectionList();
			return View("Download", skosSourceModel);
		}

		[HttpPost]
		public ActionResult Edit(SkosSourceEditModel model)
		{
			if (String.IsNullOrEmpty(model.Key))
			{
				ViewData.ModelState.AddModelError("", "Please set a key for this Skos source.");
			}
			else
			{
				if (!HasUniqueKey(model))
				{
					ViewData.ModelState.AddModelError("", "This skos source key already exists (" + model.Key + "), pick a unique key.");
				}
			}

			if (!ModelState.IsValid)
			{
				SkosSourceIndexModel indexModel = CreateIndexModel();
				return View("Index", indexModel);
			}

			var skosSource = _repository.Get(model.Id);

			if (skosSource == null) throw new HttpException(404, "No Skos source found with id: " + model.Id);

			model.Update(skosSource.Entity);

			_repository.Update(skosSource);

			return RedirectToAction("Index");
		}

		private bool HasUniqueKey(SkosSourceEditModel model)
		{
			if (!_repository.HasKey(model.Key)) return true;

			var docHoldingSameKey = _repository.GetByKey(model.Key);
			return !(docHoldingSameKey != null && docHoldingSameKey.Id != model.Id);
		}

		[TestingSeam]
		protected virtual void DownloadSkosSource(Resource<SkosSource> skosSource, string downloadUri)
		{
			var request = WebRequest.Create(downloadUri);
			request.Method = "GET";

			var response = request.GetResponse();
			using (var responseStream = response.GetResponseStream())
			using (var file = new FileStream(skosSource.FilePathName(), FileMode.CreateNew, FileAccess.Write))
			{
				responseStream.CopyTo(file);
			}
		}

		[ActionName("Delete")]
		[HttpGet]
		public ActionResult DeleteConfirm(string id)
		{
			const string controllerName = "SkosSourceRepository";
			var model = new DeleteConfirmViewModel()
			{
				Id = id,
				EntityName = "skos source",
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

		[HttpPost]
		public ActionResult Delete(string id)
		{
			_repository.Delete(id);
			
			TempData["OperationResult"] = string.Format("Skos source document {0} deleted successfully.", id);
			return RedirectToAction("Index");
		}

		[TestingSeam]
		protected virtual void StreamToFile(string filePathName, Stream stream)
		{
			using (var file = new FileStream(filePathName, FileMode.CreateNew, FileAccess.Write))
			{
				stream.CopyTo(file);
			}
		}
		
	}
}

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
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Trezorix.Common.Meta;
using Trezorix.Checkers.DocumentChecker.SkosSources;
using Trezorix.Checkers.ManagerApp.Models.RnaToolsetImport;
using Trezorix.Checkers.ManagerApp.Services;
using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.ManagerApp.Controllers
{
	public class RnaToolsetImportController : Controller
	{
		private readonly ISkosSourceRepository _repository;
		private readonly SkosProcessor _skosProcessor;

		public RnaToolsetImportController() 
			: this(new SkosSourceRepository(ManagerAppConfig.SkosSourceRepositoryPath),
			new SkosProcessor()
			)
		{
		}

		public RnaToolsetImportController(ISkosSourceRepository skosRepository, SkosProcessor skosProcessor)
		{
			_repository = skosRepository;
			_skosProcessor = skosProcessor;
		}
		
		public ActionResult StructureImportSelect(string toolsetUrl, string apiKey)
		{
			var importModel = new RnaToolsetModel()
								{
									ToolsetUrl = toolsetUrl,
									ApiKey = apiKey,
									Structures = GetStructureList(toolsetUrl, apiKey).ToList()
								};
			
			return View("StructureSelect", importModel);
		}

		[HttpPost]
		public ActionResult ImportAllStructures(string toolsetUrl, string apiKey)
		{
			IEnumerable<RnaStructureModel> structures = GetStructureList(toolsetUrl, apiKey);

			ImportStructures(toolsetUrl, apiKey, structures);

			return Json("Ok");
		}

		[HttpPost]
		public ActionResult ImportStructureSelection(string toolsetUrl, string apiKey, List<RnaStructureSelectionModel> structures)
		{
			IEnumerable<RnaStructureModel> allStructures = GetStructureList(toolsetUrl, apiKey);

			structures = structures.Where(ss => ss.Selected).ToList();

			var importStructures = allStructures.Where(s => structures.Any(ss => s.Uri == ss.Uri));
			
			ImportStructures(toolsetUrl, apiKey, importStructures);

			return Json("Ok");
		}

		private void ImportStructures(string toolsetUrl, string apiKey, IEnumerable<RnaStructureModel> importStructures)
		{
			foreach (var structure in importStructures)
			{
				var referenceStructureUri = structure.Uri;
				var requestUriString = MakeReferenceStructureRequestUrl(toolsetUrl, apiKey, referenceStructureUri);

				var key = structure.CreateKey(toolsetUrl, (checkKey) => !_repository.HasKey(checkKey));
				
				var newSkosSourceEntity = new SkosSource
				                          	{
				                          		Key = key, 
												Label = structure.Label,
												SourceUri = requestUriString
				                          	};
				
				var document = _repository.Create(newSkosSourceEntity, "downloaded_skos.xml");

				document.Entity.Status = SkosSourceState.Storing;
				_repository.Update(document);

				try
				{
					WebRequestToFile(document, requestUriString);
					document.Entity.Status = SkosSourceState.Stored;

				}
				catch (Exception ex)
				{
					document.Entity.Status = SkosSourceState.RetrievalError;
					document.Entity.LastError = "Error importing structure: " + ex.Message;
					continue;
				}
				finally
				{
					_repository.Update(document);
				}

				_skosProcessor.Process(document, _repository);
			}
		}

		private static string MakeReferenceStructureRequestUrl(string toolsetUrl, string apiKey, string referenceStructureUri)
		{
			return string.Format("{0}/api/GetSkosList.aspx?rna_api_key={1}&uri={2}", toolsetUrl, apiKey, referenceStructureUri);
		}

		private IEnumerable<RnaStructureModel> GetStructureList(string toolsetUrl, string apiKey)
		{
			XDocument structureList;

			// apikey: e22085bb3e1b4b5f8b49e6ddf015a1cc
			var request =
				WebRequest.Create(string.Format("{0}/api/GetReferenceStructureList.aspx?rna_api_key={1}", toolsetUrl, apiKey));
			request.Method = "GET";

			var response = request.GetResponse();
			using (var responseStream = response.GetResponseStream())
			{
				structureList = XDocument.Load(responseStream);
			}

			return from s in structureList.Root.Elements("referenceStructure")
				   select new RnaStructureModel
							{
								Uri = (string)s.Attribute("rootItemUri"),
								Label = (string)s.Attribute("label")
							};
		}

		private void WebRequestToFile(Resource<SkosSource> skosSource, string requestUriString)
		{
			var request =
				WebRequest.Create(requestUriString);

			request.Method = "GET";
			request.Timeout = 1200000;
			var response = request.GetResponse();

			using (var responseStream = response.GetResponseStream())
			{
				string filePathName = skosSource.FilePathName();
				StreamToFile(filePathName, responseStream);
			}
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

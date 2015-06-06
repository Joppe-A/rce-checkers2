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
using System.Web.Mvc;
using Trezorix.Checkers.ManagerApp.Models.Enrichment;
using Trezorix.Treazure.API;

namespace Trezorix.Checkers.ManagerApp.Controllers
{
	public class EnrichmentController : Controller
	{
		//
		// GET: /Enrichment/

		public ActionResult Index(string id)
		{
			var termEnrichment = new Dictionaries(ManagerAppConfig.DictionaryConnectionString);
			IEnumerable<DictionaryCollectionModel> availableDictionaryCollections = DictionaryCollectionModel.Get(termEnrichment);
			ViewData.Model = availableDictionaryCollections;
			return View();
		}

		[HttpGet()]
		public ActionResult AddWord(string id)
		{
			int selectedDictionaryCollection = -1;
			var termEnrichment = new Dictionaries(ManagerAppConfig.DictionaryConnectionString);
			if (int.TryParse(id, out selectedDictionaryCollection))
			{
				// Get all available dictionaries

				ViewData.Model = DictionaryModel.Get(termEnrichment, selectedDictionaryCollection);
			}
			return View();
		}

		[HttpPost]
		public ActionResult Create(string dictionary, string term)
		{
			int iDictionary = -1;
			string result = null;
			if ((int.TryParse(dictionary, out iDictionary)) && (!string.IsNullOrWhiteSpace(term)))
			{
				// Add the given term to the Trezorix dictionary
				var termEnrichment = new Dictionaries(ManagerAppConfig.DictionaryConnectionString);

				try
				{
					termEnrichment.AddWord(term, iDictionary);
					result = "Term " + term + " created";
				
				}
				catch (Exception)
				{
					// ToDo: Joppe: Should handle this properly, a user can't fix system errors
					result = "Failed to create term " + term ;
				}
				
			}

			ViewData.Model = result;
			return View();
		}

	}
}

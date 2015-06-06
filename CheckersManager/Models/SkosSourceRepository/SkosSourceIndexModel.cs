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
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Trezorix.Checkers.DocumentChecker.SkosSources;
using Trezorix.ResourceRepository;
using Trezorix.Checkers.ManagerApp.Helpers;

namespace Trezorix.Checkers.ManagerApp.Models.SkosSourceRepository
{
	public class SkosSourceIndexModel
	{
		public IEnumerable<SelectListItem> DictionaryCollections { get; private set; }
		public IEnumerable<SkosSourceEditModel> SkosSources { get; private set; }

		public SkosSourceIndexModel(IEnumerable<Resource<SkosSource>> skosSources, IEnumerable<string> dictionaryCollections)
		{
			DictionaryCollections = dictionaryCollections.GetSelectListItemsForDictionaryCollectionList();

			SkosSources = skosSources.Select(ss => new SkosSourceEditModel(DictionaryCollections)
			                                       {
			                                       	TermEnricherSettings = ss.Entity.TermEnricherSettings,
													Id = ss.Id,
													Status = ss.Entity.Status.AsText(),
													LastError = ss.Entity.LastError,
													Key = ss.Entity.Key,
													Label = ss.Entity.Label,
													SourceUri = ss.Entity.SourceUri == null ? null : ss.Entity.SourceUri.ToString(), 
													ModificationDate = ss.ModificationDate
													
			                                       });
		}
	}
}
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

using Trezorix.Checkers.DocumentChecker.SkosSources;

namespace Trezorix.Checkers.ManagerApp.Models.SkosSourceRepository
{
	public class SkosSourceEditModel
	{
		public SkosSourceEditModel()
		{
			TermEnricherSettings = new TermEnricherSettings();
		}

		public SkosSourceEditModel(IEnumerable<SelectListItem> dictionaryCollections) : this()
		{
			DictionaryCollections = dictionaryCollections;
		}

		public IEnumerable<SelectListItem> DictionaryCollections { get; set; }

		public TermEnricherSettings TermEnricherSettings { get; set; }
		
		public string Key { get; set; }
		public string Label { get; set; }

		public string Id { get; set; }
		public string Status { get; set; }
		public string LastError { get; set; }

		public string SourceUri { get; set; }

		public DateTime ModificationDate { get; set; }

		public void Update(SkosSource skosSource)
		{
			skosSource.TermEnricherSettings = TermEnricherSettings;
			skosSource.Key = Key;
			skosSource.Label = Label;

		}
		
	}
}
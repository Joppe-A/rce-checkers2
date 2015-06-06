﻿// Copyright 2013 Cultural Heritage Agency of the Netherlands, Dutch National Military Museum and Trezorix bv
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

namespace Trezorix.Checkers.ManagerApp.Helpers
{
	public static class DictionaryCollectionSelectListHelper
	{
		public static IList<SelectListItem> GetSelectListItemsForDictionaryCollectionList(this IEnumerable<string> dictionaryCollections)
		{
			IList<SelectListItem> selectListItems = dictionaryCollections.Select(d => new SelectListItem() { Text = d, Value = d }).ToList();
			// insert a none item in list
			selectListItems.Insert(0, new SelectListItem() { Text = "[ Geen ]", Value = null });
			return selectListItems;
		}	
	}
}
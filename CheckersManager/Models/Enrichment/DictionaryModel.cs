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

using Trezorix.Treazure.API;

namespace Trezorix.Checkers.ManagerApp.Models.Enrichment
{
	public class DictionaryModel
	{

		private int m_iId = -1;
		private string  m_sName = "";

		public int id { get { return m_iId; } }
		public string Name { get { return m_sName; } }

		public static IEnumerable<DictionaryModel> Get(Dictionaries dictionaries, int dictionaryCollectionId)
		{
			List<DictionaryModel> lstDictionaryModels = new List<DictionaryModel>();
			IEnumerable<Dictionary> dicts = dictionaries.GetAllDictionaries(dictionaryCollectionId);

			foreach (Dictionary dict in dicts)
			{
				DictionaryModel model = new DictionaryModel()
				{
					m_iId = dict.Id,
					m_sName = dict.Name
				};
				lstDictionaryModels.Add(model);
			}
			return lstDictionaryModels;
		}

	}
}
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
using Trezorix.Checkers.DocumentChecker.Documents;

namespace Trezorix.Checkers.DocumentCheckerApp
{
	public static class DocumentManager
	{
		public static void CleanUpAndRetain(IDocumentRepository documentRepository)
		{
			if (InstanceConfig.Current.DeleteResultlessDocumentsAtStartup)
			{
				// reviewless and older then a day
				documentRepository.DeleteReviewless();
			} 
			
			if (InstanceConfig.Current.DaysStoredBeforeDocumentRetention > 0)
			{
				foreach (var doc in documentRepository.All().Where(d => DateTime.Now.Subtract(d.ModificationDate).Days > InstanceConfig.Current.DaysStoredBeforeDocumentRetention))
				{
					documentRepository.Delete(doc.Id);
				}	
			}
			
		}
	}
}
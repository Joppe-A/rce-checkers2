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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.DocumentChecker.Documents
{
	public class DocumentRepository : ResourceRepository<Document>, IDocumentRepository
	{
		public DocumentRepository(string documentRepositoryPath) : base(documentRepositoryPath)
		{
		}

		public IEnumerable<Resource<Document>> GetByJobLabel(string jobLabel)
		{
			return All()
				.Where(d => d.Entity.JobLabel == jobLabel);
		}

		public IEnumerable<string> GetUniqueJobLabels()
		{
			return All().Select(d => d.Entity.JobLabel).Distinct();
		}

		public IEnumerable<Resource<Document>> Reviewless()
		{
			return All().Where(d => d.Entity.Status != DocumentState.Reviewed);
		}

		public void DeleteReviewless()
		{
			foreach (var doc in Reviewless())
			{
				Delete(doc.Id);
			}
		}
	}
}
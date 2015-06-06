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
using Trezorix.Common.Core;

namespace Trezorix.ResourceRepository
{
	public interface IResourceRepository<TEntity> : IRepository<Resource<TEntity>, string>
	{
		// ToDo: Make seperate builder somehow (we put it here cause we wanted all the repo paths to be created before a document was persisted)
		
		Resource<TEntity> Create(TEntity entity, string fileName);
		Resource<TEntity> Create(TEntity entity); // ToDo: trying to bend abstraction to something it wasn't made for? (Entity stores all it's data in the document setting file)

		string RepositoryPath { get; }
		
		void Update(Resource<TEntity> entity);
	}
}
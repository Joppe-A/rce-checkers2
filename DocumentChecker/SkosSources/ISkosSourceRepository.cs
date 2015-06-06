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

using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.DocumentChecker.SkosSources
{
	public interface ISkosSourceRepository : IResourceRepository<SkosSource>
	{
		IEnumerable<Resource<SkosSource>> GetIndexed();
		Resource<SkosSource> GetByKey(string key);
		bool HasKey(string key);
	}
}

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
using NUnit.Framework;
using Trezorix.ResourceRepository;

namespace ResourceRepositoryTests
{
	[TestFixture]
	public class ArtifactTests
	{
		[Test]
		public void FilePathName_should_return_combined_path()
		{
			// arrange
			var conversion = new Artifact("somelabel", @"C:\testout\docX\conv")
			                 	{
			                 		FileName = "test.out"
			                 	};

			// act
			var result = conversion.FilePathName;

			// assert
			Assert.AreEqual(@"C:\testout\docX\conv\test.out", result);
		}
	}
}

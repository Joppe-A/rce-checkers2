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
using System.IO;

using NUnit.Framework;

using System.Linq;
using Trezorix.ResourceRepository;
using Trezorix.Testing.Common.System;

namespace ResourceRepositoryTests
{
	[TestFixture]
	[Category("Integration")]
	public class ResourceRepositoryTests
	{
		private ResourceRepository<DummyDocument> _resourceRepository;

		[SetUp]
		public void Setup()
		{
			string repoPath = Path.Combine(FileTestHelpers.GetTestDataDir(), "Repo");
			
			Directory.CreateDirectory(repoPath);
			
			_resourceRepository = new ResourceRepository<DummyDocument>(repoPath);
		}

		[TearDown]
		public void Teardown()
		{
			Directory.Delete(_resourceRepository.RepositoryPath, true);
		}

		[Test]
		public void Create_should_return_a_Document_with_ID()
		{
			// arrange

			// act
			var result = _resourceRepository.Create(null, "somefile");

			// assert
			Assert.IsNotNullOrEmpty(result.Id);
		}

		[Test]
		[TestCase(null)]
		[TestCase("")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Create_should_throw_on_empty_filename(string fileName)
		{
			// arrange

			// act
			var result = _resourceRepository.Create(null, fileName);

			// assert
		}

		[Test]
		public void Create_should_remove_path_from_supplied_filename()
		{
			// arrange

			// act
			var result = _resourceRepository.Create(null, @"C:\Path\the_filename");

			// assert
			Assert.AreEqual("the_filename", result.FileName);
		}

		[Test]
		public void Create_should_associate_entity_with_document()
		{
			// arrange

			// act
			var myEntity = new DummyDocument();
			var result = _resourceRepository.Create(myEntity, @"C:\Path\the_filename");

			// assert
			Assert.IsNotNull(result.Entity);
		}

		[Test]
		public void Create_should_Save()
		{
			// arrange
			var doc = new SeamedResourceRepository(_resourceRepository);

			// act
			var result = doc.Create(null, "the_filename");

			// assert
			Assert.IsTrue(doc.Saved);
		}

		[Test]
		public void Delete_should_remove_document()
		{
			// arrange
			var result = _resourceRepository.Create(null, "the_filename");

			// act
			_resourceRepository.Delete(result.Id);

			// assert
			Assert.IsNull(_resourceRepository.Get(result.Id));
		}

		[Test]
		public void Delete_should_remove_physical_files()
		{
			// arrange
			var document = _resourceRepository.Create(null, "the_filename");

			FileTestHelpers.WriteTestFile(document.FilePathName());

			var cf = new Artifact("somelabel", document.ArtifactFolder);

			document.Artifacts.Add(cf);
			cf.FileName = document.FileName + ".xml";

			FileTestHelpers.WriteTestFile(cf.FilePathName);

			// act
			_resourceRepository.Delete(document.Id);

			// assert
			Assert.IsFalse(File.Exists(cf.FilePathName), "Converted XML file wasn't deleted");
			Assert.IsFalse(File.Exists(document.FilePathName()), "Source file wasn't deleted");
		}

		[Test]
		public void Update_should_persist_changes()
		{
			// arrange
			var myEntity = new DummyDocument();
			var resource = _resourceRepository.Create(myEntity, "the_filename");

			resource.Entity.SomeValue = "the changed value";
			
			// act
			_resourceRepository.Update(resource);

			// assert
			var result = _resourceRepository.Get(resource.Id);
			Assert.AreEqual("the changed value", result.Entity.SomeValue);
		}

		[Test]
		public void All_should_return_all_added_files()
		{
			Assert.AreEqual(0, _resourceRepository.All().Count());
			// arrange
			_resourceRepository.Create(null, "a_file");
			_resourceRepository.Create(null, "a_file");
			_resourceRepository.Create(null, "a_file");
			_resourceRepository.Create(null, "a_file");
			_resourceRepository.Create(null, "a_file");
			
			// act
			var result = _resourceRepository.All();

			// assert
			Assert.AreEqual(5, result.Count());
		}

		[Test]
		public void Get_on_non_existing_item_returns_null()
		{
			// arrange

			// act
			var result = _resourceRepository.Get("non-existing-Id");

			// assert
			Assert.IsNull(result);
		}

		private class SeamedResourceRepository : ResourceRepository<DummyDocument>
		{
			public bool Saved = false;

			public SeamedResourceRepository(ResourceRepository<DummyDocument> resourceRepository) 
				: base(resourceRepository.RepositoryPath)
			{
			}

			public override void Add(Resource<DummyDocument> entity)
			{
				Saved = true;
			}
			
			public override void Update(Resource<DummyDocument> entity)
			{
				
			}
		}
	}
}

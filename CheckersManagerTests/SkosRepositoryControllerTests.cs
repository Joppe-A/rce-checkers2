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
using System.Web;

using Moq;

using MvcContrib.TestHelper;

using NUnit.Framework;
using Trezorix.Checkers.Analyzer.Indexes;
using Trezorix.Checkers.Analyzer.Indexes.Solr;
using Trezorix.Checkers.DocumentChecker.SkosSources;
using Trezorix.Checkers.ManagerApp.Services;
using Trezorix.ResourceRepository;

using Trezorix.Checkers.ManagerApp.Controllers;
using Trezorix.Checkers.ManagerApp.Models.SkosSourceRepository;
using Trezorix.Testing.Common;

namespace DocumentCheckerAppTests
{
	[TestFixture]
	public class SkosRepositoryControllerTests
	{
		private readonly TermEnricherSettings _termEnricherSettings;
		
		private string _theId;
		private string _conflictingKey;
		private Resource<SkosSource> _existingDoc;

		private SeamedSkosSourceRepositoryController _skosSourceRepositoryController;
		private Mock<ISkosSourceRepository> _fakeRepository;
		
		public SkosRepositoryControllerTests()
		{
			_termEnricherSettings = new TermEnricherSettings()
			                        {
										DictionaryCollectionName = "dictionarycollection1",
			                        	Enabled = true
			                        };
		}

		[SetUp]
		public void SetupSeamedController()
		{
			_fakeRepository = new Mock<ISkosSourceRepository>();
			
			var fakeSolrIndex = new SolrIndex("someurl");
			var fakeProcessor = new Mock<SkosProcessor>();
			fakeProcessor.Setup(p => p.Process(It.IsAny<Resource<SkosSource>>(), It.IsAny<ISkosSourceRepository>()));
			_skosSourceRepositoryController = new SeamedSkosSourceRepositoryController(_fakeRepository.Object, fakeSolrIndex, fakeProcessor.Object);
		}

		[Test]
		public void Download_POST_without_key_should_give_modelerror()
		{
			// arrange
			
			// act
			var model = new SkosSourceEditModel();
			model.Key = null;
			
			var result = _skosSourceRepositoryController.Download(model);

			// assert
			result.AssertViewRendered().ForView("Download");
			Assert.IsFalse(_skosSourceRepositoryController.ModelState.IsValid, "Modelstate should be invalid");
		}

		[Test]
		public void Download_POST_with_a_unique_key_should_pass()
		{
			// arrange
			SetupRepositoryAsEmpty();
			SetupRepositoryToCreateDoc();

			// act
			var model = new SkosSourceEditModel();
			
			model.Key = "unique key";

			var result = _skosSourceRepositoryController.Download(model);

			// assert
			result.AssertActionRedirect().ToAction("Index");
			Assert.IsTrue(_skosSourceRepositoryController.ModelState.IsValid, "Modelstate should be valid");
			Assert.IsTrue(_skosSourceRepositoryController.WasDownloadCalled);
		}
		
		[Test]
		public void Download_POST_with_non_unique_key_should_give_modelerrors()
		{
			// arrange
			SetupRepositoryForKeyConflict();
			SetupRepositoryToCreateDoc();

			// act
			var model = new SkosSourceEditModel();

			model.Key = _conflictingKey;

			var result = _skosSourceRepositoryController.Download(model);

			// assert
			result.AssertViewRendered().ForView("Download");
			Assert.IsFalse(_skosSourceRepositoryController.ModelState.IsValid, "Modelstate should be invalid");
			Assert.IsTrue(_skosSourceRepositoryController.ModelState.Values.SelectMany(v => v.Errors).Any(e => e.ErrorMessage.Contains("unique")));
		}
	
		[Test]
		public void Edit_POST_redirects_to_index()
		{
			// arrange
			SetupRepositoryReturningExistingDocument();

			var model = new SkosSourceEditModel()
			            {
			            	Id = _theId,
							Key = "a key",
						};

			// act
			var result = _skosSourceRepositoryController.Edit(model);

			// assert
			result.AssertActionRedirect().ToAction("Index");
		}

		[Test]
		public void Edit_POST_should_update_TermEnricherSettings()
		{
			// arrange
			SetupRepositoryReturningExistingDocument();

			var newEnricherSettings = new TermEnricherSettings()
			{
				DictionaryCollectionName = "dictionarycollection1",
				Enabled = true
			};

			var model = new SkosSourceEditModel()
			{
				Id = _theId,
				Key = "my key",
				TermEnricherSettings = newEnricherSettings
			};

			// act
			var result = _skosSourceRepositoryController.Edit(model);

			// assert
			result.AssertActionRedirect().ToAction("Index");
			var settings = _existingDoc.Entity.TermEnricherSettings;
			Assert.AreEqual(newEnricherSettings.Enabled, settings.Enabled);
			Assert.AreEqual(newEnricherSettings.DictionaryCollectionName, settings.DictionaryCollectionName);
		}

		[Test]
		[ExpectedException(typeof(HttpException))]
		public void Edit_POST_with_non_existing_id_should_throw_HttpException()
		{
			// arrange
			SetupRepositoryAsEmpty();

			var model = new SkosSourceEditModel()
			            {
			            	Id = "Non existing",
							Key = "some key"
						};

			// act
			var result = _skosSourceRepositoryController.Edit(model);

			// assert
			result.AssertViewRendered().ForView("Index");
			Assert.IsFalse(_skosSourceRepositoryController.ModelState.IsValid, "Modelstate should be invalid");
		}

		private void SetupRepositoryAsEmpty()
		{
			_fakeRepository.Setup(r => r.Get(It.IsAny<string>())).Returns(() => null);
		}

		private void SetupRepositoryToCreateDoc()
		{
			var newDoc = new Resource<SkosSource>(new SkosSource() { Key = _theId }, "some_source") { FileName = "somefile.txt" };
			_fakeRepository.Setup(r => r.Create(It.IsAny<SkosSource>(), It.IsAny<string>())).Returns(() => newDoc);
		}

		[Test]
		public void Edit_POST_with_non_unique_key_should_return_modelerrors()
		{
			// arrange
			SetupRepositoryForKeyConflict();
			SetupRepositoryReturningExistingDocument();

			var model = new SkosSourceEditModel()
			            {
			            	Id = _theId,
			            	TermEnricherSettings = _termEnricherSettings,
			            	Key = _conflictingKey
						};

			// act
			var result = _skosSourceRepositoryController.Edit(model);

			// assert
			result.AssertViewRendered().ForView("Index");
			Assert.IsFalse(_skosSourceRepositoryController.ModelState.IsValid, "Modelstate should be invalid");
		}

		private void SetupRepositoryForKeyConflict()
		{
			_conflictingKey = "conflicting key";
			var conflictingDoc = new Resource<SkosSource>(new SkosSource() { Key = _conflictingKey }, "somepath" ) { Id = "some id" };
			_fakeRepository.Setup(c => c.HasKey(_conflictingKey)).Returns(true); 
			_fakeRepository.Setup(c => c.GetByKey(_conflictingKey)).Returns( conflictingDoc );
		}

		private void SetupRepositoryReturningExistingDocument()
		{
			_theId = "my id";
			_existingDoc = new Resource<SkosSource>(new SkosSource() { Key = _theId }, "some_source") { FileName = "somefile.txt" };
			_fakeRepository.Setup(r => r.Get(It.IsAny<string>())).Returns(() => _existingDoc);
		}
	}

	internal class SeamedSkosSourceRepositoryController : SkosSourceRepositoryController
	{
		public bool WasDownloadCalled;
		public IList<string> StreamedToFile = new List<string>();

		public SeamedSkosSourceRepositoryController(ISkosSourceRepository skosSourceRepository, SolrIndex fakeSolrIndex, SkosProcessor fakeProcessor) 
			: base(skosSourceRepository, fakeSolrIndex, fakeProcessor)
		{
		}

		protected override void DownloadSkosSource(Resource<SkosSource> skosSource, string download)
		{
			; // swallow downloading
			WasDownloadCalled = true;
		}
		
		protected override void StreamToFile(string filePathName, System.IO.Stream stream)
		{
			// copy stream to string
			StreamedToFile.Add(StreamHelpers.StreamToString(stream));
		}

		public override IEnumerable<string> DictionaryCollections
		{
			get { return new List<string> {"collection 1", "collection 2"}; }
		}

		public override ITermEnricher CreateTermEnricher
		{
			get
			{
				return new Mock<ITermEnricher>().Object;
			}
		}
	}
}

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
using System.Web;
using System.Web.Mvc;

using Moq;

using MvcContrib.TestHelper;

using NUnit.Framework;
using Trezorix.Checkers.Analyzer.Indexes.Solr;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.Checkers.DocumentChecker.Jobs;
using Trezorix.Checkers.DocumentChecker.Processing;
using Trezorix.Checkers.DocumentCheckerApp.Controllers;
using Trezorix.Checkers.FileConverter;
using Trezorix.ResourceRepository;

namespace DocumentCheckerAppTests
{
	public class BasicDocumentControllerTests
	{
		private BasicDocumentController _documentController; // SUT

		private Mock<IDocumentRepository> _fakeRepo;
		private Resource<Document> _fakeDocument;
		private IJobRepository _fakeJobRepository;
		private IFileConverter _fakeFileConverter;

		private const string ID_OF_THE_FAKE_DOCUMENT = "id_of_the_file";
		private const string THE_SOURCE_FILENAME = "file.file";

		[SetUp]
		public void Setup()
		{
			_fakeRepo = new Mock<IDocumentRepository>();
			
			_fakeJobRepository = new Mock<IJobRepository>().Object;

			_fakeFileConverter = new Mock<IFileConverter>().Object;

			SolrIndex solrIndex = new SolrIndex("NaN");
			_documentController = new BasicDocumentController(_fakeRepo.Object, solrIndex, _fakeJobRepository, () => _fakeFileConverter);
		}

		private void SetupFakeDocument()
		{
			_fakeDocument = new Resource<Document>(new Document("mysourceUri"), @"\\noexist\source")
			{
				FileName = THE_SOURCE_FILENAME,
				Id = ID_OF_THE_FAKE_DOCUMENT
			};

			_fakeRepo.Setup(r => r.Get(ID_OF_THE_FAKE_DOCUMENT)).Returns(_fakeDocument);
			_fakeRepo.Setup(x => x.Create(It.IsAny<Document>(), It.IsAny<string>())).Returns(_fakeDocument);
		}

		[Test]
		public void AdminIndex_should_return_default_view()
		{
			// arrange
			var list = new List<Resource<Document>>() { _fakeDocument };
			_fakeRepo.Setup(r => r.All()).Returns(list);

			// act
			ActionResult result = _documentController.Index();

			// assert
			var model = result.AssertViewRendered()
							.ForView("")
							.WithViewData<IEnumerable<Resource<Document>>>();
			Assert.That(model, Is.SameAs(list));
		}

		[Test]
		public void Upload_GET_should_return_default_view()
		{
			// arrange

			// act
			var result = _documentController.Upload();

			// assert
			result.AssertViewRendered().ForView("");
		}

		[Test]
		public void GetConversion_should_return_FileResult_with_correct_ContentType()
		{
			SetupFakeDocument();

			// arrange
			_fakeDocument.Entity.Status = DocumentState.Ok;

			var conversionFile = new Artifact(DocumentConverter.CONVERSION_ARTIFACT_KEY, "somepath") { ContentType = "application/mytype" };

			_fakeDocument.Artifacts.Add(conversionFile);

			// act
			ActionResult result = _documentController.GetConversion(ID_OF_THE_FAKE_DOCUMENT);

			// assert
			var fileResult = result.AssertResultIs<FileResult>();
			Assert.That(fileResult.ContentType, Is.EqualTo(@"application/mytype"));
		}

		[Test]
		[ExpectedException(typeof(HttpException))]
		public void GetConversion_while_no_conversion_has_started_should_throw_precondition_failure()
		{
			// arrange
			SetupFakeDocument();
			_fakeDocument.Artifacts.Add(new Artifact("someconversion", _fakeDocument.ArtifactFolder));
			_fakeDocument.Entity.Status = DocumentState.Stored;

			// act
			ActionResult result = _documentController.GetConversion(ID_OF_THE_FAKE_DOCUMENT);

			// assert
			// TI: Examine the exception or throw a more explicit one...
		}

		[Test]
		public void Artifact_when_a_document_is_still_converting_should_return_intermediate_conversionfiles()
		{
			// arrange
			SetupFakeDocument();

			const string key = "expected_conversion";
			_fakeDocument.Artifacts.Add(new Artifact(key, _fakeDocument.ArtifactFolder));

			_fakeDocument.Entity.Status = DocumentState.Converting;

			// act
			var result = _documentController.Artifact(ID_OF_THE_FAKE_DOCUMENT, key);

			// assert
			result.AssertResultIs<FileResult>();
		}

		[Test]
		[ExpectedException(typeof(HttpException))]
		public void GetConversion_when_no_conversion_has_started_returns_HttpPreconditionFailed()
		{
			// arrange
			SetupFakeDocument();
			const string expectedLabel = "expected_conversion";

			_fakeDocument.Artifacts.Add(new Artifact(expectedLabel, _fakeDocument.ArtifactFolder));
			_fakeDocument.Entity.Status = DocumentState.Storing;

			// act
			var result = _documentController.GetConversion(ID_OF_THE_FAKE_DOCUMENT);

			// assert

			// TI: examine exception code, or make explicit exception derivatives
		}

		[Test]
		[ExpectedException(typeof(HttpException))]
		public void Artifact_with_a_nonexisting_key_should_return_HttpNotFound()
		{
			// arrange
			SetupFakeDocument();

			_fakeDocument.Entity.Status = DocumentState.Converting;

			// act
			var result = _documentController.Artifact(ID_OF_THE_FAKE_DOCUMENT, "non_existing_conversion");

			// assert
			result.AssertResultIs<HttpNotFoundResult>();
		}
		
	}
}

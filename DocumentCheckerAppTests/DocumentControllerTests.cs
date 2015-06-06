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
using System.Net;
using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using MvcContrib.TestHelper;
using System.Collections.Generic;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.Analyzer.Indexes.Solr;
using Trezorix.Checkers.DocumentChecker;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.Checkers.DocumentChecker.Jobs;
using Trezorix.Checkers.DocumentChecker.Processing;
using Trezorix.Checkers.DocumentChecker.Profiles;
using Trezorix.Checkers.DocumentCheckerApp;
using Trezorix.Checkers.DocumentCheckerApp.Controllers;
using Trezorix.Checkers.DocumentCheckerApp.Models.Documents;
using Trezorix.Checkers.DocumentCheckerApp.Models.Shared;
using Trezorix.ResourceRepository;
using Trezorix.Checkers.FileConverter;

namespace DocumentCheckerAppTests
{
	[TestFixture]
	public class DocumentControllerTests
	{
		private DocumentController _documentController; // SUT

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
			_documentController = new DocumentController(_fakeRepo.Object, solrIndex, _fakeJobRepository, () => _fakeFileConverter);
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
		public void Upload_POST_should_return_ExtJS_Id_json_result()
		{
			// arrange
			ActiveProfile.SetInstance(new Profile()
			                          {
			                          	Key = "Test",
										StopWords = new StopWords()
			                          });

			SetupFakeDocument();

			Mock.Get(_fakeJobRepository).Setup(r => r.GetByLabel("job")).Returns(new Resource<Job>() { Entity = new Job() });

			var fakeFile = new Mock<HttpPostedFileBase>();
			fakeFile.SetupGet(f => f.FileName).Returns("Somefilename.file");

			// act
			var result = _documentController.Upload(fakeFile.Object, "job");
			
			// assert
			result.AssertResultIs<JsonResult>();
			var model = GetModelFromJsonResult<ExtJsDocumentCreationResultJsonModel>((JsonResult)result);
			Assert.AreEqual(ID_OF_THE_FAKE_DOCUMENT, model.id);
		}

		[Test]
		public void Upload_POST_with_no_file_should_return_httpBadRequest()
		{
			// arrange
			SetupFakeDocument();

			//var fakeFile = new Mock<HttpPostedFileBase>();

			// act
			var result = _documentController.Upload(null, "job");

			// assert
			result.AssertResultIs<HttpStatusCodeResult>();
			var statusCodeResult = ((HttpStatusCodeResult)result).StatusCode;
			Assert.AreEqual((int)HttpStatusCode.BadRequest, statusCodeResult);

		}

		// ToDo: Test json get methods when no conversion took place yet

		private static T GetModelFromJsonResult<T>(JsonResult ar)
		{
			return (T)ar.Data;
		}

		[Test]
		public void Index_should_return_all_files_as_json()
		{
			// arrange
			var list = new List<Resource<Document>>()
									{
										_fakeDocument,
										new Resource<Document>(null, "somefolder")
									};
			_fakeRepo.Setup(r => r.All()).Returns(list);

			// act
			ActionResult result = _documentController.Index();
			
			// assert
			var jsonResult = result.AssertResultIs<JsonResult>();
			Assert.That(jsonResult.Data, Is.EqualTo(list));
		}

		[Test]
		public void Details_should_return_default_view()
		{
			// arrange
			SetupFakeDocument();

			// act
			ActionResult result = _documentController.Details(ID_OF_THE_FAKE_DOCUMENT);

			// assert
			var resultData = result.AssertResultIs<JsonResult>().Data;
			var resultModel = (Resource<Document>) resultData;

			Assert.That(resultModel.Id, Is.SameAs(ID_OF_THE_FAKE_DOCUMENT));
		}

		[Test]
		public void Status_should_return_json_with_status_fields()
		{
			// arrange
			SetupFakeDocument();
			
			_fakeDocument.Entity.Status = DocumentState.ProcessingFailed;
			const string aFailureReason = "Conversion failed for some reason.";
			_fakeDocument.Entity.LastError = aFailureReason;
			
			// act
			ActionResult result = _documentController.Status(ID_OF_THE_FAKE_DOCUMENT);
			
			// assert
			var jsonResult = result.AssertResultIs<JsonResult>();
			var resultData = (DocumentStatusJsonModel) jsonResult.Data;
			Assert.AreEqual(DocumentState.ProcessingFailed, resultData.status);
			Assert.AreEqual(aFailureReason, resultData.conversionError);
		}

		[Test]
		public void Get_returns_Document_as_JsonResult()
		{
			// arrange
			SetupFakeDocument();

			// act
			ActionResult result = _documentController.Get(ID_OF_THE_FAKE_DOCUMENT);

			// assert
			var jsonResult = result.AssertResultIs<JsonResult>();
			var resultData = jsonResult.Data as DocumentModel;
			Assert.IsNotNull(resultData, "No data of type DocumentModel");
			Assert.AreEqual(_fakeDocument.Id, resultData.Id);
		}
		
		[Test]
		public void Delete_POST_should_pass()
		{
			// arrange
			_fakeRepo.Setup(r => r.Delete(It.IsAny<string>())).Verifiable("Delete wasn't called on repository");
			
			// act
			ActionResult result = _documentController.Delete("some_id");

			// assert
			//result.AssertActionRedirect().ToAction("Index");
			
			_fakeRepo.VerifyAll();

			var jsonResult = result.AssertResultIs<JsonResult>();
			Assert.IsTrue(((ExtJsResultModel)jsonResult.Data).success);
		}

	}

	//public class DummyDocumentProcessor : IDocumentProcessor
	//{
	//    //public DummyDocumentProcessor(IResourceRepository<Document> repository, IExpandingTokenMatcher index, TikaCommandLineWrapper tikaCommandLineWrapper, StopWords stopWords)
	//    //{
	//    //}

	//    public void ReAnalyse(Resource<Document> document)
	//    {
	//        ;
	//    }

	//    public void Process(Resource<Document> document)
	//    {
	//        ;
	//    }

		

	//}
}

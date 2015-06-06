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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Mvc;
using System.Xml.Linq;
using Trezorix.Common.Meta;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.Checkers.DocumentCheckerApp.Export;
using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.DocumentCheckerApp.Controllers
{
	public class IgnoreAsColumnAttribute : Attribute
	{

	}

	public class ExportController : DocumentControllerBase
	{
		private static IPersistArtifacts<ReviewResult> s_persister;

		public ExportController(IDocumentRepository documentRepository) : base(documentRepository)
		{

		}

		public ExportController() : this(new DocumentRepository(InstanceConfig.Current.DocumentRepositoryPath))
		{
			;
		}

		static ExportController()
		{
			s_persister = new ArtifactPersister<ReviewResult>();
		}

		[TestingSeam]
		internal static void SetArtifactPersister(IPersistArtifacts<ReviewResult> persister)
		{
			s_persister = persister;
		}

		public ActionResult Export(IEnumerable<string> documents)
		{
			var export = CreateExportModel(documents.ToArray());

			//var xml = ConstructModelXml(export);
			//xml.Save(@"C:\Development\Checkers 2\Checkers 2\DocumentCheckerAppTests\ExcelExporterTransformTestXML.xml");

			return new ExcelExportResult<ExportModel>(export);
		}

		public XDocument ConstructModelXml<T>(T model)
		{
			var resultDoc = new XDocument();

			using (var xmlWriter = resultDoc.CreateWriter())
			{
				var xs = new DataContractSerializer(typeof(T));
				xs.WriteObject(xmlWriter, model);

				xmlWriter.Flush();

			}

			return resultDoc;
		}


		private ExportModel CreateExportModel(string[] documents)
		{
			string[] uniqueSkosSources = documents
				.Select(d => Repository.Get(d))
				.Where(r => r.Entity.Status == DocumentState.Reviewed)
				.SelectMany(LoadReviewResult)
				.Select(cr => cr.SkosSourceKey)
				.Distinct().ToArray();

			var export = new ExportModel(uniqueSkosSources);

			foreach (var docId in documents)
			{
			    var documentResource = Repository.Get(docId);
			    if (documentResource == null) continue;
			    // FI: Else what if a document does not exist? 

				if (documentResource.Entity.Status != DocumentState.Reviewed) continue;

				var reviewResult = LoadReviewResult(documentResource);

				var exportDoc = new DocumentExportModel(reviewResult)
									{
										ModificationDate = documentResource.ModificationDate,
										CreationDate = documentResource.CreationDate,
										FileName = documentResource.FileName,
										SourceUri = documentResource.Entity.SourceUri,
										JobLabel = documentResource.Entity.JobLabel,
									};
				
				export.Documents.Add(exportDoc);
			}

			return export;
		}

		private static ReviewResult LoadReviewResult(Resource<Document> documentResource)
		{
			var reviewArtifact = FetchArtifact(documentResource, ReviewController.REVIEW_RESULT_ARTIFACT_KEY);

			ReviewResult reviewResult = s_persister.Revive(reviewArtifact.FilePathName);
			return reviewResult;
		}
	}
}

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
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.DocumentChecker;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.Checkers.DocumentChecker.Processing;
using Trezorix.Checkers.DocumentCheckerApp.Helpers;
using Trezorix.Checkers.DocumentCheckerApp.Models.Review;
using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.DocumentCheckerApp.Controllers
{
	public class ReviewController : DocumentControllerBase
	{
		private static readonly object s_updateLock = new object();

		// ToDo: Find better placement for this constant (it's used from ExportController also)
		internal const string REVIEW_RESULT_ARTIFACT_KEY = "review_result_artifact";

		public ReviewController() : this(new DocumentRepository(InstanceConfig.Current.DocumentRepositoryPath))
		{

		}

		public ReviewController(IDocumentRepository repository)
			: base(repository)
		{

		}

		public ActionResult AnalysisResultRendering(string id)
		{
			Resource<Document> document = FetchDocument(id);
            
            if (!HasAnalysisCompleted(document.Entity.Status))
            {
                throw new HttpException("Analysis has not yet completed. Can't render result.");
            }
			
			var cd = FetchArtifact(document, DocumentProcessor.RESULT_RENDERING_ARTIFACT_KEY);

			var xDocument = XDocument.Load(cd.FilePathName);
			
			var resultXHTML = xDocument.Root.ToString();
            
			ViewData.Model = resultXHTML;

			return View();
		}

		public ActionResult ConceptSuggestions(string id)
		{
			Resource<Document> document = FetchDocument(id);

			if (!HasAnalysisCompleted(document.Entity.Status))
			{
				return new HttpStatusCodeResult((int)HttpStatusCode.PreconditionFailed,
												"Document has not been analyzed, please check status before attempting result download.");
			}

			var cd = FetchArtifact(document, DocumentAnalyzer.ANALYSER_HITS_ARTIFACT_KEY);

			AnalysisResult result = LoadAnalysisResults(cd);

			// flatten hits and only include uniques

			return Json(GetHitsByConcept(result.TextMatches), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult SaveReviewResult(string id, ReviewResult reviewResult)
		{
			Resource<Document> document = FetchDocument(id);

			Artifact artifact = CreateReviewResultArtifact(document);

			AddOrUpdateReviewResultArtifact(document, artifact);

			var persister = new ArtifactPersister<ReviewResult>();

			persister.Persist(artifact.FilePathName, reviewResult);

			document.Entity.Status = DocumentState.Reviewed;

			Repository.Update(document);

			return ExtJsSuccess();
		}

		public ActionResult AcceptAll(string id)
		{
			Resource<Document> document = FetchDocument(id);

			Artifact artifact = CreateReviewResultArtifact(document);

			AddOrUpdateReviewResultArtifact(document, artifact);

			var persister = new ArtifactPersister<ReviewResult>();

			ReviewResult res = new ReviewResult();

			Artifact analysisArtifact = FetchArtifact(document, DocumentAnalyzer.ANALYSER_HITS_ARTIFACT_KEY);
			AnalysisResult hits = LoadAnalysisResults(analysisArtifact);

			var perConceptHM = GetHitsByConcept(hits.TextMatches);
			foreach (ConceptResult conceptResult in perConceptHM
				.Select(pc => new ConceptResult()
					   {
						   Id = pc.Id,
						   Literal = pc.Literal,
						   SkosSourceKey = pc.SkosSourceKey
					   }
			))
			{
				res.Add(conceptResult);
			}


			persister.Persist(artifact.FilePathName, res);

			document.Entity.Status = DocumentState.Reviewed;

			Repository.Update(document);

			return ExtJsSuccess();
		}

		private static void AddOrUpdateReviewResultArtifact(Resource<Document> document, Artifact artifact)
		{
			// lock to guarentee there will be only one artifact using this key
			lock (s_updateLock)
			{
				var artifacts = document.Artifacts.Where(a => a.Key == REVIEW_RESULT_ARTIFACT_KEY).ToList();
				foreach (var d in artifacts)
				{
					document.Artifacts.Remove(d);
				}
				document.Artifacts.Add(artifact);
			}
		}

		private static Artifact CreateReviewResultArtifact(Resource<Document> document)
		{
			// ToDo: Joppe: Remove existing and then add, instead of creating artifact first
			var artifact = new Artifact(REVIEW_RESULT_ARTIFACT_KEY, document.ArtifactFolder)
						   {
							   CreationDate = DateTime.Now,
							   ContentType = "text/xml"
						   };

			artifact.CreationDate = DateTime.Now;
			return artifact;
		}

		public ActionResult JobList()
		{
			return Json(Repository.GetUniqueJobLabels(), JsonRequestBehavior.AllowGet);
		}

		private static IEnumerable<PerConceptHitModel> GetHitsByConcept(IEnumerable<TextMatch> textMatches)
		{
			// ToDo: Store concepts in their own collection in the analysis result, so we don't need distinct
			var results = textMatches
				.SelectMany(tm => tm.ConceptTerms)
				.Select(c => new PerConceptHitModel()
				{
					Id = c.ConceptId,
					SkosSourceKey = c.SkosSourceKey,
					Literal = c.ConceptLabel,
					BroaderLiteral = c.BroaderLabel
				})
				.Distinct().ToList();

			// get and set labels
			foreach (var result in results.ToList())
			{
				var skosSourceBinding = ActiveProfile.Instance().SkosSourceBindings.SingleOrDefault(b => b.Key == result.SkosSourceKey);
				// Make sure the concepts are from a SkosSource that is part of the profile
				if (skosSourceBinding == null)
				{
					// remove dead concepts
					results.Remove(result);
					continue;
				}

				result.SkosSourceLabel =
					skosSourceBinding.Label;
			}

			return results;
		}
	}
}

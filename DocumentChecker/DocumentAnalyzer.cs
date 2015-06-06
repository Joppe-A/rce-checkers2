using System;
using System.Collections.Generic;
using System.Linq;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.Common.Logging;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.Checkers.DocumentChecker.Processing.Fragmenters;
using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.DocumentChecker
{
	public class DocumentAnalyzer 
	{
		private readonly IResourceRepository<Document> _repository;
		private readonly Resource<Document> _document;
		private readonly XmlFragmenter _fragmenter;
		
		private readonly ILog _log;

		private readonly ExpandingTermAnalyzer _termAnalyzer;

		public const string ANALYSER_HITS_ARTIFACT_KEY = "AnalyzerHits";

		internal DocumentAnalyzer(IResourceRepository<Document> repository, Resource<Document> document, XmlFragmenter fragmenter, ExpandingTermAnalyzer termAnalyzer, ILog log)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (repository == null) throw new ArgumentNullException("repository");

			// ToDo: See if we can remove the repository dependency (use callback to update analysis status)
			_repository = repository;
			_termAnalyzer = termAnalyzer;
			_log = log;
			_fragmenter = fragmenter;
			_document = document;
		}
		
		public void Analyse()
		{
			_document.Entity.Status = DocumentState.Analysing;
			_repository.Update(_document);

			var hits = AnalyseFragments(_termAnalyzer, _fragmenter.Fragments());
			
			StoreAnalysisResults(hits);
			
			_document.Entity.Status = DocumentState.RenderingAnalysisResult;
			_repository.Update(_document);
		}

		private AnalysisResult AnalyseFragments(ExpandingTermAnalyzer analyser, IEnumerable<Fragment> fragments)
		{
			_log.Log("Analysing fragments.");
			var hits = new AnalysisResult(); // Register hits per fragment

			foreach (var fragment in fragments)
			{
				var fragmentHits = analyser.Analyse(fragment.Value);
				if (fragmentHits.Any())
				{
					var newFrag = new FragmentTokenMatches()
					              {
					              	Fragment = fragment,
					              	TokenMatches = fragmentHits
					              };

					hits.FragmentTokenMatches.Add(newFrag);
				}

				_log.Log(String.Format("Fragment analysis resulted in {0} hits.", fragmentHits.Count()));
			}

			hits.TextMatches = analyser.TextMatches;

			_log.Log("All fragments analysed, analysis completed.");
			return hits;
		}

		private void StoreAnalysisResults(AnalysisResult hits)
		{
			var newArtifact =_document.AddArtifact(ANALYSER_HITS_ARTIFACT_KEY);
			
			newArtifact.CreationDate = DateTime.Now;
			newArtifact.ContentType = "text/xml";
			
			new ArtifactPersister<AnalysisResult>().Persist(newArtifact.FilePathName, hits);
		}
	}
}
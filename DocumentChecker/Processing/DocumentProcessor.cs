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
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.Analyzer.Matchers;
using Trezorix.Checkers.Common.Logging;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.Checkers.DocumentChecker.Processing.Fragmenters;
using Trezorix.Checkers.FileConverter;
using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.DocumentChecker.Processing
{
	public class DocumentProcessor
	{
		public const string LOG_ARTIFACT_KEY = "document_log_artifact";
		
		public const string RESULT_RENDERING_ARTIFACT_KEY = "result_rendering";

		private readonly IResourceRepository<Document> _repository;
		private readonly StopWords _stopWords;

		private readonly IExpandingTokenMatcher _matcher;
		
		private ILog _log;
		private readonly IFileConverter _fileConverter;

		public DocumentProcessor(IResourceRepository<Document> repository, StopWords stopWords, IExpandingTokenMatcher matcher, IFileConverter fileConverter)
		{
			if (repository == null) throw new ArgumentNullException("repository");
			if (stopWords == null) throw new ArgumentNullException("stopWords");
			if (matcher == null) throw new ArgumentNullException("matcher");
			
			if (fileConverter == null) throw new ArgumentNullException("fileConverter");
			_fileConverter = fileConverter;
			
			_repository = repository;
			_matcher = matcher;
			
			_stopWords = stopWords;
		}

		public void Process(Resource<Document> document)
		{
			_log = BuildLogArtifact(document);
			
			_log.Log(String.Format("Processing document '{0}' with filename '{1}'.", document.Id, document.FileName));

			try
			{
				ConvertDocument(document);

				AnalyzeDocument(document, CreateTermAnalyzer());

				_log.Log("Document processing completed.");

				document.Entity.LastError = null;
			}
			catch (Exception ex)
			{
				_log.Log("Document processing ended in error: " + ex.Message);
				document.Entity.LastError = ex.Message;
				document.Entity.Status = DocumentState.ProcessingFailed;
				_repository.Update(document);
				throw;
			}

			_repository.Update(document);

		}

		private void CreateResultRendering(Resource<Document> document)
		{
			var cd = FetchArtifact(document, DocumentConverter.CONVERSION_ARTIFACT_KEY);

			var xDocument = XDocument.Load(cd.FilePathName);
			xDocument.Declaration = new XDeclaration("1.0", "utf-8", null);
			var tikaConversion = xDocument.Root.Element(XName.Get("body", "http://www.w3.org/1999/xhtml"));

			var ad = FetchArtifact(document, DocumentAnalyzer.ANALYSER_HITS_ARTIFACT_KEY);
			AnalysisResult analysisResult = LoadAnalysisResults(ad);

			var previewRenderer = new ResultXHTMLRenderer(tikaConversion, analysisResult);

			var result = previewRenderer.Render();

			SaveResultRenderingArtifact(document, result);
		}

		private void SaveResultRenderingArtifact(Resource<Document> document, XElement result)
		{
			var rra = document.AddArtifact(RESULT_RENDERING_ARTIFACT_KEY);

			if (File.Exists(rra.FilePathName))
			{
				File.Delete(rra.FilePathName);
			}

			result.Save(rra.FilePathName);

			rra.ContentType = "xhtml";

			document.Entity.Status = DocumentState.ReadyForReview;
			
			_repository.Update(document);
		}

		protected static AnalysisResult LoadAnalysisResults(Artifact af)
		{
			var persister = new ArtifactPersister<AnalysisResult>();

			return persister.Revive(af.FilePathName);
		}

		private static Artifact FetchArtifact(Resource<Document> document, string key)
		{
			var cd = document.Artifacts.SingleOrDefault(a => a.Key == key);
			if (cd == null)
			{
				throw new Exception(String.Format("No artifact '{0}' found for document {1}", key, document.Id));
			}
			return cd;
		}

		private ExpandingTermAnalyzer CreateTermAnalyzer()
		{
			return new ExpandingTermAnalyzer(_matcher, _stopWords, _log);
		}

		public void AnalyzeDocument(Resource<Document> document, ExpandingTermAnalyzer termAnalyser)
		{
			_log.Log("Starting analysis.");

			Artifact toAnalyse = document.Artifacts.Single(a => a.Key == DocumentConverter.CONVERSION_ARTIFACT_KEY);

			using (var input = new FileStream(toAnalyse.FilePathName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				try
				{
					DocumentAnalyzer analyzer = CreateDocumentAnalyzer(document, input, termAnalyser);
					
					analyzer.Analyse();

					_log.Log("Analysis completed.");

					_log.Log("Create result XHTML rendering.");
					CreateResultRendering(document);

					_log.Log("Result XHTML rendered.");

				}
				catch (Exception ex)
				{
					_log.Log("Analysis ended in error: " + ex.Message);
					toAnalyse.Error = ex.Message;
					throw;
				}
			}

		}

		private void ConvertDocument(Resource<Document> document)
		{
			_log.Log("Starting conversion.");

			var converter = CreateDocumentConverter();

			converter.Convert(document);

			_log.Log("Conversion completed.");
		}

		private ILog BuildLogArtifact(Resource<Document> document)
		{
			var logArtifact = document.AddArtifact(LOG_ARTIFACT_KEY);
			
			_repository.Update(document);

			ILog log = new FileLogger(logArtifact.FilePathName);
			return log;
		}

		private void RemoveAnalysisArtifacts(Resource<Document> document)
		{
			document.Artifacts.RemoveWhere(x => x.Key == DocumentAnalyzer.ANALYSER_HITS_ARTIFACT_KEY);
			document.Artifacts.RemoveWhere(x => x.Key == LOG_ARTIFACT_KEY);
			document.Artifacts.RemoveWhere(x => x.Key == RESULT_RENDERING_ARTIFACT_KEY);
		}

		public void ReAnalyse(Resource<Document> document)
		{
			// FI: perhaps keep the old log and analysis result?
			RemoveAnalysisArtifacts(document);

			_log = BuildLogArtifact(document);
			
			AnalyzeDocument(document, CreateTermAnalyzer());

		}

		private DocumentConverter CreateDocumentConverter()
		{
			return new DocumentConverter(_repository, _fileConverter, _log);
		}

		private DocumentAnalyzer CreateDocumentAnalyzer(Resource<Document> document, Stream input, ExpandingTermAnalyzer termAnalyzer)
		{
			_log.Log("Setting up Xml fragmenter");
			var fragmenter = CreateFragmenter(input);
			_log.Log("Setting up Xml document analyzer");
			
			return new DocumentAnalyzer(_repository, document, fragmenter, termAnalyzer,_log);
		}

		private static XmlFragmenter CreateFragmenter(Stream input)
		{
			return new XmlFragmenter(input) { FragmentRoot = "body" };
		}
	}
}

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

using Trezorix.Checkers.Common.Logging;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.ResourceRepository;
using Trezorix.Checkers.FileConverter;

namespace Trezorix.Checkers.DocumentChecker.Processing
{
	public class DocumentConverter
	{

        public const string CONVERSION_ARTIFACT_KEY = "tika_xhtml";

		private readonly IResourceRepository<Document> _resourceRepository;
		
		private string _errorMessage;

		private Resource<Document> _document; // make this a class a method no the Document class itself?!

		private ILog _log;

		private readonly IFileConverter _tikaFileConverter;

		public DocumentConverter(IResourceRepository<Document> resourceRepository, IFileConverter converter, ILog log)
		{
			_resourceRepository = resourceRepository;
			
			_tikaFileConverter = converter;

			_log = log;
		}
		
		public void Convert(Resource<Document> document)
		{
			_document = document;
			
			document.Entity.Status = DocumentState.Converting;
			
			_log.Log(String.Format("Starting tika file conversion."));

			Convert(CONVERSION_ARTIFACT_KEY, _tikaFileConverter, document.FilePathName());
				
			_log.Log(String.Format("Tika conversion completed."));
			
			document.Entity.Status = DocumentState.Converted;
		}
		
		protected Artifact Convert(string key, IFileConverter converter, string sourceFilePathName)
		{
			_errorMessage = null;

			Artifact conversionArtifact = null;
			try
			{
				conversionArtifact = BuildConversionArtifact(key);
				
				converter.Convert(sourceFilePathName, conversionArtifact.FilePathName);

				conversionArtifact.ContentType = converter.OutputContentType;
				_resourceRepository.Update(_document);
			}
			catch (Exception ex)
			{
				if (conversionArtifact != null)
				{
					conversionArtifact.Error = String.Format("Converter {0} failed converting file {1}, exception message: {2}",
																		  converter.GetType(), _document, ex.Message);
					_errorMessage = conversionArtifact.Error;
				}
				throw;
			}
			return conversionArtifact;
		}

		private Artifact BuildConversionArtifact(string key)
		{
			var artifact = _document.AddArtifact(key);
			
			artifact.CreationDate = DateTime.Now;
			_resourceRepository.Update(_document);

			return artifact;
		}

		public string ErrorMessage
		{
			get { return _errorMessage; }
		}

	}
}
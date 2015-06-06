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
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Trezorix.Checkers.Analyzer.Indexes;
using Trezorix.Checkers.Analyzer.Indexes.Solr.SkosXmlTransformer;
using Trezorix.Checkers.DocumentChecker.SkosSources;
using Trezorix.ResourceRepository;
using Trezorix.Treazure.API;

namespace Trezorix.Checkers.ManagerApp.Services
{
	public class SkosProcessor
	{
		public virtual void Process(Resource<SkosSource> skosSource, ISkosSourceRepository repository)
		{
			skosSource.Artifacts.RemoveWhere(a => a.Key == SkosSource.solr_feed_artifact_key);

			skosSource.Entity.Status = SkosSourceState.SkosProcessing;
			repository.Update(skosSource);

			ITermEnricher termEnricher;
			if (string.IsNullOrWhiteSpace(ManagerAppConfig.DictionaryConnectionString) && skosSource.Entity.TermEnricherSettings.Enabled)
			{
				termEnricher = new SearchWords(ManagerAppConfig.DictionaryConnectionString);
			}
			else
			{
				termEnricher = new NullEnricher();
			}

			try
			{
				CreateSolrUpdateFeedArtifact(termEnricher, skosSource);
			}
			finally
			{
				if (termEnricher is IDisposable) ((IDisposable)termEnricher).Dispose();	
			}
			
			skosSource.Entity.Status = SkosSourceState.SkosProcessed;
			repository.Update(skosSource);
		}

		private static void CreateSolrUpdateFeedArtifact(ITermEnricher termEnricher, Resource<SkosSource> document)
		{
			// ToDo: Perhaps this method needs to (partly) be a business layer somewhere (this should be a method on the SkosSource class perhaps, minus the artifact creation bit (need a filename))
			using (var reader = new StreamReader(document.FilePathName(), Encoding.UTF8))
			using (var skosXmlInput = new XmlTextReader(reader))
			{
				var skosToSolr = new SkosXmlToSolrIndexUpdateXml(termEnricher);
				try
				{
					var solrUpdate = skosToSolr.CreateSolrIndexUpdateXml(document.Entity.Key, document.Entity.TermEnricherSettings.DictionaryCollectionName, skosXmlInput);
					var solrArtifact = new Artifact(SkosSource.solr_feed_artifact_key, document.ArtifactFolder);
					document.Artifacts.Add(solrArtifact);
					solrUpdate.Save(solrArtifact.FilePathName, SaveOptions.None);
				}
				catch (XmlException ex)
				{
					document.Entity.LastError = "Not a Skos file or file not properly formatted: " + ex.Message;
				}
			}
		}

		
	}
}
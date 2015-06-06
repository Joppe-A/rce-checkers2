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
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace Trezorix.Checkers.Analyzer.Indexes.Solr.SkosXmlTransformer
{
	public class SkosXmlToSolrIndexUpdateXml
	{
		private static readonly XslCompiledTransform s_transformer = GetTransformer();

		static SkosXmlToSolrIndexUpdateXml()
		{
			// explicit static constructor to make statics lazy
		}

		private readonly ITermEnricher _termEnricher;

		public SkosXmlToSolrIndexUpdateXml(ITermEnricher termEnricher)
		{
			if (termEnricher == null) throw new ArgumentNullException("termEnricher");

			_termEnricher = termEnricher;
		}

		public XDocument CreateSolrIndexUpdateXml(string skosKey, string dictionaryCollectionName, XmlReader skosXmlInput)
		{
			// TI: Inject the writer; in production just streaming to a text file might be faster (instead of building a DOM and then persisting it)
			var doc = new XDocument();
			using (var writer = doc.CreateWriter())
			{
				var args = new XsltArgumentList();
				args.AddExtensionObject("ext:Term", new SkosTermIndexerXslExtensions(dictionaryCollectionName, _termEnricher));
				
				args.AddParam("skos_key", "", skosKey ?? "");

				s_transformer.Transform(skosXmlInput, args, writer);
				writer.Close();
			}
			return doc;
		}

		private static XslCompiledTransform GetTransformer()
		{
			var transformer = new XslCompiledTransform();
			XmlReader xslReader = GetXslFromResource();
			transformer.Load(xslReader);
			return transformer;
		}

		private static XmlReader GetXslFromResource()
		{
			var assembly = Assembly.GetExecutingAssembly();

			var xslStream = assembly.GetManifestResourceStream(typeof(SkosXmlToSolrIndexUpdateXml), "SkosXmlToTermIndexerExtensions.xslt");
			if (xslStream == null) throw new ApplicationException("Xsl file missing from assembly resources");
			var reader = XmlReader.Create(xslStream);
				
			return reader;
		}
	}
}

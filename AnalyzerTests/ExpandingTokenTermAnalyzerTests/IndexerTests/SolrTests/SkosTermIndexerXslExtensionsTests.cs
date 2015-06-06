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
using NUnit.Framework;
using Trezorix.Checkers.Analyzer.Indexes;
using Trezorix.Checkers.Analyzer.Indexes.Solr.SkosXmlTransformer;

namespace AnalyzerTests.ExpandingTokenTermAnalyzerTests.IndexesTests.SolrTests
{
	[TestFixture]
	public class SkosTermIndexerXslExtensionsTests
	{
		[Test]
		public void Extension_should_partialize_in_inverse_order()
		{
			// arrange
			var extension = new SeamedSkosTermIndexerXslExtensions.Builder().Build();

			var testFeed = new List<string>
			               	{
			               		"de",
			               		"groene",
			               		"draak",
								"gaat",
								"los"
			               	};

			// act

			// Seamed method call
			var results = extension.TestCreatePartials(testFeed);

			var resultsList = results.ToList();

			// assert
			Assert.AreEqual("de groene draak gaat", resultsList[0]);
			Assert.AreEqual("de groene draak", resultsList[1]);
			Assert.AreEqual("de groene", resultsList[2]);
			Assert.AreEqual("de", resultsList[3]);
		}

		public class SeamedSkosTermIndexerXslExtensions : SkosTermIndexerXslExtensions
		{
			public class Builder
			{
				public Builder()
				{
					DictionaryCollection = null;
					TermEnricher = null;
				}

				public string DictionaryCollection { get; set; }

				public ITermEnricher TermEnricher { get; set; }

				public SeamedSkosTermIndexerXslExtensions Build()
				{
					return new SeamedSkosTermIndexerXslExtensions(DictionaryCollection, TermEnricher);
				}

			}

			public SeamedSkosTermIndexerXslExtensions(string dictionaryCollection, ITermEnricher termEnricher)
				: base(dictionaryCollection, termEnricher)
			{
				
			}

			public IEnumerable<string> TestCreatePartials(IEnumerable<string> source)
			{
				return base.CreatePartials(source);
			}
		}
	}
}

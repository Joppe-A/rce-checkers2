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
using System.Collections;
using System.IO;
using System.Linq;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.Analyzer.Indexes.Solr;
using Trezorix.Checkers.Analyzer.Matchers;
using Trezorix.Checkers.Common.Logging;

namespace Trezorix.Checkers.ExcelXmlChecker
{
	public class Checker
	{
		private ExpandingTermAnalyzer _analyzer;
		private ExcelXmlCheckerSetup _setup;
		private Parser _parser;

		public Checker(ExcelXmlCheckerSetup setup)
		{
			_setup = setup;
		}

		public AnalysisResult Check()
		{
			InitAnalyzer();

			InitExcelXmlParser();

			AnalysisResult result = Analyze();

			return result;
		}

		private AnalysisResult Analyze()
		{
			var result = new AnalysisResult();

			foreach (var textSource in _parser.TextContent)
			{
				if (textSource.Text != null)
				{
					var hits = _analyzer.Analyse(textSource.Text);

					if (hits.Any())
					{
						result.AddRowMatches(textSource.Id, hits, textSource.Text);
					}
				}
			}
			
			result.TextMatches = _analyzer.TextMatches;

			return result;
		}

		private void InitExcelXmlParser()
		{
			_parser = new Parser(_setup.InputFile, _setup.IdColumn, _setup.TextColumn)
			       	{
			       		HeaderRows = _setup.HeaderRows
			       	};
		}

		private void InitAnalyzer()
		{
			var solrIndex = new SolrIndex(ExcelXmlCheckerConfig.SolrIndexUrl); //ToDo: Grab this from commandline args also?
			IExpandingTokenMatcher matcher = new SolrExpandingTokenMatcher(solrIndex, 
			                                                               new MatcherFilter()
			                                                               	{
			                                                               		Language = "dut",
																				SkosSources = _setup.SkosSources
			                                                               	});
			
			var log = new NullLogger(); // ToDo: Create a console logger

			// FI: Wire up stopwords?
			_analyzer = new ExpandingTermAnalyzer(matcher, null, log);
		}
	}
}

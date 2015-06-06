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
using System.Diagnostics;
using System.IO;
using System.Linq;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.Analyzer.Indexes.Solr;
using Trezorix.Checkers.Analyzer.Matchers;
using Trezorix.Checkers.Analyzer.Tokenizers;
using Trezorix.Checkers.Common.Logging;
using Trezorix.Checkers.DocumentChecker.Processing.Fragmenters;

namespace Trezorix.Checkers.AnalyzerPerformanceTest
{
	class Program
	{
		static void Main(string[] args)
		{
			IExpandingTokenMatcher expandingTokenMatcher = new SolrExpandingTokenMatcher(new SolrIndex("http://localhost:8080/checker2solr"), new MatcherFilter() { Language = "dut" });

			var index = new ExpandingTermAnalyzer(expandingTokenMatcher, null, new DummyLog());

			var xf = new XmlFragmenter(File.OpenRead("tika_xhtml_artifact")) { FragmentRoot = "body" };
			
			IEnumerable<Fragment> xmlFragments = xf.Fragments().ToList();

			var aggregatedHits = new List<Token>();

			var ts = new Stopwatch();

			ts.Start();

			for (int i =0; i < 5; i++)
			{
				foreach(var fragment in xmlFragments)
				{
					if (!string.IsNullOrEmpty(fragment.Value))
					{
						var hits = index.Analyse(fragment.Value);
						aggregatedHits.AddRange(hits);
						Console.Write("." + new string('_', fragment.Value.Length / 100) + hits.Count());
					}
					else
					{
						Console.Write(".-");
					}
				}
				
				Console.Write(".|");
			}
			Console.WriteLine(".");

			ts.Stop();
			Console.WriteLine("Time taken: " + ts.Elapsed);
			Console.WriteLine("Done. press key.");
			Console.ReadKey();
		}

		private class DummyLog : ILog
		{
			
			public void WarnException(string message, Exception exception)
			{
			}

			public void Log(string message)
			{
			}

			public void Info(string message)
			{
			}

			public void Error(string message)
			{
			}

			public void ErrorException(string message, Exception exception)
			{
			}

			public void Warn(string message)
			{
			}
		}
	}
}

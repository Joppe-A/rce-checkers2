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
using Moq;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.Analyzer.Matchers;
using Trezorix.Checkers.Common.Logging;

namespace AnalyzerTests
{
	public class ExpandingTokenTermAnalyzerBuilder
	{
		public ExpandingTokenTermAnalyzerBuilder()
		{
			ExpandingTokenMatcher = new Mock<IExpandingTokenMatcher>().Object;
			StopWords = null;
			Log = new MemoryLogger();
		}

		public IExpandingTokenMatcher ExpandingTokenMatcher { get; set; }

		public StopWords StopWords { get; set; }

		public ILog Log { get; set; }

		public ExpandingTermAnalyzer Build()
		{
			return new ExpandingTermAnalyzer(ExpandingTokenMatcher, StopWords, Log);
		}

		public static ExpandingTermAnalyzer BuildDefault()
		{
			return new ExpandingTokenTermAnalyzerBuilder().Build();
		}
	}
}
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

namespace Trezorix.Checkers.Analyzer.Matchers
{
	public class TokenMatch
	{
		private DefferedMatchResult _termMatches;
		
		public MatchType MatchType;
		
		public IEnumerable<ConceptTerm> TermMatches { get { return _termMatches(); } }

		private TokenMatch()
		{
			
		}

		public static TokenMatch CreatePartial()
		{
			return new TokenMatch()
			       	{
			       		MatchType = MatchType.Partial
			       	};
		}

		public delegate IEnumerable<ConceptTerm> DefferedMatchResult();

		public static TokenMatch CreateFull(DefferedMatchResult termMatches)
		{
			return new TokenMatch()
			       	{
			       		MatchType = MatchType.Full,
			       		_termMatches = termMatches
					};
		}

		public static TokenMatch CreateFullAndPartial(DefferedMatchResult termMatches)
		{
			return new TokenMatch()
			{
				MatchType = MatchType.FullAndPartial,
				_termMatches = termMatches
			};
		}

	}

	public enum MatchType
	{
		Partial,
		Full,
		FullAndPartial
	}

}
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
using System.Linq;
using Trezorix.Checkers.Analyzer.Matchers;
using Trezorix.Checkers.Analyzer.Tokenizers;
using Trezorix.Checkers.Common.Logging;

namespace Trezorix.Checkers.Analyzer
{
	public class ExpandingTermAnalyzer
	{
		private readonly IExpandingTokenMatcher _expandingTokenMatcher;
		private readonly ILog _log;
		
		private readonly StopWords _stopWords;
		private List<string> _nonOverlappingStopWords;

		private readonly TextMatches _textMatches;
		
		public TextMatches TextMatches
		{
			get { return _textMatches; }
		}

		public ExpandingTermAnalyzer(IExpandingTokenMatcher expandingTokenMatcher, StopWords stopWords, ILog log)
		{
			_expandingTokenMatcher = expandingTokenMatcher;
			_log = log;
			
			_stopWords = stopWords ?? new StopWords();
			
			_textMatches = new TextMatches();
		}

		public IEnumerable<Token> Analyse(string text)
		{
			if (text == null) throw new ArgumentNullException("text");

			_log.Log("Gathering stop words.");

			GetNonOverlappingStopWords();
			
			_log.Log("Preparing fragment token stream.");

			var tokens = new TermTokenizer(text).Tokens();

			_log.Log("Tokenizers prepared to stream.");

			_log.Log("Sending document token stream to token matcher");
			
			var hits = MatchTokens(tokens.GetEnumerator());

			_log.Log("Token matching completed, hits collected.");

			return hits;
		}

		private IEnumerable<Token> MatchTokens(IEnumerator<Token> tokenEnum)
		{
			var hits = new List<Token>();

			LastTermMatch? lastSeenResultWithFullMatches = null;

			if (!tokenEnum.MoveNext())
			{
				_log.Log("No tokens in token stream, matching cancelled.");	
				return hits;
			}

			Token token = tokenEnum.Current;

			var reseekTokens = new Queue<Token>(50);
			var lookAheadTokens = new Queue<Token>(10);

			// TI: Stopword check the first Token -> don't match those, only match them combined with next token
			while (token != null)
			{
				_log.Log("Stop word checking " + token.Value);

				// skip stop words
				if (IsStopWord(token.Value))
				{
					_log.Log(string.Format("Token '{0}' is a stop word -> immediately expanding.", token));
					
					token = ExpandToken(token, tokenEnum, lookAheadTokens, reseekTokens);
					if (token == null) return hits;
				}

				_log.Log("Considering " + token.Value);

				var match = _expandingTokenMatcher.Match(token.Value);

				// continue scanning untill we have no partial matches left
				while (match != null && match.MatchType != MatchType.Full)
				{
					_log.Log("Token matched, analysing match type.");

					if (match.MatchType == MatchType.FullAndPartial)
					{
						// remember these full matches while we search further on the partials
						lastSeenResultWithFullMatches = LastTermMatch.Register(match, token);
						// we can clear the look ahead queue now
						// all tokens up to here are matched
						lookAheadTokens.Clear();

						_log.Log(string.Format("Token {0} has matched both partial terms and full terms, continuing search to find the most significant.", token));
					}

					token = ExpandToken(token, tokenEnum, lookAheadTokens, reseekTokens);
					if (token == null)
					{
						_log.Log("No more token in stream, end of token expansion.");
						break;
					}

					_log.Log("Considering " + token.Value);
					match = _expandingTokenMatcher.Match(token.Value);
				}

				// correct loop tail
				if (match != null && match.MatchType == MatchType.Full)
				{
					lastSeenResultWithFullMatches = LastTermMatch.Register(match, token);
					lookAheadTokens.Clear();

					_log.Log(string.Format("Token {0} resulted in a term match.", token));
				}
				
				if (lastSeenResultWithFullMatches != null)
				{
					LastTermMatch lastTermMatch = lastSeenResultWithFullMatches.Value;

					_log.Log(string.Format("Term match found for token {0}, registering term matches.", lastTermMatch.Token));

					if (!_textMatches.Contains(lastTermMatch.Token.Value))
					{
						var hitStopWordLanguages = GetStopWordLanguages(lastTermMatch.Token.Value).ToArray();

						if (hitStopWordLanguages.Any())
						{
							_log.Log(string.Format("Registered hit is a potential stopword ({0}), flagging the result as such.", lastTermMatch.Token.Value));
						}

						_textMatches.RegisterTokenHit(lastTermMatch.Token.Value, lastTermMatch.TokenMatch.TermMatches, hitStopWordLanguages);
					} // FI: Else add to some counter?

					hits.Add(lastTermMatch.Token);

					lastSeenResultWithFullMatches = null;
					
					_log.Log(string.Format("Token {0} term matches registered.", lastTermMatch.Token));
				}

				LookAheadRestore(lookAheadTokens, reseekTokens);
				
				// as long as we have tokens in the reseekTokens queue grab tokens from it
				token = NextToken(tokenEnum, reseekTokens);
			}
			return hits;
		}

		private void GetNonOverlappingStopWords()
		{
			var stopwords = new List<string>();
			foreach(var s in _stopWords.Select(s => s.Word).Distinct())
			{
				var stopWordMatch = _expandingTokenMatcher.Match(s);
				
				if (stopWordMatch == null || (stopWordMatch.MatchType != MatchType.Full && stopWordMatch.MatchType != MatchType.FullAndPartial))
				{
					stopwords.Add(s);
				}
			}

			_nonOverlappingStopWords = stopwords;
		}

		private IEnumerable<string> GetStopWordLanguages(string value)
		{
			return _stopWords.Where(s => s.Word == value).Select(s => s.Language).Distinct();
		}

		private bool IsStopWord(string value)
		{
			return _nonOverlappingStopWords.Any(s => s == value);
		}

		private Token ExpandToken(Token currentToken, IEnumerator<Token> tokenEnum, Queue<Token> lookAheadTokens, Queue<Token> reseekTokens)
		{
			Token nextToken = NextToken(tokenEnum, reseekTokens);
			if (nextToken == null) return null;

			_log.Log(string.Format("Expanding token {0} to {1}.", currentToken.Value, nextToken.Value));

			int combinedLength = (nextToken.Position.Length + nextToken.Position.Start) - currentToken.Position.Start;
			currentToken = Token.Create(currentToken.Value + " " + nextToken.Value, currentToken.Position.Start, combinedLength);

			lookAheadTokens.Enqueue(nextToken);
			return currentToken;
		}

		// FI: Allow getting hits for lesser significant finds also (it's currently skipping matches if it finds a longer term overlapping the entire smaller term)

		private struct LastTermMatch
		{
			public Token Token { get; private set; }
			public TokenMatch TokenMatch { get; private set; }

			public static LastTermMatch Register(TokenMatch tokenMatch, Token token)
			{
				var ltm = new LastTermMatch
				          	{
				          		Token = token,
								TokenMatch = tokenMatch
				          	};
				return ltm;
			}
		}

		private void LookAheadRestore(Queue<Token> lookAheadTokens, Queue<Token> reseekTokens)
		{
			if (lookAheadTokens.Any())
			{
				_log.Log("Restoring look ahead token stack.");
				foreach (var reseekToken in lookAheadTokens)
				{
					reseekTokens.Enqueue(reseekToken);
				}
				lookAheadTokens.Clear();	
			}
		}

		private static Token NextToken(IEnumerator<Token> tokenEnum, Queue<Token> reseekTokens)
		{
			if (reseekTokens.Any())
			{
				return reseekTokens.Dequeue();
			}
			
			if (!tokenEnum.MoveNext()) return null;
			return tokenEnum.Current;
		}

	}

}
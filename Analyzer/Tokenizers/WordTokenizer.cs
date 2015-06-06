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

namespace Trezorix.Checkers.Analyzer.Tokenizers
{
	public class WordTokenizer : ITokenizer
	{
		
		public IEnumerable<Token> Tokenize(Token inputToken)
		{
			if (inputToken == null) throw new ArgumentNullException("inputToken");

			string input = inputToken.Value;

			int startPos = 0;

			do
			{
				// scan for start of word
				while (!IsWordCharacter(input[startPos])) 
				{
					startPos++;
					if (startPos == input.Length) yield break;
				}

				int endPos = startPos + 1;

				while (endPos < input.Length && IsWordCharacter(input[endPos]))
				{
					endPos++;	
				}
				var length = endPos - startPos;
				yield return Token.Create(input.Substring(startPos, length), startPos + inputToken.Position.Start, length);
				startPos = endPos + 1;
			} 
			while (startPos < input.Length);
		}

		protected virtual bool IsWordCharacter(char testChar)
		{
			return Char.IsLetterOrDigit(testChar) || testChar == '-';
		}

		public IEnumerable<Token> Tokenize(IEnumerable<Token> tokens)
		{
			return tokens.SelectMany(Tokenize);
			
		}
	}
}
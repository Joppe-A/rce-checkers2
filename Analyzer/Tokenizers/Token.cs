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
using System.Runtime.Serialization;

namespace Trezorix.Checkers.Analyzer.Tokenizers
{
	[DataContract]
	public class Token
	{
		[DataMember]
		public string Value { get; private set; }
		
		[DataMember]
		public TokenPosition Position { get; private set; }

		public Token()
		{
			; //needed for deserialization
		}

		private Token(string value, TokenPosition position)
		{
			Position = position;
			Value = value;
			
		}

		public static Token Create(string value)
		{
			return Create(value, 0, 0);
		}

		public static Token Create(string value, int startPosition)
		{
			return new Token(value, TokenPosition.Create(startPosition, value.Length));
		}
		
		public static Token Create(string value, TokenPosition position)
		{
			return new Token(value, position);
		}
		
		public static Token Create(string value, int startPosition, int length)
		{
			return new Token(value, TokenPosition.Create(startPosition, length));
		}

		public override string ToString()
		{
			return string.Format("'{0}'-[{1}]", Value, Position);
		}
	}
}
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
using System.Xml;

namespace Trezorix.Checkers.ExcelXmlChecker
{
	public class Parser
	{
		private readonly string _inputFile;
		private readonly int _idCell;
		private readonly int _textCell;
		private int _headerRows;

		public Parser(string inputFile, int idCell, int textCell)
		{
			if (idCell == textCell) throw new ArgumentException();
			_inputFile = inputFile;
			_idCell = idCell;
			_textCell = textCell;
		}

		public IEnumerable<TextContent> TextContent
		{
			get
			{
				// open file
				using(var reader = XmlReader.Create(_inputFile))
				{
					// seek till first table element
					if (!reader.Read()) yield break;

					reader.ReadToFollowing("Table");

					// skip header rows
					foreach(var textContent in ParseSheet(reader))
					{
						yield return textContent;	
					}
					
				}
				yield break;
			}
		}

		private IEnumerable<TextContent> ParseSheet(XmlReader sheetReader)
		{
			int rowCounter = 1;

			// grab a subTree to avoid reading from the next sheet without needing to explicitly track the sheet's end element
			var reader = sheetReader.ReadSubtree();

			for (int i = 0; i < HeaderRows; i++)
			{
				rowCounter++;
				reader.ReadToFollowing("Row");
			}

			// loop rows
			int maxCellIndex = _textCell > _idCell ? _textCell : _idCell;

			while(reader.ReadToFollowing("Row") )
			{
				rowCounter++;
				XmlReader rowReader = reader.ReadSubtree();

				string text = null;
				string id = null;

				int cellIndex = 0;
				while (cellIndex <= maxCellIndex && rowReader.ReadToFollowing("Cell"))
				{
					rowReader.ReadToFollowing("Data");

					if (cellIndex == _textCell)
					{
						text = rowReader.ReadElementContentAsString();
					}
					else if (cellIndex == _idCell)
					{
						id = rowReader.ReadElementContentAsString();
					}
							
					if (BothValuesFound(text, id))
					{
						yield return new TextContent(id, text);
						break;
					}

					cellIndex++;
				}
						
				if (!string.IsNullOrWhiteSpace(id))
				{
					if (text == null)
					{
						yield return new TextContent(id, null);	
					}
				}
				else
				{
					throw new InvalidExcelFileException(string.Format("Row {0} doesn't contain an Id value.", rowCounter));
				}
							
			}
		}

		private static bool BothValuesFound(string text, string id)
		{
			return !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(text);
		}

		public int HeaderRows
		{
			get { return _headerRows; }
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException("HeaderRows", value, "HeaderRows can't be negative, 0 or larger required.");
				_headerRows = value;
			}
		}
	}

	public class InvalidExcelFileException : Exception
	{
		public InvalidExcelFileException(string message)
			: base(message)
		{
		}
	}

	public struct TextContent
	{
		public string Text { get; private set; }
		public string Id { get; private set; }

		public TextContent(string id, string text)
			: this()
		{
			Text = text;
			Id = id;
		}
	}
}
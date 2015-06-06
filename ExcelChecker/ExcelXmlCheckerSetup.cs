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

namespace Trezorix.Checkers.ExcelXmlChecker
{
	public struct ExcelXmlCheckerSetup
	{
		/*
		 * ExcelXmlChecker.exe inputXml idCellIndex textCellIndex outputXml -headerRows:X -skosSource:XX1 -skosSource:XX2
		 */

		// TI: These attributes aren't implemented.. ran out of time to complete it

		[FixedPositionCmdArgument(0)]
		[FilePathNameValue(CheckExistance = true)]
		public string InputFile;
		
		[FixedPositionCmdArgument(1)]
		public int IdColumn;

		[FixedPositionCmdArgument(2)]
		public int TextColumn;

		[FixedPositionCmdArgument(3)]
		[FilePathNameValue(CheckExistance = true)]
		public string OutputFile;

		[NamedCmdArgument()]
		public int HeaderRows;
		
		[NamedCmdArgument()]
		public IEnumerable<string> SkosSources;

		[NamedCmdArgument()] 
		public string Profile;
	}

	public class FilePathNameValueAttribute : CmdArgumentAttribute
	{
		public bool CheckExistance { get; set; }
		public bool AllowRelativePaths { get; set; }
	}

	public class CmdArgumentAttribute : Attribute
	{
		public string Name { get; set; }
	}

	public class NamedCmdArgumentAttribute : CmdArgumentAttribute
	{
	}

	public class FixedPositionCmdArgumentAttribute : CmdArgumentAttribute
	{
		public FixedPositionCmdArgumentAttribute(int position)
		{
			this.Position = position;
		}

		public int Position { get; set; }

	}
}
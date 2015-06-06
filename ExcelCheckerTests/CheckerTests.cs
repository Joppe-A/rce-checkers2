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
using Trezorix.Checkers.ExcelXmlChecker;
using Trezorix.Testing.Common.System;

namespace ExcelXmlCheckerTests
{
	[TestFixture]
	public class CheckerTests
	{
		[Test]
		[Category("System")]
		public void Check()
		{
			// arrange
			var setup = new ExcelXmlCheckerSetup()
			            	{
								SkosSources = new List<string> { "Verzamelwijzen (ABR)" },
								HeaderRows = 1,
								IdColumn = 0,
								InputFile = FileTestHelpers.GetTestFilesDir() + "\\test1.xml",
								OutputFile = FileTestHelpers.GetTestDataDir() + "\\result.xml",
								TextColumn = 1
			            	};

			var checker = new Checker(setup);

			// act
			var result = checker.Check();
			
			// assert
			Assert.IsTrue(result.RowMatches.Any());
			Assert.IsTrue(result.RowMatches.Any(rm => rm.Hits.Any(h => h.Value == "GRONDMONSTER")));
		}
	}
}

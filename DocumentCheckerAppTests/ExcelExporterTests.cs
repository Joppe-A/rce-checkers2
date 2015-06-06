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
using System.Text;
using System.Xml.Linq;
using NUnit.Framework;
using Trezorix.Checkers.DocumentCheckerApp.Helpers;
using Trezorix.Common.Xml;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.Checkers.DocumentCheckerApp;
using Trezorix.Checkers.DocumentCheckerApp.Export;

namespace DocumentCheckerAppTests
{
	[TestFixture]
	public class ExcelExporterTests
	{
		[Test]
		public void ExcelExport_should_construct_workbook()
		{
			// arrange
			var uniques = new string[] { "SkosSource1", "SkosSource2" };
			
			var model = new ExportModel(uniques);

			var review = new ReviewResult()
			             	{
			             		new ConceptResult() { Id = "1", Literal = "Concept1", SkosSourceKey = "SkosSource1"},
								new ConceptResult() { Id = "3", Literal = "Concept1", SkosSourceKey = "SkosSource2"}
			             	};

			model.Documents.Add(new DocumentExportModel(review) { FileName = "test1" });
			model.Documents.Add(new DocumentExportModel(review) { FileName = "test2" });

			// act
			var result = new Xmlifier<ExportModel>(CompiledTransforms.Export).ConstructXml(model);

			// assert
			Assert.AreEqual("Workbook", result.Root.Name.LocalName);
		}

	}
}

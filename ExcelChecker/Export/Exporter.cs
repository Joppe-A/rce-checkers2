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
using System.Linq;
using Trezorix.Common.Xml;
using Trezorix.Checkers.DocumentChecker.Documents;

namespace Trezorix.Checkers.ExcelXmlChecker.Export
{
	public class Exporter
	{
		public void Export(string outputFile, AnalysisResult result)
		{
			ExportModel model = PrepareExportModel(result);

			var exporter = new Xmlifier<ExportModel>(CompiledTransforms.Export);
			
			var exportXmlDoc = exporter.ConstructXml(model);

			exportXmlDoc.Save(outputFile);
		}

		internal ExportModel PrepareExportModel(AnalysisResult result)
		{
			var model = new ExportModel();
			
			foreach(var row in result.RowMatches)
			{
				var reviewResult = new ReviewResult();
				foreach(var tokenHit in row.Hits)
				{
					var value = tokenHit.Value;
					var textMatch = result.TextMatches.Single(tm => tm.MatchedText == value);

					foreach(var conceptHit in textMatch.ConceptTerms)
					{
						if (!reviewResult.Any(cr => cr.Id == conceptHit.ConceptId))
						{
							reviewResult.Add(new ConceptResult()
							{
								Id = conceptHit.ConceptId,
								Literal = conceptHit.ConceptLabel,
								SkosSourceKey = conceptHit.SkosSourceKey
							});
						}
					}
				}

				var rowModel = new RowExportModel();
				rowModel.SetAnalysisResult(reviewResult);
				rowModel.SourceUri = row.Id;
				rowModel.CellText = row.CellText;

				model.Rows.Add(rowModel);
			}
			return model;
		}
	}
}
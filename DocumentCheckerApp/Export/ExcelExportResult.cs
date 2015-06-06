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
using System.Text;
using System.Web.Mvc;
using System.Xml;
using Trezorix.Checkers.DocumentCheckerApp.Helpers;
using Trezorix.Common.Xml;

namespace Trezorix.Checkers.DocumentCheckerApp.Export
{
	public class ExcelExportResult<T> : ActionResult
	{
		private readonly T _exportModel;
		
		public ExcelExportResult(T exportModel)
		{
			_exportModel = exportModel;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			var xdoc = new Xmlifier<T>(CompiledTransforms.Export).ConstructXml(_exportModel); //ToDo: exporter instance cachable?

			context.RequestContext.HttpContext.Response.ContentType = "application/vnd.ms-excel";
			using (var writer = new XmlTextWriter(context.RequestContext.HttpContext.Response.OutputStream, Encoding.UTF8))
			{
				xdoc.WriteTo(writer);
			}
			//// use a content result object to return the excel xml
			//var contentResult = new ContentResult() {Content = xdoc.ToString(), ContentType = "application/vnd.ms-excel"};
			//contentResult.ExecuteResult(context);
		}
	}
}
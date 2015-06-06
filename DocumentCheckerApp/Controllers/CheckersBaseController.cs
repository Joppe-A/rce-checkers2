﻿// Copyright 2013 Cultural Heritage Agency of the Netherlands, Dutch National Military Museum and Trezorix bv
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
using System.Web.Mvc;

using Trezorix.Checkers.DocumentCheckerApp.Helpers;
using Trezorix.Checkers.DocumentCheckerApp.Models.Documents;
using Trezorix.Checkers.DocumentCheckerApp.Models.Shared;

namespace Trezorix.Checkers.DocumentCheckerApp.Controllers
{
	public abstract class CheckersBaseController : Controller
	{
		[NonAction]
		protected JsonResult ExtJsSuccess(bool success = true)
		{
			return Json(new ExtJsResultModel() { success = success }, JsonRequestBehavior.AllowGet);
		}

		[NonAction]
		protected XmlResult Xml(object objectToXmlify)
		{
			return new XmlResult(objectToXmlify);
		}

		protected XmlResult Xml(object objectToXmlify, string contentType)
		{
			return new XmlResult(objectToXmlify) { ContentType = contentType };
		}
	}
}
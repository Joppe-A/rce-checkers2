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
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Mvc;

namespace Trezorix.Checkers.DocumentCheckerApp.Helpers
{
	public class JsonDataContractActionResult : ActionResult
	{
		public JsonDataContractActionResult(Object data)
		{
			this.Data = data;
		}

		public Object Data { get; private set; }

		public override void ExecuteResult(ControllerContext context)
		{
			var serializer = new DataContractJsonSerializer(this.Data.GetType());
			String output;
			using (var ms = new MemoryStream())
			{
				serializer.WriteObject(ms, this.Data);
				output = Encoding.UTF8.GetString(ms.ToArray());
			}

			context.HttpContext.Response.ContentType = "application/json";
			context.HttpContext.Response.Write(output);
		}
	}

}
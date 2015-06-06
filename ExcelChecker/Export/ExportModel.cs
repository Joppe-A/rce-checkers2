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
using System.Runtime.Serialization;

namespace Trezorix.Checkers.ExcelXmlChecker.Export
{
	[DataContract]
	public class ExportModel
	{
		private static readonly IEnumerable<string> s_headers;

		[DataMember]
		public IList<RowExportModel> Rows { get; private set; }

		[DataMember]
		public IEnumerable<string> Headers { get; private set; }

		static ExportModel()
		{
			// construct header cache based on the presence of the DataMember attribute
			s_headers = (from p in typeof (RowExportModel).GetProperties()
			            where p.IsDefined(typeof (DataMemberAttribute), true)
						orderby p.Name
			            select p.Name).ToList();
		}

		public ExportModel()
		{
			Headers = s_headers;
			Rows = new List<RowExportModel>();
		}
	}
}
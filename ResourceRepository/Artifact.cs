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
using System.IO;
using System.Runtime.Serialization;

namespace Trezorix.ResourceRepository
{
	[DataContract]
	public class Artifact
	{
		private string _artifactPath;

		[DataMember]
		public string Key { get; set; }

		[DataMember]
		public string FileName { get; set; }

		[DataMember]
		public string Error { get; set; }

		public string FilePathName
		{
			get { return Path.Combine(_artifactPath, FileName); }
		}

		[DataMember]
		public DateTime CreationDate { get; set; }

		[DataMember]
		public string ContentType { get; set; }
		
		public Artifact()
		{
			// needed for deserialization
			;
		}

		public Artifact(string key, string artifactPath)
		{
			_artifactPath = artifactPath;
			Key = key;
			FileName = Key;
			CreationDate = DateTime.Now;
		}

		internal void SetPath(string artifactPath)
		{
			_artifactPath = artifactPath;
		}
	}
}
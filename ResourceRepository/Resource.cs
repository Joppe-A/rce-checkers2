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
using Trezorix.Common.Core;

namespace Trezorix.ResourceRepository
{
	[DataContract(Name="ResourceOf{0}")]
	public class Resource<TEntity> : IEntity<string>
	{
		private string _sourceFolder;
		private string _artifactFolder;
		
		public string ArtifactFolder
		{
			get { return _artifactFolder; }
		}

		public string SourceFolder
		{
			get { return _sourceFolder; }
		}

		[DataMember]
		public Artifacts Artifacts { get; set; }

		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public TEntity Entity { get; set; }

		[DataMember]
		public string FileName { get; set; }

		public string FilePathName()
		{
			//	if (FileName == null) return _sourceFolder;
			return Path.Combine(_sourceFolder, FileName);
		}

		public long FileSize()
		{
			var f = new FileInfo(FilePathName());
			return f.Length;
		}

		[DataMember]
		public DateTime CreationDate { get; set; }

		[DataMember]
		public DateTime ModificationDate { get; set; }

		public Resource(TEntity entity, string sourceFolder) : this()
		{
			SetPath(sourceFolder);
			Entity = entity;
		}

		public Resource()
		{
			Artifacts = new Artifacts();
		}
		
		// Needed to set paths after deserialization
		internal void SetPath(string sourceFolder)
		{
			_sourceFolder = sourceFolder;
			_artifactFolder = Path.Combine(sourceFolder, "artifacts");
			
			// update converted folders
			foreach (var artifact in Artifacts)
			{
				artifact.SetPath(_artifactFolder);
			}
		}

		public Artifact AddArtifact(string key)
		{
			var result = new Artifact(key, ArtifactFolder);
			Artifacts.Add(result);
			return result;
		}
	}
}
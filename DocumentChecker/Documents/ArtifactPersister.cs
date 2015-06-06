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
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Trezorix.Common.IO;
using Trezorix.Common.Meta;

namespace Trezorix.Checkers.DocumentChecker.Documents
{
	public interface IPersistArtifacts<TArtifact>
	{
		TArtifact Revive(string filePathName);
		void Persist(string filePathName, TArtifact artifact);
	}

	public class ArtifactPersister<TArtifact> : IPersistArtifacts<TArtifact>
	{
		public void Persist(string filePathName, TArtifact artifact)
		{
			var settings = new XmlWriterSettings()
			               {
			               		Indent = true
			               };
			Debug.WriteLine("Writing" + filePathName);
			
			RetryingFileOperation.Attempt(() =>
			                            {
			                                using (var fs = new FileStream(filePathName, FileMode.Create, FileAccess.Write, FileShare.None))
											using (var xmlWriter = XmlWriter.Create(fs, settings))
											{
												var xs = new DataContractSerializer(typeof(TArtifact));
												xs.WriteObject(xmlWriter, artifact);
												xmlWriter.Flush();
												xmlWriter.Close();
												fs.Flush(true);
												fs.Close();
											}
			                            });
			
					
			Debug.WriteLine("Writing completed " + filePathName);
		}

		public TArtifact Revive(string filePathName)
		{
			Debug.WriteLine("Reading " + filePathName);
			return RetryingFileOperation.Attempt(() =>
			                                    	{
														using (var fs = new FileStream(filePathName, FileMode.Open, FileAccess.Read, FileShare.Read))
														{
															var xs = new DataContractSerializer(typeof(TArtifact));
															return (TArtifact)xs.ReadObject(fs);
														}
			                                    	});
		}
	}
}
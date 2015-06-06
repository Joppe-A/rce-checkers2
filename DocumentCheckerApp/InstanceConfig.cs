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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Trezorix.Checkers.DocumentCheckerApp
{
	public class InstanceConfig
	{
		[XmlElement]
		public string DocumentRepositoryPath = @"default\Documents";

		[XmlElement]
		public string SolrIndexUrl = @"http://localhost:8080/checker2solr";

		[XmlElement]
		public string JobRepositoryPath = @"default\Jobs";

		[XmlElement]
		public string ProfileRepositoryPath = @"default\Profiles";

		[XmlElement]
		public string Profile = "default";

		[XmlElement]
		public int DaysStoredBeforeDocumentRetention = 2;

		[XmlElement]
		public bool DeleteResultlessDocumentsAtStartup = false;

		public static InstanceConfig Current { get; set; }

		[XmlIgnore]
		public string FilePathName { get; set; }

		internal static InstanceConfig Load(string filePathName)
		{
			if (filePathName == null) throw new ArgumentNullException("filePathName");

			if (!Path.IsPathRooted(filePathName))
			{
				throw new ArgumentException("Need a fully qualified (rooted) path and file name.");
			}

			InstanceConfig config;
			using(var fileStream = new FileStream(filePathName, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var configReader = XmlReader.Create(fileStream))
			{
				var deserializer = new XmlSerializer(typeof(InstanceConfig));
				config = (InstanceConfig) deserializer.Deserialize(configReader);

				configReader.Close();
				fileStream.Close();
			}

			config.FilePathName = filePathName;
			
			config.NormalizePaths();

			return config;
		}

		private void NormalizePaths()
		{
			// ToDo: If we save the file now we will change the values to absolute paths -> Do the normalization only logically, persist inner field.
			var configFilePath = Path.GetDirectoryName(FilePathName);

			if (string.IsNullOrEmpty(DocumentRepositoryPath))
			{
				throw new BadConfigurationValueException("DocumentRepositoryPath", "Configuration value missing.");
			}
			if (!Path.IsPathRooted(DocumentRepositoryPath))
			{
				DocumentRepositoryPath = Path.GetFullPath(Path.Combine(configFilePath, DocumentRepositoryPath));
			}
			
			if (string.IsNullOrEmpty(ProfileRepositoryPath))
			{
				throw new BadConfigurationValueException("ProfileRepositoryPath", "Configuration value missing.");
			}
			if (!Path.IsPathRooted(ProfileRepositoryPath))
			{
				ProfileRepositoryPath = Path.GetFullPath(Path.Combine(configFilePath, ProfileRepositoryPath));
			}
			
			if (string.IsNullOrEmpty(JobRepositoryPath))
			{
				throw new BadConfigurationValueException("JobRepositoryPath", "Configuration value missing.");
			}
			if (!Path.IsPathRooted(JobRepositoryPath))
			{
				JobRepositoryPath = Path.GetFullPath(Path.Combine(configFilePath, JobRepositoryPath));
			}

		}

		public void Save(string filePathName)
		{
			var xmlWriterSettings = new XmlWriterSettings
			                        	{
			                        		Indent = true,
										};

			using (var fileStream = new FileStream(filePathName, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var writer = XmlWriter.Create(fileStream, xmlWriterSettings))
			{
				var serializer = new XmlSerializer(typeof(InstanceConfig));
				serializer.Serialize(writer, this);
			};
		}
	}

	public class BadConfigurationValueException : ConfigurationException
	{
		public BadConfigurationValueException(string valueName, string problemDescription)
			: base(string.Format("The value {0} has an incorrect value. Problem description: {1}", valueName, problemDescription))
		{

		}
	}

	public class ConfigurationException : Exception
	{
		public ConfigurationException()
		{

		}

		public ConfigurationException(string message)
			: base(message)
		{

		}
	}
}
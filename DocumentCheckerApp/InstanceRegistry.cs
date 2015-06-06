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
	/* * 
	
	 * Sample Xml:
	 
<?xml version="1.0" encoding="utf-8"?>
<InstanceRegistry xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Instance Name="localhost/" Config="default.cfg" />
  <Instance Name="somechecker/" Config="default.cfg" />
  <Instance Name="localhost/DocumentCheckerApp" Config="default.cfg" />
  <Instance Name="localhost/CheckerTest" Config="test.cfg" />
  <Instance Name="DocumentCheckerApp(3)/" Config="test.cfg" />
</InstanceRegistry>

	 * */

	[XmlRoot("InstanceRegistry")]
	public class InstanceRegistry : List<Instance>
	{
		[XmlIgnore]
		private string FilePathName { get; set; }

		public static InstanceRegistry Load(string filePathName)
		{
			InstanceRegistry instanceRegistry;
			using (var fileStream = new FileStream(filePathName, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var configReader = XmlReader.Create(fileStream))
			{
				var deserializer = new XmlSerializer(typeof(InstanceRegistry));
				instanceRegistry = (InstanceRegistry)deserializer.Deserialize(configReader);
				configReader.Close();
				fileStream.Close();
			}

			instanceRegistry.FilePathName = filePathName;
			return instanceRegistry;
		}

		public string ResolveInstanceConfigFilePathName(string applicationName)
		{
			var currentInstance =
				this.SingleOrDefault(
					i => string.Equals(i.Name, applicationName, StringComparison.InvariantCultureIgnoreCase));

			if (currentInstance == null)
			{
				throw new ConfigurationException("Can't resolve instance configuration for application " + applicationName);
			}

			var instanceConfigFilePathName = currentInstance.Config;
			if (!Path.IsPathRooted(instanceConfigFilePathName))
			{
				instanceConfigFilePathName = Path.Combine(Path.GetDirectoryName(FilePathName), instanceConfigFilePathName);
			}

			return instanceConfigFilePathName;
		}

		public void Add(string instanceName, string configFilePathName)
		{
			var instance = new Instance()
			               	{
			               		Name = instanceName,
			               		Config = configFilePathName
			               	};
			this.Add(instance);
		}

		public void Save()
		{
			var xmlWriterSettings = new XmlWriterSettings
			{
				Indent = true,
			};

			using (var fileStream = new FileStream(FilePathName, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var writer = XmlWriter.Create(fileStream, xmlWriterSettings))
			{
				var serializer = new XmlSerializer(typeof(InstanceRegistry));
				serializer.Serialize(writer, this);
				writer.Flush();
				writer.Close();
				fileStream.Close();
			}
		}
	}

	[XmlRoot("Instance")]
	public class Instance
	{
		[XmlAttribute] 
		public string Name;

		[XmlAttribute]
		public string Config; 
	}
}
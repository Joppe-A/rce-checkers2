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
using System.IO;
using System.Web.Hosting;
using System.Xml.Serialization;
using Trezorix.Checkers.DocumentCheckerApp.Properties;

namespace Trezorix.Checkers.DocumentCheckerApp
{
	public static class SystemConfig
	{
		// system settings (values independant of app instances)
		public static readonly string JavaVMExecutableFilePathName;
		public static readonly string TikaJarFilePathName;
		public static readonly string ConfigRoot;
		
		static SystemConfig()
		{
			JavaVMExecutableFilePathName = Settings.Default.JavaVMExecutableFilePathName;
			TikaJarFilePathName = Settings.Default.TikaJarFilePathName;
			
			var configRoot = Settings.Default.ConfigRoot;
			if (string.IsNullOrEmpty(configRoot))
			{
				throw new BadConfigurationValueException("ConfigRoot", "Configuration value missing.");
			}

			if (!Path.IsPathRooted(configRoot))
			{
				configRoot = Path.GetFullPath(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, configRoot));
			}
			ConfigRoot = configRoot;
		}
	}

	
}
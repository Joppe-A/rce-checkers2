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
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using Trezorix.Checkers.ManagerApp.Properties;

namespace Trezorix.Checkers.ManagerApp
{
	public static class ManagerAppConfig
	{
		public static readonly string SolrIndexUrl;
		public static readonly string SkosSourceRepositoryPath;
		public static readonly string DictionaryConnectionString;
		public static readonly string ProfileRepositoryPath;
		
		static ManagerAppConfig()
		{

			SkosSourceRepositoryPath = Settings.Default.SkosSourceRepositoryPath;
			if (!Path.IsPathRooted(SkosSourceRepositoryPath))
			{
				SkosSourceRepositoryPath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, SkosSourceRepositoryPath);
			}

			ProfileRepositoryPath = Settings.Default.ProfileRepositoryPath;
			if (!Path.IsPathRooted(ProfileRepositoryPath))
			{
				ProfileRepositoryPath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, ProfileRepositoryPath);
			}

			SolrIndexUrl = Settings.Default.SolrIndexUrl;

			// ToDo: Properly externalize this
			// null values will disable Treazure termenricher
			var cmNode = ConfigurationManager.ConnectionStrings[Settings.Default.DictionaryConnectionString];
			DictionaryConnectionString = cmNode != null ? cmNode.ConnectionString : null;

		}

		public class BadConfigurationValueException : Exception
		{
			public BadConfigurationValueException(string valueName, string problemDescription) 
				: base(string.Format("The value {0} has an incorrect value. Problem description: {1}", valueName, problemDescription))
			{
				
			}
		}
	}
}
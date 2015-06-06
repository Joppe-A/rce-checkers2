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
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Trezorix.Checkers.DocumentChecker.Documents;

namespace Trezorix.Checkers.DocumentCheckerApp
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Document", action = "Home", id = UrlParameter.Optional } // Parameter defaults
			);

		}
		
		

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);

			//var config = new Config("default");
			//config.Save();
			
			const string instances_cfg = "instances.cfg";
			string instancesFile = Path.Combine(SystemConfig.ConfigRoot, instances_cfg);

			var registry = InstanceRegistry.Load(instancesFile);

			string applicationName = HostingEnvironment.SiteName + HostingEnvironment.ApplicationVirtualPath;
			
			var configFile = registry.ResolveInstanceConfigFilePathName(applicationName);

			InstanceConfig.Current = InstanceConfig.Load(configFile);

			var docRepo = new DocumentRepository(InstanceConfig.Current.DocumentRepositoryPath);
			DocumentManager.CleanUpAndRetain(docRepo);

		}

		
	}
}
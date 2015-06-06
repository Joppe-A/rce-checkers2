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
using System.Net;
using System.Web.Mvc;

using Trezorix.Checkers.DocumentChecker.Jobs;
using Trezorix.Checkers.DocumentCheckerApp.Models.Jobs;

namespace Trezorix.Checkers.DocumentCheckerApp.Controllers
{
	public class JobController : CheckersBaseController
	{
		private readonly IJobRepository _jobRepository;
		
		public JobController()
		{
			_jobRepository = new JobRepository(InstanceConfig.Current.JobRepositoryPath);
		}

		public JobController(IJobRepository jobRepository)
		{
			_jobRepository = jobRepository;
		}

		public ActionResult Index()
		{
			var jobList = _jobRepository.All().Select(r => new JobExtJsModel(r.Entity));

			return Json(jobList, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Add(string label, IEnumerable<string> skosSourceSelection)
		{
			if (string.IsNullOrEmpty(label))
			{
				return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest, "Label missing, can't create job.");
			}

			if (_jobRepository.GetByLabel(label) != null)
			{
				return new HttpStatusCodeResult((int) HttpStatusCode.BadRequest, "Label already in use, choose an unique name.");
			}

			// ToDo: Check selection against profile content?

			var newJob = new Job()
						 {
							Label = label, 
							SkosSourceSelection = skosSourceSelection, 
							ProfileKey = ActiveProfile.Instance().Key
						 };
			
			var newJobResource = _jobRepository.Create(newJob);

			_jobRepository.Add(newJobResource);

			return ExtJsSuccess();
		}

		public ActionResult SkosSources()
		{
			var skosSources = from binding in ActiveProfile.Instance().SkosSourceBindings
							  select new { key = binding.Key, label = binding.Label };

			return Json(skosSources, JsonRequestBehavior.AllowGet);
		}
		
	}
}

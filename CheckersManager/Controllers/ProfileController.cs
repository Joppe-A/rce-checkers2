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
using System.Web;
using System.Web.Mvc;

using Trezorix.Checkers.DocumentChecker.Profiles;
using Trezorix.Checkers.ManagerApp.Models.Profile;
using Trezorix.Checkers.ManagerApp.Models.Shared;
using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.ManagerApp.Controllers
{
	public class ProfileController : Controller
	{
		private readonly IProfileRepository _profileRepository;
		
		public ProfileController()
			: this(new ProfileRepository(ManagerAppConfig.ProfileRepositoryPath))
		{
		}

		public ProfileController(IProfileRepository profileRepository)
		{
			_profileRepository = profileRepository;
			
		}

		public ActionResult Index()
		{
			ViewData.Model = _profileRepository.All();

			return View();
		}

		public ActionResult Create()
		{
			ViewData.Model = new ProfileEditModel(new Profile(), BindableSkosSourcesList.SkosSourceBindings);

			return View("Edit");
		}

		public ActionResult Edit(string id)
		{
			Resource<Profile> profileDoc = _profileRepository.Get(id);

			if (profileDoc == null) throw new HttpException(404, "No profile with Id: " + id);

			ViewData.Model = new ProfileEditModel(profileDoc.Id, profileDoc.Entity, BindableSkosSourcesList.SkosSourceBindings);

			return View();
		}

		

		[HttpPost]
		public ActionResult Edit(ProfileEditModel profile)
		{
			Resource<Profile> editProfile;
			if (profile.Id == null)
			{
				editProfile = _profileRepository.Create(new Profile());
			}
			else
			{
				editProfile = _profileRepository.Get(profile.Id);

				if (editProfile == null) throw new HttpException(404, "No profile exists carrying id: " + profile.Id);
			}

			if (!ModelState.IsValid)
			{
				ViewData.Model = profile;
				return View("Edit");
			}

			profile.Update(editProfile.Entity);

			_profileRepository.Update(editProfile);

			return RedirectToAction("Index");
		}

		[ActionName("Delete")]
		[HttpGet]
		public ActionResult DeleteConfirm(string id)
		{
			const string controllerName = "Profile";
			var model = new DeleteConfirmViewModel()
			{
				Id = id,
				EntityName = "profile",
				CancelLink = new ActionLinkModel()
				{
					Action = "Index",
					Controller = controllerName
				},
				DeletePostLink = new ActionLinkModel()
				{
					Action = "Delete",
					Controller = controllerName
				}
			};

			return View("DeleteConfirm", model);
		}

		[HttpPost]
		public ActionResult Delete(string id)
		{
			_profileRepository.Delete(id);

			TempData["OperationResult"] = string.Format("Profile {0} deleted successfully.", id);
			return RedirectToAction("Index");
		}
	}


}

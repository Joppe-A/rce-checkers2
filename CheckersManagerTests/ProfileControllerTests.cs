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
using System.Web;
using System.Web.Mvc;

using Moq;
using MvcContrib.TestHelper;
using NUnit.Framework;
using Trezorix.Checkers.DocumentChecker.Profiles;
using Trezorix.ResourceRepository;
using Trezorix.Checkers.ManagerApp.Controllers;
using Trezorix.Checkers.ManagerApp.Models.Profile;

namespace DocumentCheckerAppTests
{
	[TestFixture]
	public class ProfileControllerTests
	{
		private const string PROFILE_ID = "theProfileId";
		private const string SKOS_SOURCE = "mySkos";

		private Mock<IProfileRepository> _mockedRepository;
		
		private ProfileController _profileController;

		[SetUp]
		public void Setup()
		{
			_mockedRepository = new Mock<IProfileRepository>();
			_profileController = new ProfileController(_mockedRepository.Object);
		}

		[Test]
		public void Edit_post_with_a_null_id_should_create_new_profile()
		{
			// arrange
			_mockedRepository.Setup(r => r.Create(It.IsAny<Profile>()))
												  .Returns(new Resource<Profile>(new Profile(), "somepath"))
												  .Verifiable("No create called on repository");

			_mockedRepository
				.Setup(r => r.Update(It.IsAny<Resource<Profile>>()))
				.Verifiable("the new profile wasn't saved");

			var edit = new ProfileEditModel() { Key = "a profile key", Id = null };

			// act
			var result = _profileController.Edit(edit);

			// assert
			_mockedRepository.VerifyAll();
		}

		[Test]
		public void Edit_post_should_update_profile()
		{
			// arrange
			const string theId = "the_id";

			var theProfile = new Resource<Profile>(new Profile(), "somepath");

			_mockedRepository
				.Setup(r => r.Get(It.Is<string>(p => p == theId)))
				.Returns(theProfile)
				.Verifiable("No attempt was made to get the profile from the repository");
			
			_mockedRepository
				.Setup(r => r.Update(It.Is<Resource<Profile>>(p => p == theProfile)))
				.Verifiable("no save call was made to the repository for the updated profile");

			var edit = new ProfileEditModel() { Key = "a profile key", Id = theId };

			// act
			var result = _profileController.Edit(edit);

			// assert
			_mockedRepository.VerifyAll();
		}

		[Test]
		[ExpectedException(typeof(HttpException))]
		public void Edit_post_non_existing_profile_should_throw()
		{
			// arrange
			_mockedRepository
				.Setup(r => r.Get(It.IsAny<string>()))
				.Returns((Resource<Profile>) null);

			var edit = new ProfileEditModel() { Key = "a profile key", Id = "non existing id" };

			// act
			var result = _profileController.Edit(edit);

			// assert
			
		}

		[Test]
		public void Edit_post_with_modelerrors_should_return_view()
		{
			// arrange
			var edit = new ProfileEditModel() { };

			_profileController.ModelState.AddModelError("Key", "Missing name");
			// act
			var result = _profileController.Edit(edit);

			// assert
			result.AssertResultIs<ViewResult>().ForView("Edit");
		}

		[Test]
		public void Edit_post_valid_update_should_update_profile()
		{
			// arrange
			var profile = new Profile()
			              { 
							  Key = "OrgProfileName",
						  
			              };

			_mockedRepository
				.Setup(r => r.Get(It.IsAny<string>()))
				.Returns(new Resource<Profile>(profile, "somepath"));

			const string theKey = "new key";
			
			var edit = new ProfileEditModel(profile, new List<SkosSourceBinding>()
			                                         	{
			                                         		new SkosSourceBinding() { Key = "skos1" },
															new SkosSourceBinding() { Key = "skos2" },
															new SkosSourceBinding() { Key = "skos3" },
			                                         	});
			edit.Id = "a-ID";
			edit.Key = theKey;
			edit.Bindings = new List<BindingEditModel>()
			                    {
			                        new BindingEditModel() { Enabled = true, Key = "skos1"},
			                        new BindingEditModel() { Enabled = true, Key = "skos2"},
			                        new BindingEditModel() { Enabled = false, Key = "skos3"}
			                    };
			
			// act
			var result = _profileController.Edit(edit);

			// assert
			Assert.AreEqual(theKey, profile.Key, "Key change didn't get through");
			Assert.AreEqual(2, profile.SkosSourceBindings.Count(), "Should have 2 skossources");
			Assert.IsTrue(profile.SkosSourceBindings.Any(b => b.Key == "skos1"), "Enabling skos source 1 didn't get through");
			Assert.IsTrue(profile.SkosSourceBindings.Any(b => b.Key == "skos2"), "Enabling skos source 2 didn't get through");
		}

	}
}

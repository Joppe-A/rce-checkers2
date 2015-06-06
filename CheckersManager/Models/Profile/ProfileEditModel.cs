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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Trezorix.Checkers.DocumentChecker.Profiles;

namespace Trezorix.Checkers.ManagerApp.Models.Profile
{
	public class ProfileEditModel
	{
		public DocumentChecker.Profiles.Profile Profile { get; private set; }

		public string Id { get; set; }

		[Required]
		public string Key { get; set; }

		public string Language { get; set; }

		public IList<BindingEditModel> Bindings { get; set; }

		public ProfileEditModel()
		{
			Bindings = new List<BindingEditModel>();
		}

		public ProfileEditModel(string profileId, DocumentChecker.Profiles.Profile profile, IEnumerable<SkosSourceBinding> repositorySources) 
			: this(profile, repositorySources)
		{
			Id = profileId;
		}

		public ProfileEditModel(DocumentChecker.Profiles.Profile profile, IEnumerable<SkosSourceBinding> repositorySources)
		{
			Profile = profile;
			Key = profile.Key;

			Language = profile.Language;

			CreateBindEditList(profile, repositorySources);
		}

		private void CreateBindEditList(DocumentChecker.Profiles.Profile profile, IEnumerable<SkosSourceBinding> repositorySources)
		{
			Bindings = repositorySources.Select(s => new BindingEditModel() { Key = s.Key, Label = s.Label }).ToList();
			
			foreach (var binding in Bindings)
			{
				var bound = profile.SkosSourceBindings.SingleOrDefault(b => binding.Key == b.Key);
				if (bound != null )
				{
					binding.Enabled = true;
					binding.Label = bound.Label;
				}
			}
		}

		public void Update(DocumentChecker.Profiles.Profile profile)
		{
			profile.Key = Key;

			profile.Language = Language;

			profile.SkosSourceBindings = Bindings.Where(b => b.Enabled)
													.Select(s => new SkosSourceBinding()
				             										{
				             											Key = s.Key, 
																		Label = s.Label,
																	})
													.ToList();
		}

	}
}
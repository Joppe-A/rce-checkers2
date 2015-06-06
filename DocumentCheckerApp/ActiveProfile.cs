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
using Trezorix.Common.Meta;
using Trezorix.Checkers.DocumentChecker.Profiles;

namespace Trezorix.Checkers.DocumentCheckerApp
{
	public static class ActiveProfile
	{
		private static Profile s_instance;
		private static readonly object s_locker = new object();

		private static Profile GetConfiguredProfile()
		{
			var profileRepository = new ProfileRepository(InstanceConfig.Current.ProfileRepositoryPath);

			var profileKey = InstanceConfig.Current.Profile;

			var profile = profileRepository.GetByKey(profileKey);
			if (profile == null)
			{
				throw new BadConfigurationValueException("Profile", "The configured profile doesn't exist. Value: " + profileKey);
			}

			return profile;
		}

		[TestingSeam]
		public static void SetInstance(Profile profile)
		{
			lock(s_locker)
			{
				s_instance = profile;
			}
		}

		public static Profile Instance()
		{
			if (s_instance == null)
			{
				lock (s_locker)
				{
					if (s_instance == null)
					{
						s_instance = GetConfiguredProfile();
					}
				}
			}
			return s_instance;
		}
	}
}
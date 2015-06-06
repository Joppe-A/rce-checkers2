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
using System.Linq;
using System.Text;

using Moq;

using NUnit.Framework;

using Trezorix.Checkers.DocumentChecker.Jobs;
using Trezorix.Checkers.DocumentChecker.Profiles;
using Trezorix.Checkers.DocumentCheckerApp;
using Trezorix.Checkers.DocumentCheckerApp.Controllers;
using Trezorix.ResourceRepository;

namespace DocumentCheckerAppTests
{
	[TestFixture]
	public class JobControllerTests
	{
		[Test]
		public void Add_should_save_a_new_job()
		{
			// arrange
			var activeProfile = new Profile();
			ActiveProfile.SetInstance(activeProfile);

			var mockRepository = new Mock<IJobRepository>();
			var controller = new JobController(mockRepository.Object);

			// act
			controller.Add("my job", new List<string> { "skossource 1", "skossource 2" });

			// assert
			mockRepository.Verify(c => c.Add(It.IsAny<Resource<Job>>()), Times.Once());
		}



	}
}

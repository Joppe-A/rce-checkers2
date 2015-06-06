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
using Trezorix.Checkers.DocumentChecker.SkosSources;
using Trezorix.Checkers.ManagerApp.Controllers;
using Trezorix.ResourceRepository;

namespace Trezorix.Checkers.CheckersManagerTests
{
	[TestFixture]
	public class RnaToolsetImportControllerTests
	{
		private RnaToolsetImportController _rnaToolsetImportController;
		private Mock<ISkosSourceRepository> _fakeRepository;

		[SetUp]
		public void Setup()
		{
			_fakeRepository = new Mock<ISkosSourceRepository>();
			_rnaToolsetImportController = new RnaToolsetImportController(_fakeRepository.Object, null);
		}

		[Test]
		public void TestToolsetImport()
		{
			Assert.Inconclusive();

			// arrange
			SetupRepositoryToCreateDoc();

			// act

			// http://rce.rnatoolset.net/api/getNodeList.aspx?rna_api_key=e22085bb3e1b4b5f8b49e6ddf015a1cc&skosOnly=true&rsid=39
			var result = _rnaToolsetImportController.ImportAllStructures("http://test.api.rnatoolset.net/",
																			   "e22085bb3e1b4b5f8b49e6ddf015a1cc");

			// assert
			Assert.Inconclusive();
		}

		private void SetupRepositoryToCreateDoc()
		{
			var newDoc = new Resource<SkosSource>(new SkosSource() { Key = "some_key" }, "some_source") { FileName = "somefile.txt" };
			_fakeRepository.Setup(r => r.Create(It.IsAny<SkosSource>(), It.IsAny<string>())).Returns(() => newDoc);
		}
	}
}

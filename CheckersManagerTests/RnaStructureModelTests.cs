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
using NUnit.Framework;
using Trezorix.Checkers.ManagerApp.Models.RnaToolsetImport;

namespace Trezorix.Checkers.CheckersManagerTests
{
	[TestFixture]
	public class RnaStructureModelTests
	{
		private RnaStructureModel _structureModel;

		[SetUp]
		public void Setup()
		{
			_structureModel = new RnaStructureModel
			{
				Label = "the structure",
				Uri = "http://5"
			};
		}

		[Test]
		public void CreateKey_tries_label()
		{
			// arrange
		
			// act
			var result = _structureModel.CreateKey("toolset url", (key) => true);
			
			// assert
			Assert.AreEqual("the structure", result);
		}

		[Test]
		public void CreateKey_with_label_unavailable_tries_adding_toolseturl_plus_id()
		{
			// arrange

			// act
			var result = _structureModel.CreateKey("toolset url", (key) => key != "the structure");

			// assert
			Assert.AreEqual("the structure (toolset url/structure/5)", result);
		}

		[Test]
		public void CreateKey_with_label_and_toolseturl_plus_id_unavailable_adds_sequence_number()
		{
			// arrange

			// act
			var result = _structureModel.CreateKey("toolset url", (key) => key != "the structure" && key != "the structure (toolset url/structure/5)");

			// assert
			Assert.AreEqual("the structure (toolset url/structure/5) (1)", result);
		}

		[Test]
		public void CreateKey_with_label_and_toolseturl_plus_id_unavailable_finds_and_adds_an_available_sequence_number()
		{
			// arrange

			// act
			var result = _structureModel.CreateKey("toolset url", (key) => key != "the structure" && key != "the structure (toolset url/structure/5)" && key != "the structure (toolset url/structure/5) (1)");

			// assert
			Assert.AreEqual("the structure (toolset url/structure/5) (2)", result);
		}
	}
}

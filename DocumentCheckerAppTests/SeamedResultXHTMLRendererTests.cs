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
using System.Xml.Linq;

using NUnit.Framework;
using Trezorix.Checkers.DocumentChecker.Processing;
using Trezorix.Checkers.DocumentCheckerApp.Helpers;

namespace DocumentCheckerAppTests
{
	[TestFixture]
	public class SeamedResultXHTMLRendererTests : ResultXHTMLRenderer
	{
		public SeamedResultXHTMLRendererTests()
			: base(new XElement("some_element"), null)
		{
			
		}

		[Test]
		public void StripOffTextSelector_should_peel_off_text_node_selector()
		{
			// arrange
			
			// act
			var result = XTextNodeLocation.CreateFromFragmentLocator("/*[1]/*[1]/text()[23]");

			// assert
			Assert.AreEqual("/*[1]/*[1]", result.ElementSelector);
			Assert.AreEqual(22, result.TextNodeIndex);
		}

	}
}
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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;

using Trezorix.Checkers.DocumentChecker.Processing.Fragmenters;
using Trezorix.Testing.Common;
using Trezorix.Testing.Common.System;

namespace DocumentCheckerTests
{
	[TestFixture]
	public class XmlFragmenterTests
	{
		private static XmlFragmenter SetupXmlFragmenter(string frag)
		{
			var frags = string.Format("<frags>{0}</frags>", frag);

			var xmlContent = StreamHelpers.StringToStream(frags);
			
			return new XmlFragmenter(xmlContent);
		}

		[Test]
		public void Fragments_given_one_element_should_return_one_item()
		{
			// arrange
			const string frag = "<p>hi</p>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			Assert.AreEqual(1, result.Count());
		}

		[Test]
		public void Fragments_given_one_element_should_return_inner_value()
		{
			// arrange
			const string frag = "<p>inner value</p>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultEntry = result.Single();
			Assert.AreEqual("inner value", resultEntry.Value);
		}

		[Test]
		public void Fragments_given_one_p_element_should_return_locator_p()
		{
			// arrange
			const string frag = "<p>inner value</p>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultEntry = result.Single();
			Assert.AreEqual("*[1]/text()[1]", resultEntry.Locator);
		}

		[Test]
		public void Fragments_given_one_element_should_not_fragment_empty_elements()
		{
			// arrange
			const string frag = "<p></p>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			Assert.IsTrue(result.Count() == 0, "Expected no fragment");
		}

		[Test]
		public void Fragments_given_an_elementless_text_should_return_the_text()
		{
			// arrange
			const string frag = "some non xml text";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultList = result.ToList();
			Assert.AreEqual(1, resultList.Count(), "Not the expected amount of results.");
			Assert.AreEqual("some non xml text", resultList.First().Value);
		}

		[Test]
		public void Fragments_given_two_elements_should_return_both()
		{
			// arrange
			const string frag = "<p>p1</p><p>p2</p>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultList = result.ToList();
			Assert.AreEqual(2, resultList.Count(), "Not the amount of expected results");
			
			Assert.AreEqual("p1", resultList[0].Value);
			Assert.AreEqual("p2", resultList[1].Value);
		}

		[Test]
		public void Fragments_given_two_within_one_textless_element_should_return_two()
		{
			// arrange
			const string frag = "<div><p>p1</p><p>p2</p></div>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultList = result.ToList();
			Assert.AreEqual(2, resultList.Count(), "Not the amount of expected results");

			Assert.AreEqual("p1", resultList[0].Value);
			Assert.AreEqual("p2", resultList[1].Value);
		}

		[Test]
		public void Fragments_given_two_within_one_element_with_text_should_return_four()
		{
			// arrange
			const string frag = "<div><p>p1</p>two<p>p2</p>childs</div>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultList = result.ToList();
			Assert.AreEqual(4, resultList.Count(), "Not the amount of expected results");

			Assert.AreEqual("p1", resultList[0].Value);
			Assert.AreEqual("two", resultList[1].Value);
			Assert.AreEqual("p2", resultList[2].Value);
			Assert.AreEqual("childs", resultList[3].Value);
		}

		[Test]
		public void Fragments_given_one_div_element_should_return_locator_div()
		{
			// arrange
			const string frag = "<div>inner value</div>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultEntry = result.Single();
			Assert.AreEqual("*[1]/text()[1]", resultEntry.Locator);
		}

		[Test]
		public void Fragments_given_element_in_element_should_return_locator_path()
		{
			// arrange
			const string frag = "<div><p>child value</p></div>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultEntry = result.Single();
			Assert.AreEqual("*[1]/*[1]/text()[1]", resultEntry.Locator);
		}

		[Test]
		public void Fragments_given_element_in_element_should_restore_locator_after_child()
		{
			// arrange
			const string frag = "<div><p>child value</p>parentvalue</div>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultList = result.ToList();
			Assert.AreEqual(2, resultList.Count(), "Not the expected amount of results");
			Assert.AreEqual("*[1]/*[1]/text()[1]", resultList[0].Locator);
			Assert.AreEqual("*[1]/text()[1]", resultList[1].Locator);
		}

		[Test]
		public void Fragments_given_two_elements_in_element_should_count_element_order()
		{
			// arrange
			const string frag = 
@"<div>
	<p>child value</p>
	<p>child value</p>
</div>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultList = result.ToList();
			Assert.AreEqual(2, resultList.Count(), "Not the expected amount of results");
			Assert.AreEqual("*[1]/*[1]/text()[1]", resultList[0].Locator);
			Assert.AreEqual("*[1]/*[2]/text()[1]", resultList[1].Locator);
		}

		[Test]
		public void Fragments_given_text_nodes_in_element_should_return_text_node_locators()
		{
			// arrange
			const string frag = "<div>text1<p>child value</p>text2</div>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultList = result.ToList();
			Assert.AreEqual(3, resultList.Count(), "Not the expected amount of results");

			Assert.AreEqual("*[1]/text()[1]", resultList[0].Locator);
			Assert.AreEqual("text1", resultList[0].Value);

			Assert.AreEqual("*[1]/*[1]/text()[1]", resultList[1].Locator);
			Assert.AreEqual("child value", resultList[1].Value);

			Assert.AreEqual("*[1]/text()[2]", resultList[2].Locator);
			Assert.AreEqual("text2", resultList[2].Value);
		}

		[Test]
		public void Fragments_given_empty_elements_should_return_not_return_them_but_do_include_in_counter()
		{
			// arrange
			const string frag = 
@"<div>
	text1
	<p>child value</p>
	<p></p>
	<p>third p value</p>
	text2
</div>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultList = result.ToList();
			
			Assert.AreEqual(4, resultList.Count(), "Not the expected amount of results");

			Assert.AreEqual("*[1]/text()[1]", resultList[0].Locator);
			Assert.AreEqual("\n\ttext1\n\t", resultList[0].Value);

			Assert.AreEqual("*[1]/*[1]/text()[1]", resultList[1].Locator);
			Assert.AreEqual("child value", resultList[1].Value);

			Assert.AreEqual("*[1]/*[3]/text()[1]", resultList[2].Locator);
			Assert.AreEqual("third p value", resultList[2].Value);

			Assert.AreEqual("*[1]/text()[2]", resultList[3].Locator);
			Assert.AreEqual("\n\ttext2\n", resultList[3].Value);

			
		}

		[Test]
		public void Fragments_given_some_empty_elements_should_valid_path()
		{
			// arrange
			const string frag =
@"<div />
<div />
<div>
	<div></div>
</div>
<p>child value</p>";

			XmlFragmenter xf = SetupXmlFragmenter(frag);

			// act
			IEnumerable<Fragment> result = xf.Fragments();

			// assert
			var resultList = result.ToList();

			Assert.AreEqual(1, resultList.Count(), "Not the expected amount of results");

			Assert.AreEqual("*[4]/text()[1]", resultList[0].Locator);
			Assert.AreEqual("child value", resultList[0].Value);

		}
	}
}

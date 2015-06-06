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
using System.Xml.Linq;
using System.Xml.XPath;
using Trezorix.Checkers.Analyzer;
using Trezorix.Checkers.Analyzer.Tokenizers;
using Trezorix.Checkers.DocumentChecker.Documents;

namespace Trezorix.Checkers.DocumentChecker.Processing
{
	public interface IResultXHTMLRenderer
	{
		XElement Render();
	}

	public class ResultXHTMLRenderer : IResultXHTMLRenderer
	{
		private readonly XElement _xhtml;
		private readonly AnalysisResult _result;

		public ResultXHTMLRenderer(XElement xhtml, AnalysisResult result)
		{
			if (xhtml == null) throw new ArgumentNullException("xhtml");
			_xhtml = xhtml;
			_result = result;
		}

		public XElement Render()
		{
			// Passes the XHTML from bottom to top to ensure the Xpaths are valid during the pass
			foreach (var frag in _result.FragmentTokenMatches.Reverse())
			{
				var location = XTextNodeLocation.CreateFromFragmentLocator(frag.Fragment.Locator);
				
				InjectHitSpansInFragmentElement(location, frag.TokenMatches);
			}
			return _xhtml;
		}

		private void InjectHitSpansInFragmentElement(XTextNodeLocation location, IEnumerable<Token> hits)
		{
			var elementSelector = location.ElementSelector;
			if (string.IsNullOrEmpty(elementSelector))
			{
				elementSelector = ".";
			}

			var element = _xhtml.XPathSelectElement(elementSelector);
			
			XText textNode = GetTextNodeFromElement(element, location.TextNodeIndex);
			
			foreach(var hit in hits.OrderByDescending(k => k.Position.Start))
			{
				var val = textNode.Value;

				var before = val.Substring(0, hit.Position.Start);

				int afterStartIndex = hit.Position.Start + hit.Position.Length;
				var after = val.Substring(afterStartIndex, val.Length - afterStartIndex);

				var hitVal = val.Substring(hit.Position.Start, hit.Position.Length);

				var beforeNode = new XText(before);
				XNode spans = new XText(hitVal);
				
				var matchedConceptIds = _result.TextMatches[hit.Value]
					.ConceptTerms
					.Distinct(new ConceptTermByConceptIdComparer())
					.Select(c => new { c.ConceptId, c.SkosSourceKey });
				
				foreach(var textMatchConcept in matchedConceptIds)
				{
					var spanAttributes = new object[]
									 {
										new XAttribute("class", "textmatch"),
										new XAttribute("data-concept", textMatchConcept.ConceptId),
										new XAttribute("data-skossource", textMatchConcept.SkosSourceKey)
									  };
					
					 spans = new XElement("span", new object[] { spanAttributes, spans });
				}
				
				textNode.ReplaceWith(beforeNode,
									 spans,
									 new XText(after)
					);

				textNode = beforeNode;
			}
		}

		private static XText GetTextNodeFromElement(XElement element, int textNodeIndex)
		{
			return element.Nodes().OfType<XText>().Skip(textNodeIndex).First();
		}

		protected struct XTextNodeLocation
		{
			public int TextNodeIndex { get; private set; }
			public string ElementSelector { get; private set; }

			private XTextNodeLocation(string elementSelector, int textNodeIndex) : this()
			{
				TextNodeIndex = textNodeIndex;
				ElementSelector = elementSelector;
			}
			
			public static XTextNodeLocation CreateFromFragmentLocator(string fragmentLocation)
			{
				try
				{
					int selectorStartPos = fragmentLocation.LastIndexOf("/text()[");
					int textNodeIndexStart = selectorStartPos + 8;
					int textNodeIndexEnd = fragmentLocation.LastIndexOf("]");

					string textNodeIndexValue = fragmentLocation.Substring(textNodeIndexStart, textNodeIndexEnd - textNodeIndexStart);

					int textNodeIndex = int.Parse(textNodeIndexValue) - 1;

					return new XTextNodeLocation(fragmentLocation.Substring(0, selectorStartPos), textNodeIndex);
				}
				catch (Exception ex)
				{
					throw new ApplicationException("Can't parse text node from location string: " + fragmentLocation, ex);
				}

			}
		}
	}
}

	internal class ConceptTermByConceptIdComparer : IEqualityComparer<ConceptTerm>
	{
		public bool Equals(ConceptTerm x, ConceptTerm y)
		{
			if (x == null || y == null) return false;

			return x.ConceptId == y.ConceptId;
		}

		public int GetHashCode(ConceptTerm obj)
		{
			if (obj == null || obj.ConceptId == null) return 0;
			return obj.ConceptId.GetHashCode();
		}
	}
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
using System.IO;
using System.Xml;

namespace Trezorix.Checkers.DocumentChecker.Processing.Fragmenters
{
	public class XmlFragmenter
	{
		private readonly XmlReader _reader;

		public XmlFragmenter(Stream input)
		{
			_reader = XmlReader.Create(input);
		}

		public string FragmentRoot { get; set; }

		public IEnumerable<Fragment> Fragments()
		{
			if (!_reader.Read())
			{
				yield break;
			}

			_reader.MoveToContent();

			if (FragmentRoot != null)
			{
				_reader.ReadToFollowing(FragmentRoot);
			}

			if (!_reader.Read())
			{
				yield break;
			}

			foreach (var frag in GatherElementFragments())
			{
				yield return frag;
			}
			
		}

		private IEnumerable<Fragment> GatherElementFragments()
		{
			var path = new BreadCrumbXPathBuilder();
			do
			{
				if (_reader.NodeType == XmlNodeType.Element)
				{
					// skip valueless nodes e.g. <div />
					if (!_reader.IsEmptyElement)
					{
						path.PathElement();	
					}
					else
					{
						path.PathEmptyElement();
					}
					
				}
				else if (_reader.NodeType == XmlNodeType.Text)
				{
					path.PathText();

					string value = _reader.Value;
					if (!string.IsNullOrEmpty(value))
					{
						yield return new Fragment(value, path.Location());
					}
				}
				else if (_reader.NodeType == XmlNodeType.EndElement)
				{
					if (!path.PathReturn()) yield break;
				}
				
			} while (_reader.Read());
		}
	}
}
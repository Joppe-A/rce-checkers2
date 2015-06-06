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

namespace Trezorix.Checkers.DocumentChecker.Processing.Fragmenters
{
	public class BreadCrumbXPathBuilder
	{
		private class BreadCrumb
		{
			public int TextCounter { get; private set; }
			public int Counter { get; private set; }
			public string Location { get; private set; }
			
			public void Increment()
			{
				Counter++;
			}
			
			public void TextIncrement()
			{
				TextCounter++;
			}

			public BreadCrumb(string location)
			{
				Location = location;
				TextCounter = 0;
				Counter = 1;
			}

		}

		private readonly Stack<BreadCrumb> _crumbs = new Stack<BreadCrumb>();
		
		private BreadCrumb _currentPath;

		public BreadCrumbXPathBuilder()
		{
			_currentPath = new BreadCrumb("");
		}

		public bool PathReturn()
		{
			if (_crumbs.Count == 0)
			{
				return false;
			}
			_currentPath = _crumbs.Pop();
			return true;
		}

		public string Location()
		{
			return string.Format("{0}/text()[{1}]", 
				_currentPath.Location, 
				_currentPath.TextCounter);
		}

		public void PathElement()
		{
			string newLocation;
			if (_currentPath.Location == string.Empty)
			{
				newLocation = string.Format("*[{0}]", _currentPath.Counter);
			}
			else
			{
				newLocation = string.Format("{0}/*[{1}]",
				                            _currentPath.Location,
				                            _currentPath.Counter);
			}
		
			_currentPath.Increment();
			_crumbs.Push(_currentPath);
			
			_currentPath = new BreadCrumb(newLocation);
		}

		public void PathEmptyElement()
		{
			_currentPath.Increment();
		}

		public void PathText()
		{
			_currentPath.TextIncrement();
		}

	}
}
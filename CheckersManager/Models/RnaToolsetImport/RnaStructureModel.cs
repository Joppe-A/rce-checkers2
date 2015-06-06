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

namespace Trezorix.Checkers.ManagerApp.Models.RnaToolsetImport
{
	public struct RnaStructureModel
	{
		public string Uri;
		public string Label;
		public bool Selected;

		public string CreateKey(string toolsetUrl, Func<string, bool> available)
		{
			string key = Label;
			if (!available(key))
			{
				key = String.Format("{0} ({1}/structure/{2})", Label, toolsetUrl, Uri);
				if (!available(key))
				{
					key = FindSafeKey(available, key);
				}
			}

			return key;
		}

		private static string FindSafeKey(Func<string, bool> available, string key)
		{
			int i = 0;
			string sequencialKey;

			do
			{
				i++;
				sequencialKey = MakeSequencialKey(i, key);
			} while (!available(sequencialKey));

			return sequencialKey;
		}

		private static string MakeSequencialKey(int i, string key)
		{
			return String.Format("{0} ({1})", key, i);
		}
	}
}
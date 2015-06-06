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
using System.IO;
using System.Xml.Xsl;
using Trezorix.Common.Xml.Xsl;

namespace Trezorix.Checkers.FileConverter
{
	public class TikaXHtmlStripper : IFileConverter
	{
		private static XslCompiledTransform s_transform;
		private static readonly Object s_transformLock = new object();

		private const string TIKA_HTML_STRIPPER_XSLT = "strip_html.xslt";

		/// <summary>
		///		Strips the Tika generated XHTML file of all HTML and keeps the most important content as plain text
		/// </summary>
		public void Convert(string sourceFilePathName, string targetFilePathName)
		{
			var transformer = Transformer();
			
			transformer.Transform(sourceFilePathName, targetFilePathName);
		}

		private static XslCompiledTransform Transformer()
		{
			if (s_transform == null)
			{
				var stylesheet = XslCompiler.Compile(typeof (TikaXHtmlStripper), TIKA_HTML_STRIPPER_XSLT);
				lock(s_transformLock)
				{
					if (s_transform == null)
					{
						s_transform = stylesheet;
					}
				}
			}
			return s_transform;
		}

		public string OutputContentType
		{
			get { return @"application/text"; }
		}

	}
}
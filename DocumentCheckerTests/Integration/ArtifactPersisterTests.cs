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
using System.Xml.Linq;
using NUnit.Framework;

using Trezorix.Checkers.Analyzer.Tokenizers;
using Trezorix.Checkers.DocumentChecker;
using Trezorix.Checkers.DocumentChecker.Documents;
using Trezorix.Testing.Common.System;

namespace DocumentCheckerTests.Integration
{
	[TestFixture]
	public class ArtifactSerializerTests
	{
		[Test]
		[Category("Integration")]
		public void SaveHits_should_serialize()
		{
			// arrange
			var analysisResult = new AnalysisResult();

			string filePathName = FileTestHelpers.GetTestFilesDir() + @"\testHits.xml";

			analysisResult.FragmentTokenMatches.Add(new FragmentTokenMatches() 
														{ 
															TokenMatches = new List<Token> 
															{
																Token.Create("HELLO")
															}, 
															Fragment = null
														}
			);

			var persister = new ArtifactPersister<AnalysisResult>();
			
			// act
			
			persister.Persist(filePathName, analysisResult);
			
			// assert
			XDocument result = XDocument.Load(filePathName);

			Assert.IsTrue(result.ToString().Contains("HELLO"), "Didn't find HELLO1 in XML.");
		}

		[Test]
		[Category("Integration")]
		public void Sequencial_NonAsync_persist_and_revive()
		{
			// arrange
			var testBlob = new TestBlob();

			string filePathName = FileTestHelpers.GetTestFilesDir() + @"\testBlob.bin";

			var persister = new ArtifactPersister<TestBlob>();

			// act
			persister.Persist(filePathName, testBlob);
			var result = persister.Revive(filePathName);
			
			// assert
			Assert.IsTrue(result.Blob.Length == 12000);
		}

		public class TestBlob
		{
			public byte[] Blob;

			public TestBlob()
			{
				var r = new Random();
				
				Blob = new byte[12000];
				
				r.NextBytes(Blob);
			}
		}
	}
}

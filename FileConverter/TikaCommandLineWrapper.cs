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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Trezorix.Checkers.FileConverter
{
	public class TikaCommandLineWrapper : IFileConverter
	{
		private readonly string _tikaJarFilePathName;
		private readonly string _javaVMExecutable;
		
		// ToDo: Web.config setting not used, needed for something else?
		private const string TIKA_RELATIVE_FILE_PATH_NAME = "lib\\tika-app-1.4.jar";

		public TikaCommandLineWrapper(string javaVMExecutable)
			: this(MakePathAbsolute(TIKA_RELATIVE_FILE_PATH_NAME), javaVMExecutable)
		{
		}

		private static string MakePathAbsolute(string relativePath)
		{
			return Path.Combine(Helpers.GetAppDomainPath(), relativePath);
		}

		public TikaCommandLineWrapper(string tikaJarFilePathName, string javaVMExecutable)
		{
			if (!Path.IsPathRooted(tikaJarFilePathName)) tikaJarFilePathName = MakePathAbsolute(tikaJarFilePathName);
			if (!File.Exists(tikaJarFilePathName)) throw new TikaAppJarNotFoundException(tikaJarFilePathName);
			
			if (!File.Exists(javaVMExecutable)) throw new JavaVmNotFoundException(javaVMExecutable);
			_tikaJarFilePathName = tikaJarFilePathName;
			_javaVMExecutable = javaVMExecutable;
		}

		public void Convert(string sourceFilePathName, string targetFilePathName)
		{
			if (string.IsNullOrEmpty(sourceFilePathName)) throw new ArgumentNullException("sourceFilePathName");
			if (string.IsNullOrEmpty(targetFilePathName)) throw new ArgumentNullException("targetFilePathName");

			if (!File.Exists(sourceFilePathName)) throw new FileDoesNotExistException(String.Format("Source file: {0}", sourceFilePathName));
			
			// java -jar C:\Development\Tika\apache-tika-0.8-src\tika-app\target\tika-app-0.8.jar "%%c" > "output\%%c.html"

			string jarCmd = string.Format(@"-jar ""{0}"" --xml --encoding=utf-8", _tikaJarFilePathName);

			using (var p = new Process{
												StartInfo =
												{
													UseShellExecute = false,
													RedirectStandardOutput = true,
													FileName = _javaVMExecutable,
													Arguments = jarCmd + " \"" + sourceFilePathName + "\"",
													CreateNoWindow = true,
													RedirectStandardError = true,
													StandardOutputEncoding = Encoding.UTF8,
													StandardErrorEncoding = Encoding.UTF8
												}
											})
			{
				
				using (var fileWriter = new FileStream(targetFilePathName, FileMode.Create))
				using (var errorStream = new StringWriter())
				using (var stream = new StreamWriter(fileWriter, Encoding.UTF8))
				{
					// async callback
					p.OutputDataReceived += (s, e) => stream.Write(e.Data);
					p.ErrorDataReceived += (s, e) => errorStream.Write(e.Data);

					p.Start();
					
					p.BeginOutputReadLine();
					p.BeginErrorReadLine();


					// waits for process to finish
					p.WaitForExit();
					
					string errorOut = errorStream.ToString();
					if (!string.IsNullOrEmpty(errorOut)) throw new TikaConversionException(errorOut);

					stream.Close();
					errorStream.Close();
					fileWriter.Close();
				}
				
				p.Close();
			}
		}

		public string OutputContentType
		{
			get { return @"application/xhtml+xml"; }
		}

		public class TikaConversionException : Exception
		{
			public TikaConversionException(string message) : base(message)
			{
			}
		}
		
		public class TikaAppJarNotFoundException : ApplicationException
		{
			public TikaAppJarNotFoundException(string tikaJarFilePathName)
				: base(tikaJarFilePathName)
			{
			}
		}

		public class JavaVmNotFoundException : ApplicationException
		{
			public JavaVmNotFoundException(string javaVmFilePathName)
				: base(javaVmFilePathName)
			{
			}
		}
	}

	
}

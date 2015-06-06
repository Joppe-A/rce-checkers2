using System;
using System.IO;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FileConverter
{
	public class XmlToJsonConverter : IFileConverter
	{
		public void Convert(string sourceFilePathName, string targetFilePathName)
		{
			if (string.IsNullOrEmpty(sourceFilePathName)) throw new ArgumentNullException("sourceFilePathName");
			if (string.IsNullOrEmpty(targetFilePathName)) throw new ArgumentNullException("targetFilePathName");

			if (!File.Exists(sourceFilePathName)) throw new FileDoesNotExistException(String.Format("Source file: {0}", sourceFilePathName));

			var doc = new XmlDocument();
			doc.Load(sourceFilePathName);

			using (TextWriter tw = new StreamWriter(targetFilePathName, false, Encoding.UTF8))
			{
				var xnc = new XmlNodeConverter();
				xnc.WriteJson(new JsonTextWriter(tw), doc,  new JsonSerializer());    
				tw.Close();
			}
			
		}

		public string OutputContentType
		{
			get { return @"application/json"; }
		}
	}
}

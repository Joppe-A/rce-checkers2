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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Trezorix.Common.Web;

namespace Trezorix.Checkers.Analyzer.Indexes.Solr
{
	public class SolrIndex
	{
		// member variables
		private readonly string _solrUrl = "";
		private readonly string _solrSearchUrl;
		private readonly string _solrUpdateUrl;

		// constructor
		public SolrIndex(string solrUrl)
		{
			_solrUrl = solrUrl;
			_solrSearchUrl = solrUrl + "/select/";
			_solrUpdateUrl = _solrUrl + "/update";
		}

		public void Update(XDocument aDoc)
		{
			string statusDescription = "";

			WebPost(_solrUpdateUrl, aDoc, ref statusDescription);
		}

		/*
		solr 1.4 will support multiple deletes
		<delete>
			<id>05991</id>
		  <id>06000</id>
			<query>office:Bridgewater</query>
			<query>office:Osaka</query>
		</delete>
		*/

		// <delete><id>05991</id></delete>
		public void DeleteDoc(string id)
		{
			string statusDescription = "";
			WebPost(_solrUpdateUrl, "<delete><id>" + id + "</id></delete>", ref statusDescription);
		}

		public void Clear()
		{
			string statusDescription = "";
			WebPost(_solrUpdateUrl, "<delete><query>*:*</query></delete>", ref statusDescription);
		}

		public void Commit(bool waitFlush = false, bool waitSearcher = true)
		{
			string statusDescription = "";
			WebPost(_solrUpdateUrl, String.Format("<commit waitFlush=\"{0}\" waitSearcher=\"{1}\" />", waitFlush, waitSearcher), ref statusDescription);
		}

		public void Optimize()
		{
			string statusDescription = "";
			WebPost(_solrUpdateUrl, "<optimize />", ref statusDescription);
		}

		private static HttpStatusCode WebPost(string url, XDocument doc, ref string statusDescription)
		{
			var settings = new XmlWriterSettings()
			                     	{
			                     		Encoding = Encoding.UTF8,
										OmitXmlDeclaration = true,
			                     	};
			return WebPost(url,
							(Stream request) =>
							{
								using(var xmlWriter = XmlWriter.Create(request, settings))
								{
									doc.WriteTo(xmlWriter);	
								}
							},
							ref statusDescription);
		}

		private static HttpStatusCode WebPost(string url, string data, ref string statusDescription)
		{
			// Uploads xml to solr, must be utf-8 data
			byte[] bytesToPost = Encoding.UTF8.GetBytes(data);
			var code = WebPost(url, bytesToPost, ref statusDescription);
			return code;
		}

		private static HttpStatusCode WebPost(string url, byte[] postData, ref string statusDescription)
		{
			return WebPost(url, (s) => s.Write(postData, 0, postData.Length), ref statusDescription);
		}

		private delegate void DataWriter(Stream writer);

		private static HttpStatusCode WebPost(string url, DataWriter writer, ref string statusDescription)
		{
			HttpStatusCode iCode;

			var oRequest = (HttpWebRequest)WebRequest.Create(url);
			oRequest.Method = "POST";
			oRequest.ContentType = "text/xml";

			using (var dataStream = oRequest.GetRequestStream())
			{
				writer(dataStream);

				dataStream.Close();
			}

			try
			{
				using (var oResponse = (HttpWebResponse)oRequest.GetResponse())
				{
					statusDescription = oResponse.StatusDescription;
					iCode = oResponse.StatusCode;
				}
			}
			catch (WebException ex)
			{
				var errorResponse = (HttpWebResponse)ex.Response;
				
				var tomcatResult = GetTomcatErrorResponseMessage(errorResponse);
				throw new SolrIndexerException(tomcatResult, ex);
			}
			
			return iCode;
		}

		private static WebResponse WebGet(string url, NameValueCollection parameters)
		{
			var getUrl = url + parameters.ToQueryString();
			var request = WebRequest.Create(getUrl);

			return request.GetResponse();
		}

		public XDocument SolrSelectXml(NameValueCollection searchParameters)
		{
			WebResponse response;
			try
			{
				response = WebGet(_solrSearchUrl, searchParameters);
			}
			catch (WebException ex)
			{
				string errorMessage = GetTomcatErrorResponseMessage(ex.Response);
				throw new SolrIndexerException(errorMessage, ex);
			}

			return XDocument.Load(response.GetResponseStream());
		}

		private static string GetTomcatErrorResponseMessage(WebResponse errorResponse)
		{
			if (errorResponse.ContentLength != 0)
			{
				using (var stream = errorResponse.GetResponseStream())
				{
					if (stream != null)
					{
						using (var reader = new StreamReader(stream))
						{
							string message = reader.ReadToEnd();
							return ParseApacheTomcatErrorDescription(message);
						}
					}
				}
			}
			return "No error response content received from server, examine inner exception.";
		}

		// TI: perhaps put this in a util/extension method?
		private static string ParseApacheTomcatErrorDescription(string message)
		{
			try
			{
				int startIndex = message.IndexOf("<h1>");
				int endIndex = message.IndexOf("</h1>", startIndex + 4);
				var desc = message.Substring(startIndex + 4, endIndex - startIndex - 4);
				if (!string.IsNullOrWhiteSpace(desc))
				{
					return desc;
				}
			}
			catch (Exception)
			{
				;// no uglies here, non critical code
			}
			return "Could not parse Tomcat error Description, please examine the response body: \r\n" + message;
		}

		public class SolrIndexerException : Exception
		{
			public SolrIndexerException(string message, Exception innerException)
				: base(message, innerException)
			{

			}
		}
	}
}

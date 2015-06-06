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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Data.SqlClient;
using Trezorix.Checkers.Analyzer.Indexes;

namespace Trezorix.Treazure.API
{
	/// <summary>
	/// Class containing methods to search through known words in the
	/// Trezorix Dictionary database
	/// </summary>
	public class SearchWords : ITermEnricher, IDisposable
	{
		private SqlConnection _connection;
		private bool _isDisposed;

		public SearchWords(string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
						throw new ArgumentNullException("connectionString");

			_connection = new SqlConnection(connectionString);
		}
		
		/// <summary>
		/// Get a match set of the given word in the given language
		/// </summary>
		/// <param name="term">Term you want to match</param>
		/// <param name="language">Language you want to match with. (Use ISO Country codes (US, NL etc) not db identifiers)</param>
		/// <param name="selectedDictionaryCollectionName">Contains a list of selected dictionary collection names</param>
		/// <returns></returns>
		public IEnumerable<TermEnrichment> Enrich(string term, string language, string selectedDictionaryCollectionName)
		{
			IEnumerable<TermEnrichment> lstMatches = MatchExact(term, language, selectedDictionaryCollectionName).ToList();
			Debug.WriteLineIf(lstMatches.Any(), "TermEnricher supplied " + lstMatches.Count() + " alternative words for " + term);

			return lstMatches;
		}

		/// <summary>
		/// Gets an enumerator of available domains.
		/// </summary>
		/// <remarks>
		/// The AvailableDomain object contains properties for the domain's Uri, Name and wether
		/// the domain is a default domain or not. Default domains are domains that are not specified
		/// for a certain 'type' of terms
		/// </remarks>
		/// <returns>
		/// IEnumerable of AvailableDomain objects.
		/// </returns>
		//public IEnumerable<AvailableDomain> Domains()
		//{
		//    List<AvailableDomain> lstDomains = null;
		//    lstDomains = GetAvailableDomains();
		//    return lstDomains;
		//}
		
		private IEnumerable<TermEnrichment> MatchExact(string word, string languageCode, string selectedDictionaryCollections)
		{
			var lstMatches = new List<TermEnrichment>();

			using (var cmdMatch = new SqlCommand("spMatch", _connection))
			{
				cmdMatch.CommandType = System.Data.CommandType.StoredProcedure;
				var parmWord = new SqlParameter("@Word", word);
				var parmLanguage = new SqlParameter("@LanguageCode", languageCode);
				var parmDictionaries = new SqlParameter("@Domain", selectedDictionaryCollections);
					
				cmdMatch.Parameters.Add(parmWord);
				cmdMatch.Parameters.Add(parmLanguage);
				cmdMatch.Parameters.Add(parmDictionaries);

				_connection.Open();
					
				SqlDataReader rdrResult = cmdMatch.ExecuteReader();

				while (rdrResult.Read())
				{
					long matchIdentifier = (rdrResult.IsDBNull(0)) ? -1 : rdrResult.GetInt64(0);
					string sWord = (rdrResult.IsDBNull(1)) ? null : rdrResult.GetString(1);
					string sDictionary = (rdrResult.IsDBNull(2)) ? null : rdrResult.GetString(2);
					string sDictionaryCollection = (rdrResult.IsDBNull(3)) ? null : rdrResult.GetString(3);

					TermEnrichment currentTermEnrichment = lstMatches.FirstOrDefault(
						x => x.DictionaryCollectionName == sDictionaryCollection 
							    && x.Dictionary == sDictionary 
							    && x.GroupId == matchIdentifier
						);

					if (currentTermEnrichment == null)
					{
						currentTermEnrichment = new TermEnrichment(sDictionaryCollection, sDictionary, (int)matchIdentifier);
						lstMatches.Add(currentTermEnrichment);
					}
					currentTermEnrichment.AddTerm(sWord);

				}

				_connection.Close();
			}
		
			return lstMatches;
		}

		//private List<AvailableDomain> GetAvailableDomains()
		//{
		//    List<AvailableDomain> availableDomains = null;

		//    if (!m_bIsDisposed)
		//    {
		//        CreateConnection();

		//        SqlDataReader rdrResult = null;
		//        using (SqlCommand cmdMatch = new SqlCommand("spAvailableDomains", m_Connection))
		//        {
		//            cmdMatch.CommandType = System.Data.CommandType.StoredProcedure;

		//            rdrResult = cmdMatch.ExecuteReader();


		//            if (rdrResult != null)
		//            {
		//                while (rdrResult.Read())
		//                {
		//                    string sUri = (rdrResult.IsDBNull(0)) ? null : rdrResult.GetString(0);
		//                    string sDomain = (rdrResult.IsDBNull(1)) ? null : rdrResult.GetString(1);
		//                    bool bDefault = (rdrResult.IsDBNull(2)) ? false : rdrResult.GetBoolean(2);

		//                    AvailableDomain newAvailableDomain = new AvailableDomain()
		//                    {
		//                        Uri = sUri,
		//                        Name = sDomain,
		//                        IsDefault = bDefault
		//                    };

		//                    availableDomains.Add(newAvailableDomain);
		//                }
		//            }
		//        }
		//    }

		//    return availableDomains;
		//}
		
		~SearchWords()
		{
			Dispose(false);
		}

		/// <summary>
		/// Disposes all class resources
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if ((disposing) && (!_isDisposed))
			{
				if (_connection != null)
				{
					if (_connection.State == System.Data.ConnectionState.Open)
						_connection.Close();
					_connection.Dispose();
				}
				_connection = null;
			}
			_isDisposed = true;
		}

		//public IEnumerable<TermEnrichment> Enrich(string term, string language, IEnumerable<string> domains)
		//{
		//    var enrichmentQueryResult = EnrichPrev(term, language, domains);
		//    if (enrichmentQueryResult != null)
		//    {
		//        return FilterEnrichments(enrichmentQueryResult, domains);
		//    }
		//    return null;
		//}

		//public virtual IEnumerable<TermEnrichment> FilterEnrichments(IEnumerable<TermEnrichment> enrichments, IEnumerable<string> domains)
		//{
		//    // if we have domain hits consider only those
		//    var domainWordGroup = enrichments.Where(te => te.DictionaryCollection == domains.First()); // ToDo: Only using one domain currently

		//    // if there is only one word group return it
		//    if (domainWordGroup.Count() == 1)
		//    {
		//        return domainWordGroup;
		//    }

		//    // if we have only one word group in dictionary return it
		//    var dictionaryWordGroup = enrichments.Where(te => te.DictionaryCollection == null);
		//    if (dictionaryWordGroup.Count() == 1)
		//    {
		//        return dictionaryWordGroup;
		//    }

		//    return null;
		//}
	}
}
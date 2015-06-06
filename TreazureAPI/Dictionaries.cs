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
using System.Text;
using System.Data.SqlClient;

namespace Trezorix.Treazure.API
{
	public class Dictionaries
	{
		

		private string _connectionString;

		public string ConnectionString
		{
			get { return _connectionString; }
			set { _connectionString = value; }
		}

		public Dictionaries(string connectionString)
		{
			_connectionString = connectionString;
			CreateConnection();
		}

		public IEnumerable<Dictionary> GetAllDictionaries(string dictionaryCollectionName)
		{
			return GetDictionariesInCollection(GetDictionaryCollectionId(dictionaryCollectionName));
		}

		public IEnumerable<Dictionary> GetAllDictionaries(int dictionaryCollectionName)
		{
			return GetDictionariesInCollection(dictionaryCollectionName);
		}

		public IEnumerable<DictionaryCollection> GetAllDictionaryCollections()
		{
			return GetDictionaryCollections();
		}

		public void AddWord(string word, int dictionary)
		{
			if (string.IsNullOrWhiteSpace(word))
			{
				throw new ArgumentNullException("word");
			}

			if (dictionary <= 0)
			{
				throw new ArgumentException("Dictionary must be larger then zero.", "dictionary");
			}

			using (SqlConnection connection = CreateConnection())
			using (SqlCommand cmdMatch = new SqlCommand("spAddWord", connection))
			{
				cmdMatch.CommandType = System.Data.CommandType.StoredProcedure;
				SqlParameter parmWord = new SqlParameter("@Word", word);
				SqlParameter parmDictionary = new SqlParameter("@Dictionary", dictionary);

				cmdMatch.Parameters.Add(parmWord);
				cmdMatch.Parameters.Add(parmDictionary);

				bool? result = cmdMatch.ExecuteScalar() as bool?;
				if (!(result ?? false))
				{
					throw new Exception("Insert failed, returned null or false.");
				}
			}
		}

		private int GetDictionaryCollectionId(string dictionaryCollectionName)
		{
			if (string.IsNullOrWhiteSpace(dictionaryCollectionName))
			{
				throw new ArgumentNullException("dictionaryCollectionName");
			}

			int dictionaryCollectionId = -1;

			using (SqlConnection connection = CreateConnection())
			using (var cmdDictionaryCollection = new SqlCommand("spRetrieveDomainID", connection))
			{
				cmdDictionaryCollection.CommandType = System.Data.CommandType.StoredProcedure;
				var parmDictionaryCollectionName = new SqlParameter("@DomainName", dictionaryCollectionName);

				cmdDictionaryCollection.Parameters.Add(parmDictionaryCollectionName);

				var result = cmdDictionaryCollection.ExecuteScalar() as int?;
				dictionaryCollectionId = (result.HasValue) ? result.Value : -1;
				if (result != null)
				{
					dictionaryCollectionId = int.Parse(result.ToString());
				}
			}

			return dictionaryCollectionId;
		}

		private IEnumerable<Dictionary> GetDictionariesInCollection(int dictionaryCollectionName)
		{
			List<Dictionary> lstDictionaries = null;

			if (dictionaryCollectionName > 0)
			{
				using (SqlConnection connection = CreateConnection())
				using (var cmdDictionaries = new SqlCommand("spAvailableDictionaries", connection))
				{
					cmdDictionaries.CommandType = System.Data.CommandType.StoredProcedure;
					var parmDictionaryCollectionName = new SqlParameter("@Domain", dictionaryCollectionName);
					cmdDictionaries.Parameters.Add(parmDictionaryCollectionName);

					using (SqlDataReader rdrDictionaries = cmdDictionaries.ExecuteReader())
					{
						lstDictionaries = new List<Dictionary>();
						while (rdrDictionaries.Read())
						{
							int dictionaryIdentifier = (rdrDictionaries.IsDBNull(0)) ? -1 : rdrDictionaries.GetInt32(0);
							string sDictionaryName = (rdrDictionaries.IsDBNull(1)) ? null : rdrDictionaries.GetString(1);
							string sDictionaryCollectionName = (rdrDictionaries.IsDBNull(2)) ? null : rdrDictionaries.GetString(2);
							string sLanguage = (rdrDictionaries.IsDBNull(3)) ? null : rdrDictionaries.GetString(3);

							Dictionary foundDictionary = new Dictionary()
								{
									Id = dictionaryIdentifier,
									Name = sDictionaryName,
									DictionaryCollection = sDictionaryCollectionName,
									Language = sLanguage
								};

							lstDictionaries.Add(foundDictionary);

						}
					}

				}
			}

			return lstDictionaries;
		}

		private IEnumerable<DictionaryCollection> GetDictionaryCollections()
		{
			List<DictionaryCollection> dictionaryCollections;
			using (SqlConnection connection = CreateConnection())
			using (SqlCommand cmdDictionaries = new SqlCommand("spAvailableDomains", connection))
			{
				cmdDictionaries.CommandType = System.Data.CommandType.StoredProcedure;

				using (SqlDataReader rdrDictionaries = cmdDictionaries.ExecuteReader())
				{
					dictionaryCollections = new List<DictionaryCollection>();
					while (rdrDictionaries.Read())
					{
						int dictionaryCollectionId = (rdrDictionaries.IsDBNull(0)) ? -1 : rdrDictionaries.GetInt32(0);
						string dictionaryCollectionName = (rdrDictionaries.IsDBNull(1)) ? null : rdrDictionaries.GetString(1);

						DictionaryCollection foundDictionaryCollection = new DictionaryCollection()
							{
								Id = dictionaryCollectionId,
								Name = dictionaryCollectionName
							};

						dictionaryCollections.Add(foundDictionaryCollection);
					}
				}
			}

			return dictionaryCollections;
		}

		private SqlConnection CreateConnection()
		{
			if (string.IsNullOrWhiteSpace(_connectionString))
						throw new Exception("The connectionstring is not set");
			
			SqlConnection connection = new SqlConnection(_connectionString);
			connection.Open();

			return connection;
		}

	}
}

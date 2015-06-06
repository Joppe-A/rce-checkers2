using System.Collections.Generic;
using Trezorix.Checkers.Analyzer.Indexes;

namespace Trezorix.Checkers.ManagerApp.Services
{
	public class NullEnricher : ITermEnricher
	{
		public IEnumerable<TermEnrichment> Enrich(string term, string language, string dictionaryCollectionName)
		{
			return null;
		}
	}
}
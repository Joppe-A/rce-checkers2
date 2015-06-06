using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trezorix.Treazure.API;

namespace Util
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args[0] == "enrich")
			{
				var searchWord =
					new SearchWords(@"DATA SOURCE=localhost\sqlexpress;Initial Catalog=Dictionaries;user id=dictionary;password=d1ctionary;");

				var enrichments = searchWord.Enrich(args[1], "nl", "RCE");

				foreach (var enrichment in enrichments)
				{
					Console.WriteLine("Group: " + enrichment.GroupId);
					Console.WriteLine("------------------------------------------");
					foreach (var term in enrichment.Terms)
					{
						Console.WriteLine(term);
					}
					Console.WriteLine("------------------------------------------");
				}
			}
		}
	}
}

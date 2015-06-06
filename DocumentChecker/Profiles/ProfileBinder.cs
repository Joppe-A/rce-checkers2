using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trezorix.Checkers.DocumentChecker.SkosSources;

namespace Trezorix.Checkers.DocumentChecker.Profiles
{
	public class ProfileBinder
	{
		private readonly ISkosSourceRepository _skosSourceRepository;

		public ProfileBinder(ISkosSourceRepository skosSourceRepository)
		{
			_skosSourceRepository = skosSourceRepository;
		}

		public void RefreshBindings(Profile profile)
		{
			RemoveDeadBindings(profile);
			ExpandBindingsWithNewSkosSources(profile);
		}

		private void ExpandBindingsWithNewSkosSources(Profile profile)
		{
			foreach (var binding in _skosSourceRepository.All()
				.Where(ss => !profile.SkosSourceBindings.Any(b => ss.Entity.Key == b.Key))
				.Select(b => new SkosSourceBinding()
				             	{
				             		Key = b.Entity.Key,
				             		Label = b.Entity.Label
				             	}))
			{
				profile.SkosSourceBindings.Add(binding);
			}
		}

		private void RemoveDeadBindings(Profile profile)
		{
			foreach(var binding in profile.SkosSourceBindings.ToList())
			{
				if (_skosSourceRepository.GetByKey(binding.Key) == null)
				{
					profile.SkosSourceBindings.Remove(binding);
				}
			}
		}
	}
}

using System;
using System.Linq;

namespace SHARP.Core
{
	public class PredicateConstraint<VM> : IQueryConstraint<VM>
		where VM : IViewModel
	{
		public Func<VM, bool> Predicate { get; private set; }

		public static PredicateConstraint<VM> Where(Func<VM, bool> predicate) =>
			new()
			{
				Predicate = predicate
			};

		public static PredicateConstraint<VM> WhereAll(params Func<VM, bool>[] predicates)
		{
			if (predicates == null || predicates.Length == 0)
				throw new ArgumentException("At least one predicate must be provided");

			return Where(vm => predicates.All(p => p(vm)));
		}

		public static PredicateConstraint<VM> WhereAny(params Func<VM, bool>[] predicates)
		{
			if (predicates == null || predicates.Length == 0)
				throw new ArgumentException("At least one predicate must be provided");

			return Where(vm => predicates.Any(p => p(vm)));
		}
	}
}
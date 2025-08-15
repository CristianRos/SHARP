using System;

namespace SHARP.Core
{
	public class CoordinatorConstraint<VM> : IQueryConstraint<VM>
		where VM : IViewModel

	{
		public string ContextName { get; private set; }
		public Func<string, bool> ContextMatcher { get; private set; }
		public CoordinatorContextType ContextType { get; private set; }
		public CoordinatorStateType StateType { get; private set; }

		public static CoordinatorConstraint<VM> ForContext(string contextName) =>
			new()
			{
				ContextName = contextName,
				ContextType = CoordinatorContextType.ContextName
			};

		public static CoordinatorConstraint<VM> WithContextMatcher(Func<string, bool> matcher) =>
			new()
			{
				ContextMatcher = matcher,
				ContextType = CoordinatorContextType.ContextMatcher
			};

		public static CoordinatorConstraint<VM> ThatRequiresAnyContext() =>
			new()
			{
				ContextType = CoordinatorContextType.WithAnyContext
			};

		public static CoordinatorConstraint<VM> ThatExcludesContext() =>
			new()
			{
				ContextType = CoordinatorContextType.WithoutContext
			};

		public static CoordinatorConstraint<VM> ThatAreActive() =>
			new()
			{
				StateType = CoordinatorStateType.Active
			};

		public static CoordinatorConstraint<VM> ThatAreOrphaned() =>
			new()
			{
				StateType = CoordinatorStateType.Orphaned
			};
	}

	public enum CoordinatorContextType { ContextName, ContextMatcher, WithAnyContext, WithoutContext }
	public enum CoordinatorStateType { Active, Orphaned }
}
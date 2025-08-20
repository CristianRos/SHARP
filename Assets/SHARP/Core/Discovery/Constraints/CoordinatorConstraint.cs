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

		public CoordinatorConstraint()
		{
			ContextName = null;
			ContextMatcher = null;
			ContextType = CoordinatorContextType.None;
			StateType = CoordinatorStateType.None;
		}

		CoordinatorConstraint(
			string contextName,
			Func<string, bool> contextMatcher,
			CoordinatorContextType contextType,
			CoordinatorStateType stateType)
		{
			ContextName = contextName;
			ContextMatcher = contextMatcher;
			ContextType = contextType;
			StateType = stateType;
		}

		public void ForContext(string contextName)
		{
			ContextName = contextName;
			ContextType = CoordinatorContextType.ContextName;
		}

		public void WithContextMatcher(Func<string, bool> matcher)
		{
			ContextMatcher = matcher;
			ContextType = CoordinatorContextType.ContextMatcher;
		}

		public void ThatRequiresAnyContext()
		{
			ContextType = CoordinatorContextType.WithAnyContext;
		}

		public void ThatExcludesContext()
		{
			ContextType = CoordinatorContextType.WithoutContext;
		}

		public void ThatAreActive()
		{
			StateType = CoordinatorStateType.Active;
		}

		public void ThatAreOrphaned()
		{
			StateType = CoordinatorStateType.Orphaned;
		}

		public CoordinatorConstraint<VM> Clone() =>
			new(
				ContextName,
				ContextMatcher,
				ContextType,
				StateType
			);
	}

	public enum CoordinatorContextType { ContextName, ContextMatcher, WithAnyContext, WithoutContext, None }
	public enum CoordinatorStateType { Active, Orphaned, None }
}
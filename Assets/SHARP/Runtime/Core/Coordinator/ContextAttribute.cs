using System;

namespace SHARP.Core
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class ContextAttribute : Attribute
	{
		public string DefaultContext { get; }

		public ContextAttribute(string defaultContext)
		{
			DefaultContext = defaultContext;
		}
	}
}
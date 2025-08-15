using UnityEngine;

namespace SHARP.Core
{
	public class SpatialConstraint<VM> : IQueryConstraint<VM>
		where VM : IViewModel
	{
		public Transform ReferenceTransform { get; private set; }
		public SpatialRelationType RelationType { get; private set; }
		public int? DepthLimit { get; private set; }
		public bool WithinDepth { get; private set; }

		public static SpatialConstraint<VM> ChildrenOf(Transform reference, int? depthLimit = null, bool withinDepth = true) =>
			new()
			{
				ReferenceTransform = reference,
				RelationType = SpatialRelationType.Children,
				DepthLimit = depthLimit,
				WithinDepth = withinDepth
			};

		public static SpatialConstraint<VM> DescendantsOf(Transform reference, int? depthLimit = null) =>
			new()
			{
				ReferenceTransform = reference,
				RelationType = SpatialRelationType.Descendants,
				DepthLimit = depthLimit
			};

		public static SpatialConstraint<VM> SiblingsOf(Transform reference) =>
			new()
			{
				ReferenceTransform = reference,
				RelationType = SpatialRelationType.Siblings
			};

		public static SpatialConstraint<VM> SiblingsOfIncludingSelf(Transform reference) =>
			new()
			{
				ReferenceTransform = reference,
				RelationType = SpatialRelationType.SiblingsAndSelf
			};
	}

	public enum SpatialRelationType { Children, Descendants, Siblings, SiblingsAndSelf }
}
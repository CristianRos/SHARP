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

		public SpatialConstraint()
		{
			ReferenceTransform = null;
			RelationType = SpatialRelationType.None;
			DepthLimit = null;
			WithinDepth = true;
		}

		SpatialConstraint(
			Transform reference,
			SpatialRelationType relationType,
			int? depthLimit,
			bool withinDepth)
		{
			ReferenceTransform = reference;
			RelationType = relationType;
			DepthLimit = depthLimit;
			WithinDepth = withinDepth;
		}

		public void ChildrenOf(Transform reference)
		{
			ReferenceTransform = reference;
			RelationType = SpatialRelationType.Children;
		}

		public void DescendantsOf(Transform reference, int? depthLimit = null, bool withinDepth = true)
		{
			ReferenceTransform = reference;
			RelationType = SpatialRelationType.Descendants;
			DepthLimit = depthLimit;
			WithinDepth = withinDepth;
		}

		public void SiblingsOf(Transform reference)
		{
			ReferenceTransform = reference;
			RelationType = SpatialRelationType.Siblings;
		}

		public void SiblingsOfIncludingSelf(Transform reference)
		{
			ReferenceTransform = reference;
			RelationType = SpatialRelationType.SiblingsAndSelf;
		}

		public SpatialConstraint<VM> Clone() =>
			new(
				ReferenceTransform,
				RelationType,
				DepthLimit,
				WithinDepth
			);
	}

	public enum SpatialRelationType { Children, Descendants, Siblings, SiblingsAndSelf, None }
}
using UnityEngine;

namespace SHARP.Core
{
	public enum HierarchyDirection { Up, Down }

	public class HierarchyQuery
	{
		public Transform ReferenceTransform { get; set; }
		public int? ExactDepth { get; set; }
		public int? MaxDepth { get; set; }
	}
}
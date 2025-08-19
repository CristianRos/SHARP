using System;
using System.Collections.Generic;

namespace SHARP.Core
{
	public class BiDirectionalMap<TKey, TValue>
	{
		public readonly Dictionary<TKey, TValue> Forward = new();
		public readonly Dictionary<TValue, TKey> Reverse = new();

		public int Count => Forward.Count;

		public Dictionary<TKey, TValue>.KeyCollection Keys => Forward.Keys;
		public Dictionary<TKey, TValue>.ValueCollection Values => Forward.Values;


		public void Add(TKey key, TValue value)
		{
			if (Forward.ContainsKey(key) || Reverse.ContainsKey(value))
				throw new ArgumentException("Duplicate key or value.");

			Forward.Add(key, value);
			Reverse.Add(value, key);
		}

		public bool TryAdd(TKey key, TValue value)
		{
			if (Forward.ContainsKey(key) || Reverse.ContainsKey(value))
				return false;

			Forward[key] = value;
			Reverse[value] = key;
			return true;
		}

		public bool RemoveByKey(TKey key)
		{
			if (!Forward.TryGetValue(key, out var value))
				return false;

			Forward.Remove(key);
			Reverse.Remove(value);
			return true;
		}

		public bool RemoveByValue(TValue value)
		{
			if (!Reverse.TryGetValue(value, out var key))
				return false;

			Reverse.Remove(value);
			Forward.Remove(key);
			return true;
		}

		public bool TryGetValue(TKey key, out TValue value) =>
			Forward.TryGetValue(key, out value);

		public bool TryGetKey(TValue value, out TKey key) =>
			Reverse.TryGetValue(value, out key);

		public void Clear()
		{
			Forward.Clear();
			Reverse.Clear();
		}
	}
}
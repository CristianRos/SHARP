using System;
using System.Collections.Generic;
using System.Linq;

namespace SHARP.Core
{
	public class BiDirectionalSetMap<TKey, TValue>
	{
		private readonly Dictionary<TKey, HashSet<TValue>> _forward = new();
		private readonly Dictionary<TValue, TKey> _reverse = new();

		public int Count => _forward.Count;

		public Dictionary<TKey, HashSet<TValue>>.KeyCollection ForwardKeys => _forward.Keys;
		public Dictionary<TKey, HashSet<TValue>>.ValueCollection ForwardValues => _forward.Values;

		public Dictionary<TValue, TKey>.KeyCollection ReverseKeys => _reverse.Keys;
		public Dictionary<TValue, TKey>.ValueCollection ReverseValues => _reverse.Values;

		public void Add(TKey key, TValue value)
		{
			if (_reverse.ContainsKey(value))
				throw new ArgumentException("Value already exists in the map.");

			if (!_forward.TryGetValue(key, out var set))
			{
				set = new HashSet<TValue>();
				_forward[key] = set;
			}

			set.Add(value);
			_reverse[value] = key;
		}

		public bool TryAdd(TKey key, TValue value)
		{
			if (_reverse.ContainsKey(value))
				return false;

			if (!_forward.TryGetValue(key, out var set))
			{
				set = new HashSet<TValue>();
				_forward[key] = set;
			}

			set.Add(value);
			_reverse[value] = key;
			return true;
		}

		public bool Remove(TKey key, TValue value)
		{
			if (!_forward.TryGetValue(key, out var set) || !set.Remove(value))
				return false;

			_reverse.Remove(value);
			if (set.Count == 0) _forward.Remove(key);
			return true;
		}

		public bool RemoveByKey(TKey key)
		{
			if (!_forward.TryGetValue(key, out var set))
				return false;

			_forward.Remove(key);
			foreach (var value in set)
			{
				_reverse.Remove(value);
			}
			return true;
		}

		public bool RemoveByValue(TValue value)
		{
			if (!_reverse.TryGetValue(value, out var key))
				return false;

			_reverse.Remove(value);
			if (_forward.TryGetValue(key, out var set))
			{
				set.Remove(value);
				if (set.Count == 0) _forward.Remove(key);
			}

			return true;
		}

		public bool TryGetValues(TKey key, out HashSet<TValue> values) => _forward.TryGetValue(key, out values);
		public bool TryGetKey(TValue value, out TKey key) => _reverse.TryGetValue(value, out key);

		public void Clear()
		{
			_forward.Clear();
			_reverse.Clear();
		}
	}
}
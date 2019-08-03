using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;

namespace ExpirableCollections
{
    /// <summary>
    /// A dictionary that removes items after a set amount of time.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ExpirableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, Tuple<DateTime, TValue>> _dictionary
            = new Dictionary<TKey, Tuple<DateTime, TValue>>();
        private readonly Timer _timer;

        /// <summary>
        /// The length of time the items in the collection are valid.
        /// In the event the lifespan is changed while items are still
        /// in the collection and the expiration of one or more items
        /// is in the past, the expired items will be removed during
        /// the next poll. 
        /// </summary>
        public TimeSpan Lifespan { get; set; }

        /// <summary>
        /// The frequency, in milliseconds, in which the application polls the
        /// collection for updates.
        /// </summary>
        public double Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        /// <summary>
        /// Maintains a dictionary of items that are periodically removed
        /// after a set interval.
        /// </summary>
        /// <param name="interval">
        /// The frequency, in milliseconds, in which the application polls
        /// the collection for updates.
        /// </param>
        /// <param name="lifespan">
        /// The length of time the items in the collection are valid.
        /// In the event the lifespan is changed while items are still
        /// in the collection and the expiration of one or more items
        /// is in the past, the expired items will be removed during
        /// the next poll.
        /// </param>
        public ExpirableDictionary(int interval, TimeSpan lifespan)
        {
            _timer = new Timer { Interval = interval };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();

            Lifespan = lifespan;
        }

        /// <summary>
        /// Maintains a dictionary of items that are periodically removed
        /// after a set interval.
        /// </summary>
        /// <param name="interval">
        /// The frequency, in milliseconds, in which the application polls
        /// the collection for updates.
        /// </param>
        /// <param name="lifespan">
        /// The length of time the items in the collection are valid.
        /// In the event the lifespan is changed while items are still
        /// in the collection and the expiration of one or more items
        /// is in the past, the expired items will be removed during
        /// the next poll.
        /// </param>
        /// <param name="dictionary">
        /// Provides a dictionary to populate the ExpirableDictionary.
        /// </param>
        public ExpirableDictionary(int interval, TimeSpan lifespan,
            Dictionary<TKey, TValue> dictionary)
        {
            _timer = new Timer { Interval = interval };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();

            Lifespan = lifespan;

            foreach (var item in dictionary)
            {
                var pair = new KeyValuePair<TKey, Tuple<DateTime, TValue>>(item.Key, 
                    new Tuple<DateTime, TValue>(DateTime.Now, item.Value));

                _dictionary.Add(pair);
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var item in _dictionary)
            {
                if (DateTime.Now - item.Value.Item1 >= Lifespan)
                {
                    _dictionary.Remove(item);
                }
            }
        }

        /// <summary>
        /// Converts ExpirableDictionary to a Dictionary.
        /// </summary>
        /// <returns>
        /// A dictionary containing all the items
        /// currently in the ExpirableDictionary.
        /// </returns>
        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dictionary = new Dictionary<TKey, TValue>();

            foreach (var item in _dictionary)
            {
                dictionary.Add(item.Key, item.Value.Item2);
            }

            return dictionary;
        }

        #region IDictionary Implementation
        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in _dictionary)
            {
                yield return new KeyValuePair<TKey, TValue>(item.Key,
                    item.Value.Item2);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in _dictionary)
            {
                yield return new KeyValuePair<TKey, TValue>(item.Key,
                    item.Value.Item2);
            }
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item.Key,
                new Tuple<DateTime, TValue>(DateTime.Now, item.Value));
        }

        /// <inheritdoc />
        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            foreach (var t in _dictionary)
            {
                var pair = new KeyValuePair<TKey, TValue>(t.Key, t.Value.Item2);
                if ((object)item == (object)pair)
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            const BindingFlags bindingFlags = BindingFlags.NonPublic |
                                              BindingFlags.Instance;
            var entries = _dictionary.GetType().GetField("_entries", bindingFlags)?.GetValue(_dictionary);
            if (entries == null)
                return;

            var entry = _dictionary.GetType().GetNestedType("Entry", bindingFlags);
            var hashCodeProp = entry.GetField("hashCode");
            var keyProp = entry.GetField("key");
            var valueProp = entry.GetField("value");

            foreach (var item in (IEnumerable) entries)
            {
                var hashCode = (int)hashCodeProp.GetValue(item);
                if (hashCode < 0)
                    continue;

                var key = (TKey)keyProp.GetValue(item);
                var value = (TValue)valueProp.GetValue(item);
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(key, value);
            }
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Remove(item.Key);
        }

        /// <inheritdoc />
        public int Count => _dictionary.Count;

        /// <inheritdoc />
        public bool IsReadOnly => _dictionary.IsReadOnly;

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, new Tuple<DateTime, TValue>(DateTime.Now, value));
        }

        /// <inheritdoc />
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            if(_dictionary.TryGetValue(key, out var outValue))
            {
                value = outValue.Item2;
                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        public TValue this[TKey key]
        {
            get => _dictionary[key].Item2;
            set => _dictionary[key] = new Tuple<DateTime, TValue>(DateTime.Now, value);
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys => _dictionary.Keys;

        /// <inheritdoc />
        public ICollection<TValue> Values
        {
            get
            {
                var output = new List<TValue>();
                foreach (var item in _dictionary.Values)
                {
                    output.Add(item.Item2);
                }

                return output;
            }
        }
        #endregion
    }
}

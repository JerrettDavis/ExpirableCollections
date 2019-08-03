using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

namespace ExpirableCollections
{
    // Modified from this SO answer:
    // https://stackoverflow.com/questions/13266579/how-to-remove-items-from-a-generic-list-after-n-minutes
    /// <summary>
    /// A list that removes items after a set amount of time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExpirableList<T> : IList<T>
    {
        private readonly List<Tuple<DateTime, T>> _collection = new 
            List<Tuple<DateTime, T>>();
        private readonly Timer _timer;

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
        /// The length of time the items in the collection are valid.
        /// In the event the lifespan is changed while items are still
        /// in the collection and the expiration of one or more items
        /// is in the past, the expired items will be removed during
        /// the next poll. 
        /// </summary>
        public TimeSpan Lifespan { get; set; }

        /// <summary>
        /// Maintains a list of items that are periodically removed
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
        public ExpirableList(int interval, TimeSpan lifespan)
        {
            _timer = new Timer { Interval = interval };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();

            Lifespan = lifespan;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var item in _collection)
            {
                if (DateTime.Now - item.Item1 >= Lifespan)
                {
                    _collection.Remove(item);
                }
            }
        }

        #region IList Implementation
        /// <inheritdoc />
        public T this[int index]
        {
            get => _collection[index].Item2;
            set => _collection[index] 
                = new Tuple<DateTime, T>(DateTime.Now, value);
        }


        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _collection)
            {
                yield return item.Item2;
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in _collection)
            {
                yield return item.Item2;
            }
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            _collection.Add(new Tuple<DateTime, T>(DateTime.Now, item));
        }

        /// <inheritdoc />
        public int Count => _collection.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void CopyTo(T[] array, int index)
        {
            for (var i = 0; i < _collection.Count; i++)
                array[i + index] = _collection[i].Item2;
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            var contained = Contains(item);
            for (var i = _collection.Count - 1; i >= 0; i--)
            {
                if ((object)_collection[i].Item2 == (object)item)
                    _collection.RemoveAt(i);
            }
            return contained;
        }

        /// <inheritdoc />
        public void RemoveAt(int i)
        {
            _collection.RemoveAt(i);
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            foreach (var t in _collection)
            {
                if ((object)t.Item2 == (object)item)
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            _collection.Insert(index, 
                new Tuple<DateTime, T>(DateTime.Now, item));
        }

        /// <inheritdoc />
        public int IndexOf(T item)
        {
            for (var i = 0; i < _collection.Count; i++)
            {
                if ((object)_collection[i].Item2 == (object)item)
                    return i;
            }

            return -1;
        }

        /// <inheritdoc />
        public void Clear()
        {
            _collection.Clear();
        }
        #endregion
    }
}

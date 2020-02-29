using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Utility
{
    /// <summary>
    /// Max priority queue implementation that uses a binary heap. Supports priority increases.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to determine object priority.</typeparam>
    /// <typeparam name="TValue">The type of objects to be stored.</typeparam>
    internal sealed class KeyedPriorityQueue<TKey, TValue>
    {
        private readonly IComparer<TKey> comparer;
        private readonly IDictionary<TValue, int> lookup;
        private (TKey, TValue)[] heap = new (TKey, TValue)[16];

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedPriorityQueue{TKey,TValue}"/> class that uses the default comparer for TKey.
        /// </summary>
        public KeyedPriorityQueue()
            : this(Comparer<TKey>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedPriorityQueue{TKey,TValue}"/> class that uses the specified comparison method.
        /// </summary>
        /// <param name="comparison">The delegate to use to compare items in the queue.</param>
        public KeyedPriorityQueue(Comparison<TKey> comparison)
            : this(Comparer<TKey>.Create(comparison ?? throw new ArgumentNullException(nameof(comparison))))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedPriorityQueue{TKey,TValue}"/> class that uses the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer to use to compare items in the queue.</param>
        public KeyedPriorityQueue(IComparer<TKey> comparer)
        {
            this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            this.lookup = new Dictionary<TValue, int>();
        }

        /// <summary>
        /// Gets the number of items in the queue.
        /// </summary>
        public int Count { get; private set; } = 0;

        /// <summary>
        /// Enqueues an item.
        /// </summary>
        /// <param name="key">The key for the item.</param>
        /// <param name="value">The item to enqueue.</param>
        public void Enqueue(TKey key, TValue value)
        {
            if (Count >= heap.Length)
            {
                Array.Resize(ref heap, Math.Max(heap.Length * 2, 1));
            }

            heap[Count] = (key, value);
            lookup[value] = Count;
            Count++;
            BubbleUp(Count - 1);
        }

        /// <summary>
        /// Retrieves the highest-priority item from the queue.
        /// </summary>
        /// <returns>The dequeued item.</returns>
        public TValue Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            (_, TValue value) = heap[0];

            Count--;
            Swap(Count, 0);
            heap[Count] = default; // To avoid memory leak for reference types
            lookup.Remove(value); // also to avoid memory leak
            BubbleDown(0);

            return value;
        }

        /// <summary>
        /// Retrieves the highest-priority item from the queue, without actually removing it from the queue.
        /// </summary>
        /// <returns>The next item in the queue.</returns>
        public TValue Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            return heap[0].Item2;
        }

        /// <summary>
        /// Increase the priority of an item in the queue.
        /// </summary>
        /// <param name="item">The item to increase the priority of.</param>
        /// <param name="newKey">The item's new key. Must result in an increase in priority.</param>
        public void IncreasePriority(TValue item, TKey newKey)
        {
            var i = lookup[item];

            // Not foolproof, sometimes won't fire for priority reduction,
            // but better than checking against old key (which could have changed in place).
            if ((LChild(i) < Count && this.comparer.Compare(newKey, heap[LChild(i)].Item1) < 0) || (RChild(i) < Count && this.comparer.Compare(newKey, heap[RChild(i)].Item1) < 0))
            {
                throw new ArgumentException("Priority reduction not supported", nameof(newKey));
            }

            heap[i].Item1 = newKey;
            BubbleUp(i);
        }

        private static int Parent(int i) => (i + 1) / 2 - 1;

        private static int LChild(int i) => (i + 1) * 2 - 1;

        private static int RChild(int i) => LChild(i) + 1;

        private void BubbleUp(int i)
        {
            while (i > 0 && !Dominates(Parent(i), i))
            {
                Swap(i, Parent(i));
                i = Parent(i);
            }
        }

        private void BubbleDown(int i)
        {
            int dominating;
            while ((dominating = GetDominating(i)) != i)
            {
                Swap(i, dominating);
                i = dominating;
            }
        }

        private int GetDominating(int parentIndex)
        {
            var dominating = parentIndex;
            dominating = GetDominating(dominating, LChild(parentIndex));
            dominating = GetDominating(dominating, RChild(parentIndex));
            return dominating;
        }

        private int GetDominating(int lowIndex, int highIndex)
        {
            return highIndex < Count && !Dominates(lowIndex, highIndex) ? highIndex : lowIndex;
        }

        private bool Dominates(int i, int j) => this.comparer.Compare(heap[i].Item1, heap[j].Item1) >= 0;

        private void Swap(int i, int j)
        {
            var temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;

            // don't forget to update the lookup
            this.lookup[heap[i].Item2] = i;
            this.lookup[heap[j].Item2] = j;
        }
    }
}

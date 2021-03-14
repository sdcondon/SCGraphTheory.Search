using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Utility
{
    /// <summary>
    /// Max priority queue implementation that uses a binary heap. Supports priority increases.
    /// </summary>
    /// <typeparam name="TElement">The type of objects to be stored.</typeparam>
    /// <typeparam name="TPriority">The type used to determine object priority.</typeparam>
    internal sealed class KeyedPriorityQueue<TElement, TPriority>
    {
        private readonly IComparer<TPriority> comparer;
        private readonly IDictionary<TElement, int> lookup;
        private (TElement element, TPriority priority)[] heap = new (TElement, TPriority)[16];

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedPriorityQueue{TElement, TPriority}"/> class that uses the default comparer for TPriority.
        /// </summary>
        public KeyedPriorityQueue()
            : this(Comparer<TPriority>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedPriorityQueue{TElement, TPriority}"/> class that uses the specified comparison method.
        /// </summary>
        /// <param name="comparison">The delegate to use to compare items in the queue.</param>
        public KeyedPriorityQueue(Comparison<TPriority> comparison)
            : this(Comparer<TPriority>.Create(comparison ?? throw new ArgumentNullException(nameof(comparison))))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedPriorityQueue{TElement, TPriority}"/> class that uses the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer to use to compare items in the queue.</param>
        public KeyedPriorityQueue(IComparer<TPriority> comparer)
        {
            this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            this.lookup = new Dictionary<TElement, int>();
        }

        /// <summary>
        /// Gets the number of items in the queue.
        /// </summary>
        public int Count { get; private set; } = 0;

        /// <summary>
        /// Enqueues an item.
        /// </summary>
        /// <param name="element">The item to enqueue.</param>
        /// <param name="priority">The priority of the item.</param>
        public void Enqueue(TElement element, TPriority priority)
        {
            if (Count >= heap.Length)
            {
                Array.Resize(ref heap, Math.Max(heap.Length * 2, 1));
            }

            heap[Count] = (element, priority);
            lookup[element] = Count;
            Count++;
            BubbleUp(Count - 1);
        }

        /// <summary>
        /// Retrieves the highest-priority item from the queue.
        /// </summary>
        /// <returns>The dequeued item.</returns>
        public TElement Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            (TElement value, _) = heap[0];

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
        public TElement Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            return heap[0].element;
        }

        /// <summary>
        /// Increase the priority of an item in the queue.
        /// </summary>
        /// <param name="item">The item to increase the priority of.</param>
        /// <param name="newPriority">The item's new priority. Must result in an increase in priority.</param>
        public void IncreasePriority(TElement item, TPriority newPriority)
        {
            var i = lookup[item];

            // Not foolproof, sometimes won't fire for priority reduction,
            // but better than checking against old priority (which could have changed in place).
            if ((LChild(i) < Count && this.comparer.Compare(newPriority, heap[LChild(i)].priority) < 0) || (RChild(i) < Count && this.comparer.Compare(newPriority, heap[RChild(i)].priority) < 0))
            {
                throw new ArgumentException("Priority reduction not supported", nameof(newPriority));
            }

            heap[i].priority = newPriority;
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

        private bool Dominates(int i, int j) => this.comparer.Compare(heap[i].priority, heap[j].priority) >= 0;

        private void Swap(int i, int j)
        {
            var temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;

            // don't forget to update the lookup
            this.lookup[heap[i].element] = i;
            this.lookup[heap[j].element] = j;
        }
    }
}

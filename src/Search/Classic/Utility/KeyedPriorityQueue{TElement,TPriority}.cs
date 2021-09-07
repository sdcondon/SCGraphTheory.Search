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
        private readonly IDictionary<TElement, int> indicesByElement;
        private readonly IComparer<TPriority> priorityComparer;

        private (TElement element, TPriority priority)[] heap = new (TElement, TPriority)[16];

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedPriorityQueue{TElement, TPriority}"/> class.
        /// </summary>
        /// <param name="priorityComparer">The comparer to use to compare priorities.</param>
        public KeyedPriorityQueue(IComparer<TPriority> priorityComparer)
        {
            this.indicesByElement = new Dictionary<TElement, int>();
            this.priorityComparer = priorityComparer ?? throw new ArgumentNullException(nameof(priorityComparer));
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
            if (indicesByElement.ContainsKey(element))
            {
                throw new ArgumentException("Element already exists in the queue", nameof(element));
            }

            if (Count >= heap.Length)
            {
                Array.Resize(ref heap, Math.Max(heap.Length * 2, 1));
            }

            BubbleUp(Count++, (element, priority));
        }

        /// <summary>
        /// Retrieves the highest-priority item from the queue.
        /// </summary>
        /// <returns>The dequeued item.</returns>
        public TElement Dequeue()
        {
            return Dequeue(out _);
        }

        /// <summary>
        /// Retrieves the highest-priority item from the queue.
        /// </summary>
        /// <param name="priority">The priority of the dequeued item.</param>
        /// <returns>The dequeued item.</returns>
        public TElement Dequeue(out TPriority priority)
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            var entry = heap[0];

            BubbleDown(0, heap[--Count]);

            heap[Count] = default; // To avoid memory leak for reference types
            indicesByElement.Remove(entry.element);

            priority = entry.priority;
            return entry.element;
        }

        /// <summary>
        /// Retrieves the highest-priority item from the queue, without actually removing it from the queue.
        /// </summary>
        /// <returns>The next item in the queue.</returns>
        public TElement Peek()
        {
            return Peek(out _);
        }

        /// <summary>
        /// Retrieves the highest-priority item from the queue, without actually removing it from the queue.
        /// </summary>
        /// <param name="priority">The priority of the next item in the queue.</param>
        /// <returns>The next item in the queue.</returns>
        public TElement Peek(out TPriority priority)
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            priority = heap[0].priority;
            return heap[0].element;
        }

        /// <summary>
        /// Increase the priority of an item in the queue.
        /// </summary>
        /// <param name="item">The item to increase the priority of.</param>
        /// <param name="newPriority">The item's new priority. Must result in an increase in priority.</param>
        public void IncreasePriority(TElement item, TPriority newPriority)
        {
            var i = indicesByElement[item];
            ref var entry = ref heap[i];

            if (priorityComparer.Compare(newPriority, entry.priority) < 0)
            {
                throw new ArgumentException("Priority reduction not supported", nameof(newPriority));
            }

            BubbleUp(i, (entry.element, newPriority));
        }

        /// <summary>
        /// Gets the priority associated with a given element, if it is present.
        /// </summary>
        /// <param name="element">The element whose priority to retrieve.</param>
        /// <param name="priority">
        /// When this method returns, the priority associated with the specified element, if the
        /// element is found; otherwise, the default value for the type of the value parameter.
        /// </param>
        /// <returns>
        /// true if the queue contains an element with the specified key; otherwise, false.
        /// </returns>
        public bool TryGetPriority(TElement element, out TPriority priority)
        {
            if (indicesByElement.TryGetValue(element, out var index))
            {
                priority = heap[index].priority;
                return true;
            }
            else
            {
                priority = default;
                return false;
            }
        }

        private void BubbleUp(int index, (TElement element, TPriority priority) entry)
        {
            while (index > 0)
            {
                var parentIndex = (index - 1) / 2;
                ref var parent = ref heap[parentIndex];

                if (priorityComparer.Compare(parent.priority, entry.priority) >= 0)
                {
                    break;
                }

                heap[index] = parent;
                indicesByElement[parent.element] = index;

                index = parentIndex;
            }

            heap[index] = entry;
            indicesByElement[entry.element] = index;
        }

        private void BubbleDown(int i, (TElement element, TPriority priority) entry)
        {
            while (i != -1)
            {
                var dominatingIndex = -1;
                ref var dominating = ref entry;

                // TODO-PERFORMANCE: Does this *really* need to be a loop? Performance test this at some point..
                for (int childIndex = 2 * i + 1; childIndex - 2 * i <= 2 && childIndex < Count; childIndex++)
                {
                    ref var child = ref heap[childIndex];
                    if (priorityComparer.Compare(child.priority, dominating.priority) > 0)
                    {
                        dominatingIndex = childIndex;
                        dominating = ref child;
                    }
                }

                heap[i] = dominating;
                indicesByElement[dominating.element] = i;

                i = dominatingIndex;
            }
        }
    }
}

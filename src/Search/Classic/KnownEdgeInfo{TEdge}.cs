namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// A container for information about an edge discovered by a search.
    /// </summary>
    /// <typeparam name="TEdge">The type of the edge.</typeparam>
    public readonly struct KnownEdgeInfo<TEdge>
    {
        /// <summary>
        /// The known edge.
        /// </summary>
        public readonly TEdge Edge;

        /// <summary>
        /// A value indicating whether this edge is on the frontier of the search.
        /// </summary>
        public readonly bool IsOnFrontier;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnownEdgeInfo{TEdge}"/> struct.
        /// </summary>
        /// <param name="edge">The known edge.</param>
        /// <param name="isOnFrontier">A value indicating whether this edge is on the frontier of the search.</param>
        public KnownEdgeInfo(TEdge edge, bool isOnFrontier)
        {
            this.Edge = edge;
            this.IsOnFrontier = isOnFrontier;
        }
    }
}

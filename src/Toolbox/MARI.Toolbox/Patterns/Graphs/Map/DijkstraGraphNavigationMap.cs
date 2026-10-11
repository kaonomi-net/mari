// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Graphs.Map
{
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Helpers;

    using System;
    using System.Collections.Frozen;
    using System.Diagnostics;

    /// <summary>
    /// Navigator distance using Dijkstra Algorithm
    /// </summary>
    public sealed class DijkstraGraphNavigationMap : SafeDisposable, IGraphNavigationMap
    {
        #region Fields

        private readonly Dictionary<string, DijkstraGraphNavigationIndex> _dijkstraGraphNavigationIndices;
        private readonly ReaderWriterLockSlim _locker;
        private readonly IGraph _graph;

        #endregion Fields

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DijkstraGraphNavigationMap"/> class.
        /// </summary>
        private DijkstraGraphNavigationMap(IGraph graph, IReadOnlyCollection<DijkstraGraphNavigationIndex> dijkstraGraphNavigationIndices)
        {
            this._locker = new ReaderWriterLockSlim();
            this._graph = graph;
            this._dijkstraGraphNavigationIndices = dijkstraGraphNavigationIndices.ToDictionary(k => k.Source.Uri);
        }

        #endregion Ctor

        #region Nested

        /// <summary>
        ///
        /// </summary>
        private sealed class DijkstraGraphNavigationIndex
        {
            #region Fields

            private readonly IReadOnlyDictionary<IGraphNode, int> _map;

            #endregion Fields

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="DijkstraGraphNavigationIndex"/> class.
            /// </summary>
            public DijkstraGraphNavigationIndex(IGraphNode source, IReadOnlyDictionary<IGraphNode, int>? map)
            {
                this._map = map?.ToFrozenDictionary() ?? DictionaryHelper<IGraphNode, int>.ReadOnly;

                this.Source = source;
            }

            #endregion Ctor

            #region Properties

            /// <summary>
            /// Gets the source.
            /// </summary>
            public IGraphNode Source { get; }

            #endregion Properties

            #region Methods

            /// <inheritdoc cref="IGraphNavigationMap.GetShortestPath(IGraphNode, IGraphNode, string[])" />
            /// <remarks>
            ///     For now <paramref name="relationTypes"/> is ignored
            /// </remarks>
            internal IReadOnlyCollection<IGraphNode> GetShortestPath(IGraphNode target, GetPathOptions? option = null)
            {
                var maxDistanceTolerate = option?.MaxToleratePathSize;
                if (maxDistanceTolerate <= 0)
                    maxDistanceTolerate = int.MaxValue;

                if (!this._map.TryGetValue(target, out var distance) || distance >= maxDistanceTolerate)
                {
                    // No connection
                    return EnumerableHelper<IGraphNode>.ReadOnly;
                }

                var path = new Stack<IGraphNode>();

                do
                {
                    var minimalDistance = distance;

                    path.Push(target);
                    var directConnections = target.GetRelatedLink(option?.FilterRelationTypes);

                    distance = -1;

                    if (minimalDistance > 0 && directConnections is not null && directConnections.Any())
                    {
                        foreach (var co in directConnections)
                        {
                            if (this._map.TryGetValue(co, out var coMapDistances) && coMapDistances < minimalDistance)
                            {
                                target = co;
                                minimalDistance = coMapDistances;
                                distance = coMapDistances;
                                break;
                            }
                        }
                    }
                } while (distance > -1);

                return path;
            }

            /// <summary>
            /// Serialize the distance into group of int (index of the graphnode in the <paramref name="indexTable"/>) separate by -1 each time the score increase
            /// </summary>
            internal int[] ToSerializable(IReadOnlyDictionary<string, int> indexTable)
            {
                return this._map.GroupBy(m => m.Value)
                                .Select(m => (Distance: m.Key, NodeIndexes: m.Select(nodeUri => indexTable.TryGetValueInline(nodeUri.Key.Uri, out _, -2)).ToArray()))
                                .OrderBy(m => m.Distance)
                                .Aggregate((IEnumerable<int>)null!, (acc, info) => acc is null ? info.NodeIndexes : acc.Append(-1).Concat(info.NodeIndexes)).ToArray();
            }

            /// <summary>
            /// Load distance from format describe by <see cref="ToSerializable"/>
            /// </summary>
            internal static DijkstraGraphNavigationIndex BuildFrom(IGraphNode graphNode,
                                                                   int indx,
                                                                   IGraph graph,
                                                                   DijkstraGraphNavigationMapDefinition definitions,
                                                                   string[] indexNodes)
            {
                var index = (definitions.Map is not null && definitions.Map.Length < indx) ? definitions.Map[indx] : null;

                IReadOnlyDictionary<IGraphNode, int>? map = null;

                if (index is not null)
                {
                    var localMap = new Dictionary<IGraphNode, int>();
                    map = localMap;

                    int score = 1;
                    for (int i = 0; i < index.Length; ++i)
                    {
                        var currentGraphNodeIndex = index[i];

                        if (currentGraphNodeIndex == -1)
                        {
                            score++;
                            continue;
                        }

                        var nodeStr = indexNodes[currentGraphNodeIndex];
                        var node = graph[nodeStr];

                        if (node is null)
                            continue;

                        localMap.Add(node, score);
                    }
                }
                return new DijkstraGraphNavigationIndex(graphNode, map);
            }

            #endregion Methods
        }

        #endregion Nested

        #region Methods

        /// <summary>
        /// Builds from graph
        /// </summary>
        /// <param name="computeOnBuild">By default the distance will be build on demand, set to <c>TRUE</c> all the graph will be distance by this method</param>
        public static async ValueTask<DijkstraGraphNavigationMap> BuildFromAsync(IGraph graph, bool computeOnBuild, CancellationToken token = default, IProgress<(int Counter, int Total)>? progress = null)
        {
            var map = await BuildImplFromAsync(graph);

            if (computeOnBuild)
                map.ComputeAllMissingIndexesAsync(progress, token);

            return map;
        }

        /// <summary>
        /// Builds from graph and pre-distance definitions
        /// </summary>
        public static ValueTask<DijkstraGraphNavigationMap> BuildFromAsync(IGraph graph, DijkstraGraphNavigationMapDefinition definitions)
        {
            return BuildImplFromAsync(graph, definitions);
        }

        /// <summary>
        /// Builds from graph and pre-distance definitions
        /// </summary>
        private static ValueTask<DijkstraGraphNavigationMap> BuildImplFromAsync(IGraph graph, DijkstraGraphNavigationMapDefinition? definitions = null)
        {
            var indexNodes = definitions?.Nodes.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;

            var dijkstraMap = new DijkstraGraphNavigationMap(graph,
                                                             definitions?.Nodes
                                                                         .Select((n, indx) =>
                                                                         {
                                                                             var graphNode = graph[n];

                                                                             if (graphNode is null)
                                                                                 return null;

                                                                             return DijkstraGraphNavigationIndex.BuildFrom(graphNode, indx, graph, definitions, indexNodes);
                                                                         })
                                                                         .NotNull()
                                                                         .ToArray() ?? EnumerableHelper<DijkstraGraphNavigationIndex>.ReadOnlyArray);

            return ValueTask.FromResult(dijkstraMap);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IGraphNode> GetShortestPath(IGraphNode source, IGraphNode target, GetPathOptions? options = null)
        {
            DijkstraGraphNavigationIndex? index = null;

            this._locker.EnterReadLock();

            try
            {
                this._dijkstraGraphNavigationIndices.TryGetValue(source.Uri, out index);
            }
            finally
            {
                this._locker.ExitReadLock();
            }

            if (index is null)
            {
                if (!this._dijkstraGraphNavigationIndices.TryGetValue(source.Uri, out index))
                {
                    using (var timeout = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(System.Diagnostics.Debugger.IsAttached ? 300_000 : 30)))
                    {
                        var newIndex = BuildNavigationMap(source, timeout.Content);
                        this._dijkstraGraphNavigationIndices.Add(source.Uri, newIndex);
                        index = newIndex;
                    }
                }
            }

            return index.GetShortestPath(target, options);
        }

        /// <summary>
        /// Convert local data to serializable one to be build later from
        /// </summary>
        public DijkstraGraphNavigationMapDefinition ToSerializable()
        {
            var indexTable = this._graph.Nodes.Select(n => n.Uri).ToArray();
            var indexedNodeTable = indexTable.Select((n, i) => (n, i)).ToDictionary(k => k.n, v => v.i);

            var map = new int[indexTable.Length][];

            for (int i = 0; i < indexTable.Length; i++)
            {
                var current = indexTable[i];
                if (this._dijkstraGraphNavigationIndices.TryGetValue(current, out var indx))
                    map[i] = indx.ToSerializable(indexedNodeTable);
            }

            return new DijkstraGraphNavigationMapDefinition(indexTable, map);
        }

        #region Tools

        /// <summary>
        /// Compute all missing node navigation distance
        /// </summary>
        private void ComputeAllMissingIndexesAsync(IProgress<(int Counter, int Total)>? progress, CancellationToken token)
        {
            this._locker.EnterWriteLock();
            try
            {
                var missingMapNode = this._graph.Nodes.Where(n => this._dijkstraGraphNavigationIndices.ContainsKey(n.Uri) == false).ToArray();
                int counter = 0;
                foreach (var node in missingMapNode)
                {
                    var indx = BuildNavigationMap(node, token);
                    this._dijkstraGraphNavigationIndices[node.Uri] = indx;

                    if (progress is not null)
                    {
                        counter++;
                        progress.Report((counter, missingMapNode.Length));
                    }
                }
            }
            finally
            {
                this._locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Compute a navigation indice cache for start node <paramref name="node"/>
        /// </summary>
        private DijkstraGraphNavigationIndex BuildNavigationMap(IGraphNode node, CancellationToken token)
        {
            var map = new Dictionary<IGraphNode, int>();
            Map(node, map, token);
            var navigationIndice = new DijkstraGraphNavigationIndex(node, map);
            return navigationIndice;
        }

        /// <summary>
        /// Parkour all the connected node and distance it based on the distance with the root
        /// </summary>
        private void Map(IGraphNode root, Dictionary<IGraphNode, int> map, CancellationToken cancellationToken)
        {
            var nodeSee = new HashSet<string>(this._graph.Nodes.Count + 1);
            var queue = new Queue<(IGraphNode Node, string? ParentUri)>(this._graph.Nodes.Count + 1);

            //var parents = new Dictionary<string, HashSet<string>>();

            queue.Enqueue((root, null));
            var directConnectedNodes = new HashSet<IGraphNode>(this._graph.Nodes.Count + 1);

            while (queue.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var current = queue.Dequeue();

                Debug.WriteLine("--- Current " + current.Node.DisplayName);

                nodeSee.Add(current.Node.Uri);
                directConnectedNodes.Clear();

                foreach (var linked in current.Node.GetRelatedLink())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    directConnectedNodes.Add(linked);

                    if (nodeSee.Contains(linked.Uri))
                        continue;

                    var added = nodeSee.Add(linked.Uri);

                    //HashSet<string>? nodeParentHashset = null;
                    //if (!parents.TryGetValue(linked.Uri, out nodeParentHashset))
                    //{
                    //    nodeParentHashset = new HashSet<string>();
                    //    parents.Add(linked.Uri, nodeParentHashset);
                    //}

                    //nodeParentHashset.Add(current..Uri);
                    //Debug.WriteLine("    -- Add parent " + current.DisplayName + " to " + linked.DisplayName);

                    // Add only if it's the first time i see it
                    if (added)
                    {
                        Debug.WriteLine("    --- Add Child " + linked.DisplayName);
                        queue.Enqueue((linked, current.Node.Uri));
                    }
                }

                var depth = 0;

                if (!string.IsNullOrEmpty(current.ParentUri))
                {
                    var parentNode = this._graph[current.ParentUri];
                    if (parentNode is not null && map.TryGetValue(parentNode, out var parentMappedDistance))
                        depth = parentMappedDistance + 1;
                }

                Debug.WriteLine("    --- Apply depth " + depth);

                map[current.Node] = depth;
                //parents.Remove(current.ParentUri);
            }
        }

        /// <inheritdoc />
        protected override void DisposeBegin()
        {
            this._locker.Dispose();
            base.DisposeBegin();
        }

        #endregion Tools

        #endregion Methods
    }
}
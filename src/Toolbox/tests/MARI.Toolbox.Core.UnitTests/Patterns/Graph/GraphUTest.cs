// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Patterns.Graph
{
    using MARI.Toolbox.Patterns.Graphs;
    using MARI.Toolbox.Patterns.Graphs.Map;

    using NFluent;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="IGraph"/> and co
    /// </summary>
    public sealed class GraphUTest
    {
        #region Fields

        private const string ROOT_URI = "http://kaonomi.net";

        private static readonly GraphDefinition s_graphDefinition;
        private static readonly string[] s_pathMapping;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="GraphUTest"/> class.
        /// </summary>
        static GraphUTest()
        {
            /*
             *  A -- Parent --> B -- Parent --> C
             *  |               ^               |
             *  |               |               |
             *  D -- Parent ----              Parent
             *                  |               |
             *                  |               |
             *  A <-- Parent -- F <-- Parent -- E
             */

            s_pathMapping = new[]
            {
                "a/bdf/ce",
                "b/acdf/e",
                "c/be/adf",
                "d/ab/cf/e",
                "e/cf/ab/d",
                "f/abe/cd"
            };

            var propRef = new GraphPropertyDefinition("prop_ref_name", "name", true);

            var a = new GraphNodeDefinition(ROOT_URI + "/a",
                                            "a",
                                            new[]
                                            {
                                                new GraphPropertyValue(propRef.RefId, null, "a"),
                                                new GraphPropertyValue(null, "Weight", "42"),
                                            },
                                            new[] { "class" },
                                            true);

            var b = new GraphNodeDefinition(ROOT_URI + "/b",
                                            "b",
                                            new[]
                                            {
                                                new GraphPropertyValue(propRef.RefId, null, "b"),
                                                new GraphPropertyValue(null, "Weight", "21"),
                                            },
                                            new[] { "class" },
                                            true);

            var c = new GraphNodeDefinition(ROOT_URI + "/c",
                                            "c",
                                            new[]
                                            {
                                               new GraphPropertyValue(propRef.RefId, null, "c"),
                                               new GraphPropertyValue(null, "Weight", "12"),
                                            },
                                            new[] { "class" },
                                            true);

            var d = new GraphNodeDefinition(ROOT_URI + "/d",
                                            "d",
                                            new[]
                                            {
                                               new GraphPropertyValue(propRef.RefId, null, "d"),
                                               new GraphPropertyValue(null, "Weight", "2"),
                                            },
                                            new[] { "instance" },
                                            true);

            var e = new GraphNodeDefinition(ROOT_URI + "/e",
                                            "e",
                                            null,
                                            new[] { "instance", "test" },
                                            true);

            var f = new GraphNodeDefinition(ROOT_URI + "/f",
                                            "f",
                                            null,
                                            new[] { "test" },
                                            true);

            var aToB = new GraphNodeRelationDefinition(a.Uri, b.Uri, null, GraphNodeRelation.PARENT);
            var bToC = new GraphNodeRelationDefinition(b.Uri, c.Uri, null, GraphNodeRelation.PARENT);
            var cToE = new GraphNodeRelationDefinition(c.Uri, e.Uri, null, GraphNodeRelation.PARENT);
            var eToF = new GraphNodeRelationDefinition(e.Uri, f.Uri, null, GraphNodeRelation.PARENT);
            var fToA = new GraphNodeRelationDefinition(f.Uri, a.Uri, null, GraphNodeRelation.PARENT);

            var aToD = new GraphNodeRelationDefinition(a.Uri,
                                                       d.Uri,
                                                       new[]
                                                       {
                                                          new GraphPropertyValue(null, "Weight", "2"),
                                                       },
                                                       GraphNodeRelation.PARENT);

            var dToB = new GraphNodeRelationDefinition(d.Uri, b.Uri, null, GraphNodeRelation.PARENT);
            var fToB = new GraphNodeRelationDefinition(f.Uri, b.Uri, null, GraphNodeRelation.PARENT);

            var graphDef = new GraphDefinition(ROOT_URI,
                                               new[] { propRef },
                                               new[] { new GraphPropertyValue(propRef.RefId, null, "root") },
                                               new[] { a, b, c, d, e, f },
                                               new[] { aToB, bToC, cToE, eToF, fToA, aToD, dToB, fToB },
                                               null!);
            s_graphDefinition = graphDef;
        }

        #endregion

        #region Methods

        [Fact]
        public void Build_Graph_From_Descriptions()
        {
            var graph = Graph.BuildFrom(s_graphDefinition);
            ValidateGraphFromDefinition(graph, s_graphDefinition);
        }

        [Fact]
        public void Build_Graph_Map_Default()
        {
            var graph = Graph.BuildFrom(s_graphDefinition);

            var a = graph[ROOT_URI + "/a"];
            var e = graph[ROOT_URI + "/e"];

            Check.That(a).IsNotNull();
            Check.That(e).IsNotNull();

            var path = graph.GetShortestPath(a!, e!);

            Check.That(path).IsNotNull()
                            .And
                            .CountIs(3);

            var pathStr = string.Join("", path.Select(p => p.DisplayName));
            Check.That(pathStr).IsEqualTo("afe");
        }

        [Fact]
        public void Build_Graph_Map_Default_No_Intermediate()
        {
            var graph = Graph.BuildFrom(s_graphDefinition);

            var a = graph[ROOT_URI + "/a"];
            var b = graph[ROOT_URI + "/b"];

            Check.That(a).IsNotNull();
            Check.That(b).IsNotNull();

            var path = graph.GetShortestPath(a!, b!);

            Check.That(path).IsNotNull()
                            .And
                            .CountIs(2);

            var pathStr = string.Join("", path.Select(p => p.DisplayName));
            Check.That(pathStr).IsEqualTo("ab");
        }

        [Fact]
        public void Build_Graph_Map_Default_Same_Source()
        {
            var graph = Graph.BuildFrom(s_graphDefinition);

            var a = graph[ROOT_URI + "/a"];

            Check.That(a).IsNotNull();

            var path = graph.GetShortestPath(a!, a!);

            Check.That(path).IsNotNull()
                            .And
                            .CountIs(1);

            var pathStr = string.Join("", path.Select(p => p.DisplayName));
            Check.That(pathStr).IsEqualTo("a");
        }

        [Fact]
        public async Task Dijkstra_Build_Restore()
        {
            var graph = Graph.BuildFrom(s_graphDefinition);

            var a = graph[ROOT_URI + "/a"];
            var e = graph[ROOT_URI + "/e"];

            var map = await DijkstraGraphNavigationMap.BuildFromAsync(graph, true);
            var path = map.GetShortestPath(a!, e!);

            Check.That(path).IsNotNull()
                            .And
                            .CountIs(3);

            var pathStr = string.Join("", path.Select(p => p.DisplayName));
            Check.That(pathStr).IsEqualTo("afe");

            // Get map status
            var def = map.ToSerializable();
            Check.That(def.Map).IsNotNull().And.CountIs(def.Nodes.Count);

            string[] serializeMap = def.Map!.Select(m => string.Join("", m.Select(mm => (mm == -1 ? "/" : def.Nodes.ElementAt(mm).Replace(ROOT_URI + "/", "")))))
                                            .Select(l => string.Join("/", l.Split("/").Select(p => string.Join("", p.OrderBy(a => a)))))
                                            .OrderBy(o => o)
                                            .ToArray();

            Check.That(serializeMap).IsEqualTo(s_pathMapping);

            // Build this time from saved maps
            var otherMap = await DijkstraGraphNavigationMap.BuildFromAsync(graph, def);

            var otherDef = map.ToSerializable();
            Check.That(otherDef.Map).IsNotNull().And.CountIs(otherDef.Nodes.Count);

            string[] otherSerializeMap = otherDef.Map!.Select(m => string.Join("", m.Select(mm => (mm == -1 ? "/" : otherDef.Nodes.ElementAt(mm).Replace(ROOT_URI + "/", "")))))
                                                      .Select(l => string.Join("/", l.Split("/").Select(p => string.Join("", p.OrderBy(a => a)))))
                                                      .OrderBy(o => o)
                                                      .ToArray();

            Check.That(otherSerializeMap).IsEqualTo(s_pathMapping);
        }

        #region Tools

        /// <summary>
        /// Validates the graph follow the schema of the definition
        /// </summary>
        private void ValidateGraphFromDefinition(IGraph graph, GraphDefinition graphDef)
        {
            Check.That(graph.Uri).IsEqualTo(graphDef.RootUri);

            // Check propDef ref
            foreach (var propRef in graphDef.RefPropertiesDefinitions)
            {
                var existRef = graph.PropertyRef.TryGetValueInline(propRef.RefId, out var exist);

                Check.That(exist).IsTrue();
                Check.That(existRef).IsNotNull();

                Check.That(existRef!.RefId).IsEqualTo(propRef.RefId);
                Check.That(existRef!.DisplayName).IsEqualTo(propRef.DisplayName);
                Check.That(existRef!.OnlyOneAllowed).IsEqualTo(propRef.OnlyOneAllowed);
            }

            Check.That(graphDef.Nodes).IsNotNull()
                                      .And
                                      .CountIs(graph.Nodes.Count);

            foreach (var node in graphDef.Nodes)
            {
                var grphNode = graph[node.Uri];

                Check.That(grphNode).IsNotNull();
                Check.That(grphNode!.Uri).IsEqualTo(node.Uri);
                Check.That(grphNode!.IsRoot).IsEqualTo(node.IsRoot);

                Check.That(grphNode!.HasEntity).IsEqualTo(node.Entity is not null);
                if (grphNode.HasEntity)
                    Check.That(grphNode!.EntityType).IsEqualTo(node.Entity!.GetType());

                var relationFrom = graphDef.Relations.Where(r => r.UriSource == node.Uri).ToDictionary(k => k.UriSource + k.RelationType + k.UriTarget);
                var relationTo = graphDef.Relations.Where(r => r.UriTarget == node.Uri).ToDictionary(k => k.UriTarget + k.RelationType + k.UriSource);

                Check.That(grphNode.RelationshipsFrom).IsNotNull()
                                                      .And
                                                      .CountIs(relationFrom.Count);

                foreach (var from in grphNode.RelationshipsFrom)
                {
                    Check.That(from.Source).IsNotNull()
                                           .And
                                           .IsEqualTo(grphNode);

                    var defRelation = relationFrom.TryGetValueInline(from.Source?.Uri + from.RelationType + from.Target?.Uri, out var relFromExist);

                    Check.That(relFromExist).IsTrue();
                    Check.That(defRelation).IsNotNull();

                    Check.That(from.Target?.Uri).IsNotNull().And.IsEqualTo(defRelation!.UriTarget);
                }

                Check.That(grphNode.RelationshipsTo).IsNotNull()
                                                    .And
                                                    .CountIs(relationTo.Count);

                foreach (var to in grphNode.RelationshipsTo)
                {
                    Check.That(to.Target).IsNotNull()
                                         .And
                                         .IsEqualTo(grphNode);

                    var defRelation = relationTo.TryGetValueInline(to.Target?.Uri + to.RelationType + to.Source?.Uri, out var relToExist);

                    Check.That(relToExist).IsTrue();
                    Check.That(defRelation).IsNotNull();

                    Check.That(to.Source?.Uri).IsNotNull().And.IsEqualTo(defRelation!.UriSource);
                }

                // Check properties
                foreach (var propDef in node.Properties ?? EnumerableHelper<GraphPropertyValue>.ReadOnly)
                {
                    var expectNodeProp = propDef.DisplayName;

                    if (!string.IsNullOrEmpty(propDef.RefId))
                    {
                        var refProp = graph.PropertyRef.TryGetValueInline(propDef.RefId, out var refExist);

                        Check.That(refExist).IsTrue();
                        Check.That(refProp).IsNotNull();

                        expectNodeProp = refProp!.DisplayName;
                    }

                    var prop = grphNode.Properties?.FirstOrDefault(p => p.RefPropId == propDef.RefId && p.Name == expectNodeProp);

                    Check.That(prop).IsNotNull();
                    Check.That(prop!.Value).IsEqualTo(propDef.Value);
                }
            }
        }

        #endregion

        #endregion
    }
}

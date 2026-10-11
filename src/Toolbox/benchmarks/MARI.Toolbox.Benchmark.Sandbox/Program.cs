// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

using BenchmarkDotNet.Running;

using MARI.Toolbox.Benchmark.Sandbox;

BenchmarkRunner.Run<StringSplitOptiBenchmark>();

//var inst = new StringSplitOptiBenchmark();
//inst.ClassicSplit();
//inst.OptiSplit();
//inst.LessOptiSplit();

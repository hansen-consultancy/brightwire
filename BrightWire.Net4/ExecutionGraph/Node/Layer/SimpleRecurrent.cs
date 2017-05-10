﻿using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BrightWire.Models;
using ProtoBuf;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    internal class SimpleRecurrent : NodeBase
    {
        MemoryFeeder _memory;
        INode _input, _output = null, _activation;
        int _inputSize;

        public SimpleRecurrent(GraphFactory graph, int inputSize, float[] memory, INode activation, string name = null)
            : base(name)
        {
            _Create(graph, inputSize, memory, activation);
        }

        void _Create(GraphFactory graph, int inputSize, float[] memory, INode activation)
        {
            _inputSize = inputSize;
            _activation = activation;
            int hiddenLayerSize = memory.Length;
            _memory = new MemoryFeeder(memory);
            _input = new FlowThrough();

            var inputChannel = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wh");
            var memoryChannel = graph.Build(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uh");

            _output = graph.Add(inputChannel, memoryChannel)
                .Add(activation)
                .Add(_memory.SetMemoryAction)
                .Build()
            ;
        }

        public override List<IWire> Output => _output.Output;

        public override void ExecuteForward(IContext context)
        {
            foreach (var node in SubNodes)
                node.ExecuteForward(context, 0);
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("SRN", _WriteData(WriteTo));
        }

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _ReadFrom(data, reader => ReadFrom(factory, reader));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            var Wh = _input.SearchFor("Wh") as FeedForward;
            var Uh = _memory.SearchFor("Uh") as FeedForward;

            writer.Write(_inputSize);
            _memory.Data.WriteTo(writer);
            _Serialise(_activation, writer);

            Wh.WriteTo(writer);
            Uh.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var inputSize = reader.ReadInt32();
            var memory = FloatVector.ReadFrom(reader);
            var activation = _Hydrate(factory, reader);

            if (_memory == null)
                _Create(factory, inputSize, memory.Data, activation);
            else
                _memory.Data = memory;

            var Wh = _input.SearchFor("Wh") as INode;
            var Uh = _memory.SearchFor("Uh") as INode;

            Wh.ReadFrom(factory, reader);
            Uh.ReadFrom(factory, reader);
        }

        public override IEnumerable<INode> SubNodes
        {
            get
            {
                yield return _input;
                yield return _memory;
            }
        }
    }
}
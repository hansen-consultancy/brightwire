﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    /// <summary>
    /// Calculates the square root of the input
    /// </summary>
    class SquareRootOfInput : NodeBase
    {
        /// <summary>
        /// Derivative of sqrt(x) is 0.5/sqrt(x)
        /// </summary>
        class Backpropagation : SingleBackpropagationBase<SquareRootOfInput>
        {
            readonly IMatrix _output;

            public Backpropagation(SquareRootOfInput source, IMatrix output) : base(source)
            {
                _output = output;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var es = errorSignal.GetMatrix();
                using (var oneHalf = context.LinearAlgebraProvider.CreateMatrix(es.RowCount, es.ColumnCount, 0.5f))
                using (var delta = oneHalf.PointwiseDivide(_output))
                    return errorSignal.ReplaceWith(es.PointwiseMultiply(delta));
            }
        }
        public SquareRootOfInput(string name = null) : base(name)
        {
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            var output = input.Sqrt(1e-8f);
            _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, output));
        }
    }
}

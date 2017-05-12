﻿using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    class MaxPool : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase
        {
            readonly List<Dictionary<Tuple<int, int>, Tuple<int, int>>> _indexPosList;
            readonly int _columns, _rows, _newColumns, _newRows;

            public Backpropagation(List<Dictionary<Tuple<int, int>, Tuple<int, int>>> indexPosList, int columns, int rows, int newColumns, int newRows)
            {
                _indexPosList = indexPosList;
                _columns = columns;
                _rows = rows;
                _newColumns = newColumns;
                _newRows = newRows;
            }

            protected override IGraphData _Backward(IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                return errorSignal;
                //var matrixList = errorSignal.AsIndexable().Columns.Select(v => v.ToArray()).ToList();
                //var lap = context.LinearAlgebraProvider;

                //var newMatrixList = new List<IMatrix>();
                //Tuple<int, int> newIndex;
                //for (var i = 0; i < matrixList.Count; i++) {
                //    var matrix = matrixList[i];
                //    var table = _indexPosList[i];

                //    newMatrixList.Add(lap.Create(_rows, _columns, (x, y) => {
                //        if (table.TryGetValue(Tuple.Create(x, y), out newIndex)) {
                //            var newIndex2 = newIndex.Item1 * _newRows + newIndex.Item2;
                //            return matrix[newIndex2];
                //        }
                //        return 0f;
                //    }));
                //}
                //using (var tensor = lap.CreateTensor(newMatrixList)) {
                //    var ret = tensor.ConvertToMatrix();
                //    foreach (var item in newMatrixList)
                //        item.Dispose();
                //    return ret;
                //}
            }
        }
        readonly int _width, _height, _stride;

        public MaxPool(int width, int height, int stride, string name = null) : base(name)
        {
            _width = width;
            _height = height;
            _stride = stride;
        }

        public override void ExecuteForward(IContext context)
        {
            var tensor = context.Data.GetTensor();
            var indexPosList = new List<Dictionary<Tuple<int, int>, Tuple<int, int>>>();
            var output = tensor.MaxPool(_width, _height, _stride, indexPosList);

            var graphData = new TensorGraphData(output);
            _AddNextGraphAction(context, graphData, () => new Backpropagation(indexPosList, tensor.ColumnCount, tensor.RowCount, output.ColumnCount, output.RowCount));
        }
    }
}
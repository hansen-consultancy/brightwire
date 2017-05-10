﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace BrightWire.ExecutionGraph.Engine
{
    class LearningContext : ILearningContext
    {
        readonly ILinearAlgebraProvider _lap;
        readonly List<(IMatrix Error, Action<IMatrix> Updater)> _layerUpdate = new List<(IMatrix, Action<IMatrix>)>();
        readonly bool _calculateTrainingError, _deferUpdates;
        readonly Stopwatch _timer = new Stopwatch();
        float _learningRate;
        int _batchSize, _rowCount = 0, _currentEpoch = 0;
        XmlWriter _writer;
        StringBuilder _log;

        public LearningContext(ILinearAlgebraProvider lap, float learningRate, int batchSize, bool calculateTrainingError, bool deferUpdates)
        {
            _lap = lap;
            _calculateTrainingError = calculateTrainingError;
            _learningRate = learningRate;
            _batchSize = batchSize;
            _deferUpdates = deferUpdates;
        }

        public ILinearAlgebraProvider LinearAlgebraProvider { get { return _lap; } }
        public int RowCount { get { return _rowCount; } }
        public int CurrentEpoch { get { return _currentEpoch; } }
        public float LearningRate { get { return _learningRate; } }
        public int BatchSize { get { return _batchSize; } }
        public bool CalculateTrainingError { get { return _calculateTrainingError; } }
        public long EpochMilliseconds { get { return _timer.ElapsedMilliseconds; } }
        public double EpochSeconds { get { return EpochMilliseconds / 1000.0; } }
        public void ClearLog() => _log?.Clear();
        public bool LogMatrixValues { get; set; }

        public void Store(IMatrix error, Action<IMatrix> updater)
        {
            if (_deferUpdates)
                _layerUpdate.Add((error, updater));
            else
                updater(error);
        }

        public bool EnableLogging
        {
            get { return _writer != null; }
            set
            {
                if(value) {
                    _log = new StringBuilder();
                    var settings = new XmlWriterSettings {
                        ConformanceLevel = ConformanceLevel.Auto
                    };
                    _writer = XmlWriter.Create(_log, settings);
                }else {
                    _writer = null;
                }
            }
        }

        public string CurrentLogXml
        {
            get
            {
                _writer?.Flush();
                return _log?.ToString();
            }
        }

        public void Log(Action<XmlWriter> callback)
        {
            if (_writer != null)
                callback(_writer);
        }

        public void Log(string name, IMatrix matrix)
        {
            if (LogMatrixValues && matrix != null) {
                Log(writer => {
                    if(name != null)
                        writer.WriteStartElement(name);
                    writer.WriteRaw(matrix.AsIndexable().AsXml);
                    if(name != null)
                        writer.WriteEndElement();
                });
            }
        }

        public void Log(string name, int channel, int id, IMatrix input, IMatrix output, Action<XmlWriter> callback = null)
        {
            Log(writer => {
                writer.WriteStartElement(name);
                writer.WriteAttributeString("id", id.ToString());
                writer.WriteAttributeString("channel", channel.ToString());

                Log("input", input);
                Log("output", output);

                callback?.Invoke(writer);
                writer.WriteEndElement();
            });
        }

        public void StartEpoch()
        {
            ++_currentEpoch;
            _rowCount = 0;
            _timer.Restart();
        }

        public void SetRowCount(int rowCount)
        {
            _rowCount = rowCount;
        }

        public void EndEpoch()
        {
            ApplyUpdates();
            _timer.Stop();
            _rowCount = 0;
        }

        public void ApplyUpdates()
        {
            foreach(var item in _layerUpdate)
                item.Updater(item.Error);
            _layerUpdate.Clear();
        }
    }
}
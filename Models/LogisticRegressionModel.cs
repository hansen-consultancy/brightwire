﻿using BrightWire.Linear;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models
{
    [ProtoContract]
    public class LogisticRegressionModel
    {
        [ProtoMember(1)]
        public FloatArray Theta { get; set; }

        public ILogisticRegressionPredictor CreatePredictor(ILinearAlgebraProvider lap)
        {
            return new LogisticRegressionPredictor(lap, lap.Create(Theta.Data));
        }
    }
}

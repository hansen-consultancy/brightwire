﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    class RmsProp : AdaGrad
    {
        protected readonly float _decayRate;

        public RmsProp(float decayRate, IMatrix cache, IGradientDescentOptimisation updater) : base(cache, updater)
        {
            _decayRate = decayRate;
        }

        public override void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            using (var deltaSquared = delta.PointwiseMultiply(delta)) {
                _cache.AddInPlace(deltaSquared, _decayRate, 1 - _decayRate);

                using (var cachedSqrt = _cache.Sqrt(1e-8f))
                using (var delta2 = delta.PointwiseDivide(cachedSqrt)) {
                    _updater.Update(source, delta2, context);
                }
            }
        }
    }
}
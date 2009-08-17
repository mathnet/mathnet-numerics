using System.Collections.Generic;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using LinearAlgebra.Double;

    public class DenseVectorTests : VectorTests
    {
        protected override Vector CreateVector(int size)
        {
            return new DenseVector(size);
        }

        protected override Vector CreateVector(IList<double> data)
        {
            var vector = new DenseVector(data.Count);
            for(var index = 0; index < data.Count; index++)
            {
                vector[index] = data[index];
            }

            return vector;
        }
    }
}

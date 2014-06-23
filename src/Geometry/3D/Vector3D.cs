namespace MathNet.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using Units;

    [Serializable]
    public struct Vector3D : IXmlSerializable, IEquatable<Vector3D>, IEquatable<UnitVector3D>, IFormattable
    {
        #region Common

        /// <summary>
        /// Default is serializing as attributes, set to true for elements
        /// </summary>
        public bool SerializeAsElements;

        /// <summary>
        /// Using public fields cos: http://blogs.msdn.com/b/ricom/archive/2006/08/31/performance-quiz-11-ten-questions-on-value-based-programming.aspx
        /// </summary>
        public readonly double X;

        /// <summary>
        /// Using public fields cos: http://blogs.msdn.com/b/ricom/archive/2006/08/31/performance-quiz-11-ten-questions-on-value-based-programming.aspx
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// Using public fields cos: http://blogs.msdn.com/b/ricom/archive/2006/08/31/performance-quiz-11-ten-questions-on-value-based-programming.aspx
        /// </summary>
        public readonly double Z;

        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            SerializeAsElements = false;
        }

        public Vector3D(IEnumerable<double> data)
            : this(data.ToArray())
        {
        }

        public Vector3D(double[] data)
            : this(data[0], data[1], data[2])
        {
            if (data.Length != 3)
            {
                throw new ArgumentException("Size must be 3");
            }
        }

        /// <summary>
        /// A vector orthogonbal to this
        /// </summary>
        public UnitVector3D Orthogonal
        {
            get
            {
                if (-X - Y > 0.1)
                {
                    return new UnitVector3D(Z, Z, -X - Y);
                }
                return new UnitVector3D(-Y - Z, X, X);
            }
        }

        /// <summary>
        /// Returns a copy of the inner dense vector
        /// </summary>
        public DenseVector ToDenseVector()
        {
            return new DenseVector(new[] { X, Y, Z });
        }

        public static Vector3D Parse(string value)
        {
            var doubles = Parser.ParseItem3D(value);
            return new Vector3D(doubles);
        }

        public static bool operator ==(Vector3D left, Vector3D right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Vector3D left, Vector3D right)
        {
            return !Equals(left, right);
        }

        [Obsolete("Not sure this is nice")]
        public static Vector<double> operator *(Matrix<double> left, Vector3D right)
        {
            return left * right.ToDenseVector();
        }

        [Obsolete("Not sure this is nice")]
        public static Vector<double> operator *(Vector3D left, Matrix<double> right)
        {
            return left.ToDenseVector() * right;
        }

        public static double operator *(Vector3D left, Vector3D right)
        {
            return left.DotProduct(right);
        }

        public override string ToString()
        {
            return ToString(null, CultureInfo.InvariantCulture);
        }

        public string ToString(IFormatProvider provider)
        {
            return ToString(null, provider);
        }

        public string ToString(string format, IFormatProvider provider = null)
        {
            var numberFormatInfo = provider != null ? NumberFormatInfo.GetInstance(provider) : CultureInfo.InvariantCulture.NumberFormat;
            string separator = numberFormatInfo.NumberDecimalSeparator == "," ? ";" : ",";
            return string.Format("({1}{0} {2}{0} {3})",
                separator,
                X.ToString(format, numberFormatInfo),
                Y.ToString(format, numberFormatInfo),
                Z.ToString(format, numberFormatInfo));
        }

        public bool Equals(Vector3D other)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return X == other.X && Y == other.Y && Z == other.Z;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public bool Equals(UnitVector3D other)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return X == other.X && Y == other.Y && Z == other.Z;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public bool Equals(Vector3D other, double tolerance)
        {
            if (tolerance < 0)
            {
                throw new ArgumentException("epsilon < 0");
            }

            return Math.Abs(other.X - X) < tolerance &&
                   Math.Abs(other.Y - Y) < tolerance &&
                   Math.Abs(other.Z - Z) < tolerance;
        }

        public bool Equals(UnitVector3D other, double tolerance)
        {
            if (tolerance < 0)
            {
                throw new ArgumentException("epsilon < 0");
            }

            return Math.Abs(other.X - X) < tolerance &&
                   Math.Abs(other.Y - Y) < tolerance &&
                   Math.Abs(other.Z - Z) < tolerance;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is UnitVector3D && Equals((UnitVector3D)obj) || obj is Vector3D && Equals((Vector3D)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
        /// </returns>
        public XmlSchema GetSchema() { return null; }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. </param>
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var e = (XElement)XNode.ReadFrom(reader);
            // Hacking set readonly fields here, can't think of a cleaner workaround
            XmlExt.SetReadonlyField(ref this, x => x.X, XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("X")));
            XmlExt.SetReadonlyField(ref this, x => x.Y, XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("Y")));
            XmlExt.SetReadonlyField(ref this, x => x.Z, XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("Z")));
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
        public void WriteXml(XmlWriter writer)
        {
            if (SerializeAsElements)
            {
                writer.WriteElementString("X", X.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Y", Y.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Z", Z.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteAttribute("X", X);
                writer.WriteAttribute("Y", Y);
                writer.WriteAttribute("Z", Z);
            }
        }

        public static Vector3D ReadFrom(XmlReader reader)
        {
            var v = new Vector3D();
            v.ReadXml(reader);
            return v;
        }

        #endregion Common

        public static Vector3D NaN
        {
            get
            {
                return new Vector3D(double.NaN, double.NaN, double.NaN);
            }
        }

        internal Matrix<double> CrossProductMatrix
        {
            get
            {
                var matrix = new DenseMatrix(3, 3);
                matrix[0, 1] = -Z;
                matrix[0, 2] = Y;

                matrix[1, 0] = Z;
                matrix[1, 2] = -X;

                matrix[2, 0] = -Y;
                matrix[2, 1] = X;
                return matrix;
            }
        }

        /// <summary>
        /// The length of the vector not the count of elements
        /// </summary>
        public double Length
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        public static Vector3D operator +(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3D operator -(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3D operator -(Vector3D v)
        {
            return v.Negate();
        }

        public static Vector3D operator *(double d, Vector3D v)
        {
            return new Vector3D(d * v.X, d * v.Y, d * v.Z);
        }

        // Commented out because the d * v reads nicer than v *d 
        //public static Vector3D operator *(Vector3D v,double d)
        //{
        //    return d*v;
        //}

        public static Vector3D operator /(Vector3D v, double d)
        {
            return new Vector3D(v.X / d, v.Y / d, v.Z / d);
        }

        ////public static explicit operator Vector3D(System.Windows.Media.Media3D.Vector3D v)
        ////{
        ////    return new Vector3D(v.X, v.Y, v.Z);
        ////}

        ////public static explicit operator System.Windows.Media.Media3D.Vector3D(Vector3D p)
        ////{
        ////    return new System.Windows.Media.Media3D.Vector3D(p.X, p.Y, p.Z);
        ////}

        public UnitVector3D Normalize()
        {
            return new UnitVector3D(X, Y, Z);
        }

        public Vector3D ScaleBy(double scaleFactor)
        {
            return scaleFactor * this;
        }

        public Ray3D ProjectOn(Plane planeToProjectOn)
        {
            return planeToProjectOn.Project(this);
        }

        public Vector3D ProjectOn(UnitVector3D uv)
        {
            double pd = DotProduct(uv);
            return pd * this;
        }

        public bool IsParallelTo(Vector3D othervector, double tolerance = 1e-6)
        {
            var @this = Normalize();
            var other = othervector.Normalize();
            var dp = Math.Abs(@this.DotProduct(other));
            return Math.Abs(1 - dp) < tolerance;
        }

        public bool IsParallelTo(UnitVector3D othervector, double tolerance = 1e-6)
        {
            var @this = Normalize();
            var dp = Math.Abs(@this.DotProduct(othervector));
            return Math.Abs(1 - dp) < tolerance;
        }

        public bool IsPerpendicularTo(Vector3D othervector, double tolerance = 1e-6)
        {
            var @this = Normalize();
            var other = othervector.Normalize();
            return Math.Abs(@this.DotProduct(other)) < tolerance;
        }

        public bool IsPerpendicularTo(UnitVector3D othervector, double tolerance = 1e-6)
        {
            var @this = Normalize();
            return Math.Abs(@this.DotProduct(othervector)) < tolerance;
        }

        public Vector3D Negate()
        {
            return new Vector3D(-1 * X, -1 * Y, -1 * Z);
        }

        public double DotProduct(Vector3D v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }

        public double DotProduct(UnitVector3D v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }

        [Obsolete("Use - instead")]
        public Vector3D Subtract(Vector3D v)
        {
            return new Vector3D(X - v.X, Y - v.Y, Z - v.Z);
        }

        [Obsolete("Use + instead")]
        public Vector3D Add(Vector3D v)
        {
            return new Vector3D(X + v.X, Y + v.Y, Z + v.Z);
        }

        public Vector3D CrossProduct(Vector3D inVector3D)
        {
            var x = Y * inVector3D.Z - Z * inVector3D.Y;
            var y = Z * inVector3D.X - X * inVector3D.Z;
            var z = X * inVector3D.Y - Y * inVector3D.X;
            var v = new Vector3D(x, y, z);
            return v;
        }

        public Vector3D CrossProduct(UnitVector3D inVector3D)
        {
            var x = Y * inVector3D.Z - Z * inVector3D.Y;
            var y = Z * inVector3D.X - X * inVector3D.Z;
            var z = X * inVector3D.Y - Y * inVector3D.X;
            var v = new Vector3D(x, y, z);
            return v;
        }

        public Matrix<double> GetUnitTensorProduct()
        {
            //unitTensorProduct:matrix([ux^2,ux*uy,ux*uz],[ux*uy,uy^2,uy*uz],[ux*uz,uy*uz,uz^2]),
            var matrix = new DenseMatrix(3, 3);
            matrix[0, 0] = X * X;
            matrix[0, 1] = X * Y;
            matrix[0, 2] = X * Z;

            matrix[1, 0] = X * Y;
            matrix[1, 1] = Y * Y;
            matrix[1, 2] = Y * Z;

            matrix[2, 0] = X * Z;
            matrix[2, 1] = Y * Z;
            matrix[2, 2] = Z * Z;
            return matrix;
        }

        /// <summary>
        /// Returns signed angle in radians 
        /// </summary>
        /// <param name="toVector3D">The fromVector3D to calculate the signed angle to </param>
        /// <param name="aboutVector3D">The fromVector3D around which to rotate </param>
        public Angle SignedAngleTo(Vector3D toVector3D, UnitVector3D aboutVector3D)
        {
            return Normalize().SignedAngleTo(toVector3D.Normalize(), aboutVector3D);
        }

        public Angle SignedAngleTo(UnitVector3D toVector3D, UnitVector3D aboutVector3D)
        {
            return Normalize().SignedAngleTo(toVector3D, aboutVector3D);
        }

        public Angle AngleTo(Vector3D v)
        {
            return Normalize().AngleTo(v.Normalize());
        }

        public Angle AngleTo(UnitVector3D v)
        {
            return Normalize().AngleTo(v);
        }

        public Vector3D Rotate<T>(UnitVector3D aboutVector, double angle, T angleUnit) where T : IAngleUnit
        {
            return Rotate(aboutVector, Angle.From(angle, angleUnit));
        }

        public Vector3D Rotate(Vector3D aboutVector, Angle angle)
        {
            return Rotate(aboutVector.Normalize(), angle);
        }

        public Vector3D Rotate(UnitVector3D aboutVector, Angle angle)
        {
            var cs = CoordinateSystem.Rotation(angle, aboutVector);
            return cs.Transform(this);
        }

        public Point3D ToPoint3D()
        {
            return new Point3D(X, Y, Z);
        }

        public Vector3D TransformBy(CoordinateSystem coordinateSystem)
        {
            return coordinateSystem.Transform(this);
        }

        public Vector3D TransformBy(Matrix<double> m)
        {
            return new Vector3D(m.Multiply(this.ToDenseVector()));
        }
    }
}

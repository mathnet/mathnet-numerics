MATLAB Level-5 Mat Files
========================

Level-5 MATLAB Mat files are popular as binary file container for storing one or more matrices.
Math.NET Numerics provides basic support for such Mat files with the **MathNet.Numerics.Data.Matlab** package,
which is available on NuGet as separate package and not included in the basic distribution.


Reading matrices from a MATLAB file
-----------------------------------

The `MatlabReader` class provides static functions to list all matrices stored in a MAT file or stream,
and to read them individually as Math.NET matrices:

    [lang=csharp]
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.Data.Matlab;

    // read the first matrix as double
    Matrix<double> m = MatlabReader.Read<double>("collection.mat");

    // read a specific matrix named "vd":
    Matrix<double> m = MatlabReader.Read<double>("collection.mat", "vd");

    // we can also choose to convert to a different type:
    Matrix<Complex> m = MatlabReader.Read<Complex>("collection.mat");

    // read all matrices of a file by name into a dictionary
    Dictionary<string,Matrix<double>> ms =
        MatlabReader.ReadAll<double>("collection.mat");

    // read the matrices named "Ad" and "vd" into a dictionary
    var ms = MatlabReader.ReadAll<double>("collection.mat", "vd", "Ad");

Alternatively the reader can list all matrices of a file into named data elements,
which can then be read into matrices individually. This is useful e.g. if we need to
read some of the matrices to a different type:

    [lang=csharp]
    List<MatlabMatrix> ms = MatlabReader.List("collection.mat");
    Matrix<double> Ad = MatlabReader.Unpack<double>(ms.Find(m => m.Name == "Ad"));
    Matrix<float> vd = MatlabReader.Unpack<float>(ms.Find(m => m.Name == "vd"));


Writing matrices to a MATLAB file
---------------------------------

The dual to the reader above is the `MatlabWriter` class that can serialize matrices
to a MATLAB file or stream. Like the reader, the writer can use `MatlabMatrix` data elements
to compose packed matrices into a file. Each matrix has a name which must not contain spaces.

    [lang=csharp]
    var matrices = new List<MatlabMatrix>();
    m.Add(MatlabWriter.Pack(myFirstMatrix, "m1");
    m.Add(MatlabWriter.Pack(mySecondMatrix, "m2");
    MatlabWrier.Store("file.mat", matrices);

But there are also direct routines if only a single matrix or matrices of all the same data type
are to be stored in a file:

    [lang=csharp]
    // write a single matrix "myMatrix" and name it "m1".
    MatlabWriter.Write("file.mat", myMatrix, "m1");

    // write multiple matrices, from a list of matrices and a list of their names:
    MatlabWriter.Write("file.mat", new[] { m1, m2 }, new[] { "m1", "m2" });

    // write a dictionary of matrices:
    var dict = new Dictionary<string, Matrix<double>>();
    dict.Add("m1", m1);
    dict.Add("m2", m2);
    MatlabWriter.Write("file.mat", dict);


Alternatives
------------

The data extension packages also offer other ways to serialize a matrix to a text file.
Among others:

* [Delimited Text Files (CSV & TSV)](CSV.html)
* [NIST MatrixMarket text files](MatrixMarket.html)

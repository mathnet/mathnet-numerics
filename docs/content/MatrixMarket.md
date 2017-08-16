NIST MatrixMarket Text Files
============================

MatrixMarket is both a [vast repository of test data](http://math.nist.gov/MatrixMarket/)
and a text-based [exchange file format](http://math.nist.gov/MatrixMarket/formats.html) provided by NIST.
Being text-based makes it convenient to deal with and program against, and also works well with versioning
tools like [Git](https://www.git-scm.com/). But other than [CSV](CSV.html) it can also store sparse matrices in a compact way.

Math.NET Numerics provides basic support for MatrixMarket files with the **MathNet.Numerics.Data.Text** package,
which is available on NuGet as separate package and not included in the basic distribution.


Reading a matrix from a MatrixMarket file
-----------------------------------------

The `MatrixMarketReader` class provides static functions to read a matrix or a vector from a file or string.
It can read from:

* **TextReader**: If you have your delimited data already in memory in a string,
  you can use this method using a StringReader.
* **Stream**: read directly from a stream, e.g. a MemoryStream, FileStream or NetworkStream.
* **File Path (string)**: read from a file, specified by the file system path. Optionally GZip compressed.

All these functions expect the data type of the matrix to be generated as generic type argument.
Only Double, Single, Complex and Complex32 are supported.

Example:

    [lang=csharp]
    using MathNet.Numerics.Data.Text;

    Matrix<double> matrix = MatrixMarketReader.ReadMatrix<double>("fidap007.mtx");


Writing a matrix to a MatrixMarket file
---------------------------------------

The dual to the reader above is the `MatrixMarketWriter` class that can serialize a matrix or vector
to a MatrixMarket text file, stream or TextWriter.

Example:

    [lang=csharp]
    MatrixMarketWriter.WriteMatrix("matrix.mtx", m);


Alternatives
------------

The data extension packages also offer other ways to serialize a matrix to a binary stream or file.
Among others:

* [Delimited Text Files (CSV & TSV)](CSV.html)
* [MATLAB Level-5 Mat files](MatlabFiles.html)

(*** hide ***)
#I "../../out/lib/net40"
#r "MathNet.Numerics.dll"
#r "MathNet.Numerics.FSharp.dll"

(**
Delimited Text Files (CSV & TSV)
================================

Likely the most common text file format for tabular data, CSV or TSV files store data as text
with one line per row and each column value separated by a comma or tabulator.
Such delimited text files are supported by virtually all software that deals with
tabular data in some way.

Example:

    [lang=text]
    A,B,C
    0.5,0.6,98.0
    2.0,3.4,5.3

Unfortunately there are multiple ways to separate values and to deal with spacing around them.
CSV files typically use commas like in the example and TSV typically tabulators, but other separators
like semicolons or colons are common as well. Delimited files can optionally have headers on the first line
to describe the columns below (A, B and C in the example).

Math.NET Numerics provides basic support for delimited files with the **MathNet.Numerics.Data.Text** package,
which is available on NuGet as separate package and not included in the basic distribution.


Reading a delimited file as matrix
----------------------------------

The `DelimitedReader` class provides static functions to read a matrix in delimited form:

* **Read**: read from a TextReader. If you have your delimited data already in memory in a string,
  you can use this method using a StringReader.
* **ReadStream**: read directly from a stream, e.g. a MemoryStream, FileStream or NetworkStream.
* **ReadFile**: read from a file, specified by the file system path.

All these functions are generic in the data type of the matrix to be generated.
Only Double, Single, Complex and Complex32 are supported (C#: double, float; F#: float, float32).

Example:

    [lang=csharp]
    Matrix<double> matrix = DelimitedReader.Read<double>("data.csv", false, ",", true);

In addition to the file path, stream or text reader as first argument, the behavior can be customized
with the following optional arguments:

* **sparse**: Whether the the returned matrix should be constructed as sparse (true) or dense (false).  
  Default: false.
* **delimiter**: Number delimiter between numbers of the same line. Supports Regex groups.  
  Default: `\s` (white space).
* **hasHeaders**: Whether the first row contains column headers or not.  
  Default: false.
* **formatProvider**: The culture to use. It is often a good idea to use InvariantCulture here,
  to make the format invariant from the local culture.  
  Default: null.


Writing a matrix as delimited file
----------------------------------

The dual to the delimiter above is the `DelimitedWriter` class that can serialize a matrix
to a delimited text file, stream or TextWriter.

The static Write functions accept the following optional arguments, in addition to the matrix and target
as first two required arguments.

* **delimiter**: Number delimiter to write between numbers of the same line.  
  Default: `,` (white space).
* **columnHeaders**: list of column header strings, or null if no headers should be written.  
  Default: null.
* **format**: The number format to use on each element. Default: null.
* **formatProvider**: The culture to use. It is often a good idea to use InvariantCulture here,
  to make the format invariant from the local culture.  
  Default: null.

Example:

    [lang=csharp]
    DelimitedWriter.WriteFile(matrix, "data.csv");


Alternatives
------------

The data extension packages also offer other ways to serialize a matrix to a binary stream or file.
Among others:

* NIST MatrixMarket text files
* MATLAB mat files

*)

Delimited Text Files (CSV & TSV)
================================

Likely the most common file format for tabular data, delimited files like CSV store data as text
with one line per row and values within rows separated by a comma.
Such text files are supported by virtually all software that deals with tabular data.

Example:

    [lang=text]
    A,B,C
    0.5,0.6,98.0
    2.0,3.4,5.3

Unfortunately there is no universal standard on what character is used as separator and how
individual values are formatted and escaped. CSV files traditionally use a comma as separator, but this
causes problems e.g. in Germany where the comma is used as decimal point in numbers. The tabulator
proves to be a useful alternative, usually denoted by using the TSV extension instead of CSV.
Other separators like semicolons or colons are common as well.

Math.NET Numerics provides basic support for delimited files with the **MathNet.Numerics.Data.Text** package,
which is available on NuGet as separate package and not included in the basic distribution.


Reading a matrix from a delimited file
--------------------------------------

The `DelimitedReader` class provides static functions to read a matrix from a file or string in delimited form.
It can read from:

* **TextReader**: If you have your delimited data already in memory in a string,
  you can use this method using a StringReader.
* **Stream**: read directly from a stream, e.g. a MemoryStream, FileStream or NetworkStream.
* **File Path (string)**: read from a file, specified by the file system path.

All these functions expect the data type of the matrix to be generated as generic type argument.
Only Double, Single, Complex and Complex32 are supported.

Example:

    [lang=csharp]
    using MathNet.Numerics.Data.Text;

    Matrix<double> matrix = DelimitedReader.Read<double>("data.csv", false, ",", true);

Unfortunately the lack of standard means that the parsing logic needs to be parametrized accordingly.
There are ways to automatically profile the provided file to find out the correct parameters automatically,
but for simplicity the Read functions expects those parameters explicitly as optional arguments:

* **sparse**: Whether the the returned matrix should be constructed as sparse (true) or dense (false).  
  Default: false.
* **delimiter**: Number delimiter between numbers of the same line. Supports Regex groups.  
  Default: `\s` (white space).
* **hasHeaders**: Whether the first row contains column headers or not. If true, the first line will be skipped.  
  Default: false.
* **formatProvider**: The culture to use. It is often a good idea to use InvariantCulture,
  to make the format independent from the local culture.  
  Default: null.


Writing a matrix to a delimited file
------------------------------------

The dual to the reader above is the `DelimitedWriter` class that can serialize a matrix
to a delimited text file, stream or TextWriter.

The static Write functions accept the following optional arguments to control the output format:

* **delimiter**: Number delimiter to write between numbers of the same line.  
  Default: `\t` (tabulator).
* **columnHeaders**: list of column header strings, or null if no headers should be written.  
  Default: null.
* **format**: The number format to use on each element, similar to what can be provided to Double.ToString().  
  Default: null.
* **formatProvider**: The culture to use. It is often a good idea to use InvariantCulture,
  to make the format independent from the local culture.  
  Default: null.

Example:

    [lang=csharp]
    DelimitedWriter.Write("data.csv", matrix, ",");


Alternatives
------------

The data extension packages also offer other ways to serialize a matrix to a binary stream or file.
Among others:

* [NIST MatrixMarket text files](MatrixMarket.html)
* [MATLAB Level-5 Mat files](MatlabFiles.html)

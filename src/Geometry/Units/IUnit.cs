namespace MathNet.Geometry.Units
{
    public interface IUnit
    {
        double Conversionfactor { get; }
       
        string ShortName { get; }
    }
}
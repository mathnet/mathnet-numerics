namespace MathNet.Geometry.Units
{
    public static class UnitConverter
    {
        public static double ConvertTo<TUnit>(double value, TUnit toUnit)
            where TUnit : IUnit
        {
            return value / toUnit.Conversionfactor;
        }

        public static double ConvertFrom<TUnit>(double value, TUnit toUnit)
            where TUnit : IUnit
        {
            return value * toUnit.Conversionfactor;
        }
    }
}

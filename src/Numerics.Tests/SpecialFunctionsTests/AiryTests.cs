using NUnit.Framework;
using Complex = System.Numerics.Complex;

namespace MathNet.Numerics.Tests.SpecialFunctionsTests
{
    /// <summary>
    /// Airy functions tests.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class AiryTests
    {
        [TestCase(0.0, 0.0, 0.35502805388781723926, 0.0, 14)]
        [TestCase(1.0, 0.0, 0.13529241631288141552, 0.0, 14)]
        [TestCase(-1.0, 0.0, 0.53556088329235211880, 0.0, 14)]
        [TestCase(0.0, 1.0, 0.33149330543214118898, -0.31744985896844377348, 14)]
        [TestCase(0.0, -1.0, 0.33149330543214118898, 0.31744985896844377348, 14)]
        [TestCase(1.0, 1.0, 0.060458308371838149197, -0.15188956587718140235, 13)]
        [TestCase(-1.0, -1.0, 0.82211742655527259396, 0.11996634266442434389, 14)]
        [TestCase(10.0, 5.0, -7.0165968356580205348E-10, 2.7637938765570892385E-10, 14)]
        [TestCase(-10.0, -5.0, 1.2329510105552886439E6, 502435.45036313325712, 13)]
        [TestCase(100.0, 100.0, 2.9099582462207032076E-188, 2.3530135917061787560E-188, 14)]
        [TestCase(32.0, -64.0, 5.3014568355995704254E14, 2.0523039181737934724E13, 11)]
        public void AiryAiApprox(double zr, double zi, double cyr, double cyi, int decimalPlaces)
        {
            AssertHelpers.AlmostEqualRelative(
                new Complex(cyr, cyi),
                SpecialFunctions.AiryAi(new Complex(zr, zi)),
                decimalPlaces
                );
        }

        [TestCase(0.0, 0.0, -0.25881940379280679841, 0.0, 14)]
        [TestCase(1.0, 0.0, -0.15914744129679321279, 0.0, 14)]
        [TestCase(-1.0, 0.0, -0.010160567116645209395, 0.0, 13)]
        [TestCase(0.0, 1.0, -0.43249265984180709931, 0.098047856229243232384, 14)]
        [TestCase(0.0, -1.0, -0.43249265984180709931, -0.098047856229243232384, 14)]
        [TestCase(1.0, 1.0, -0.13062795349964751771, 0.16306759644932391574, 13)]
        [TestCase(-1.0, -1.0, -0.37906047922683349624, 0.60450013086224607164, 14)]
        [TestCase(10.0, 5.0, 2.5069533633744176669E-9, -3.7264734152221837760E-10, 13)]
        [TestCase(-10.0, -5.0, -2.5522092133303029354E6, 3.6244486331968648174E6, 13)]
        [TestCase(100.0, 100.0, -2.1269500702792576318E-187, -3.9094417620496355697E-187, 14)]
        [TestCase(32.0, -64.0, -3.9067669879175239067E15, 2.2082697395233711387E15, 12)]
        public void AiryAiPrimeApprox(double zr, double zi, double cyr, double cyi, int decimalPlaces)
        {
            AssertHelpers.AlmostEqualRelative(
                new Complex(cyr, cyi),
                SpecialFunctions.AiryAiPrime(new Complex(zr, zi)),
                decimalPlaces
                );
        }

        [TestCase(0.0, 0.0, 0.61492662744600073515, 0.0, 14)]
        [TestCase(1.0, 0.0, 1.2074235949528712594, 0.0, 14)]
        [TestCase(-1.0, 0.0, 0.10399738949694461189, 0.0, 14)]
        [TestCase(0.0, 1.0, 0.64885820833039494458, 0.34495863476804837025, 14)]
        [TestCase(0.0, -1.0, 0.64885820833039494458, -0.34495863476804837025, 14)]
        [TestCase(1.0, 1.0, 0.71665807338276843179, 0.61988929040084476435, 13)]
        [TestCase(-1.0, -1.0, 0.21429040153487357398, -0.67391692372270520960, 13)]
        [TestCase(10.0, 5.0, -6.2471332619252646778E7, -9.0137631732829873100E6, 13)]
        [TestCase(-10.0, -5.0, 502435.45036315399060, -1.2329510105552595201E6, 13)]
        [TestCase(100.0, 100.0, 1.7086751714463652039E185, -3.1416590020830804578E185, 11)]
        [TestCase(32.0, -64.0, 2.0523039181737934724E13, -5.3014568355995704254E14, 11)]
        public void AiryBiApprox(double zr, double zi, double cyr, double cyi, int decimalPlaces)
        {
            AssertHelpers.AlmostEqualRelative(
                new Complex(cyr, cyi),
                SpecialFunctions.AiryBi(new Complex(zr, zi)),
                decimalPlaces
                );
        }

        [TestCase(0.0, 0.0, 0.44828835735382635791, 0.0, 14)]
        [TestCase(1.0, 0.0, 0.93243593339277563296, 0.0, 14)]
        [TestCase(-1.0, 0.0, 0.59237562642279235082, 0.0, 14)]
        [TestCase(0.0, 1.0, 0.13502664671081897270, -0.12883738678125487904, 14)]
        [TestCase(0.0, -1.0, 0.13502664671081897270, 0.12883738678125487904, 14)]
        [TestCase(1.0, 1.0, 0.075662844174965992918, 0.78370099878545527505, 13)]
        [TestCase(-1.0, -1.0, 0.83447348852278263690, 0.34652606326682852855, 14)]
        [TestCase(10.0, 5.0, -1.9502117576621140236E8, -7.7790596402473222951E7, 14)]
        [TestCase(-10.0, -5.0, 3.6244486331969762244E6, 2.5522092133302581994E6, 13)]
        [TestCase(100.0, 100.0, 3.3072107798533888664E186, -2.6734837736904900245E186, 12)]
        [TestCase(32.0, -64.0, 2.2082697395233711387E15, 3.9067669879175239067E15, 12)]
        public void AiryBiPrimeApprox(double zr, double zi, double cyr, double cyi, int decimalPlaces)
        {
            AssertHelpers.AlmostEqualRelative(
                new Complex(cyr, cyi),
                SpecialFunctions.AiryBiPrime(new Complex(zr, zi)),
                decimalPlaces
                );
        }
    }
}

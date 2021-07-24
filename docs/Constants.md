Constants
=========

Math.NET Numerics contains a set of often used mathematical and scientific
constants. Mathematical and defined scientific constants are as accurate as
double precision allows, while measured constants are implemented according
to 2007 CODATA.

All constants are defined as static constant fields of the `Constants` class.


SI Unit Scaling
---------------

Constant | Factor    | Constant | Factor
-------- | --------- | -------- | ----------
Deca     | $10^{1}$  | Deci     | $10^{-1}$
Hecto    | $10^{2}$  | Centi    | $10^{-2}$
Kilo     | $10^{3}$  | Milli    | $10^{-3}$
Mega     | $10^{6}$  | Micro    | $10^{-6}$
Giga     | $10^{9}$  | Nano     | $10^{-9}$
Tera     | $10^{12}$ | Pico     | $10^{-12}$
Peta     | $10^{15}$ | Femto    | $10^{-15}$
Exa      | $10^{18}$ | Atto     | $10^{-18}$
Zetta    | $10^{21}$ | Zepto    | $10^{-21}$
Yotta    | $10^{24}$ | Yocto    | $10^{-24}$


Mathematical Constants
----------------------

Constant          | Definition                | Value (Rounded)
----------------- | ------------------------- | ---------------
E                 | $e$                       | 2.7182818284590452354
Log2E             | $\log_2{e}$               | 1.4426950408889634074
Log10E            | $\log_{10}{e}$            | 0.4342944819032518277
Ln2               | $\log_e{2}$               | 0.6931471805599453094
Ln10              | $\log_e{10}$              | 2.3025850929940456840
LnPi              | $\log_e{\pi}$             | 1.1447298858494001741
Ln2PiOver2        | $\frac{1}{2}\log_e{2\pi}$ | 0.9189385332046727418
InvE              | $\frac{1}{e}$             | 0.3678794411714423216
SqrtE             | $\sqrt{e}$                | 1.6487212707001281468
Sqrt2             | $\sqrt{2}$                | 1.4142135623730950488
Sqrt3             | $\sqrt{3}$                | 1.7320508075688772935
Sqrt1Over2        | $\sqrt{\frac{1}{2}} = \frac{1}{\sqrt{2}} = \frac{\sqrt{2}}{2}$ | 0.7071067811865475244
HalfSqrt3         | $\frac{1}{2}\sqrt{3}$     | 0.8660254037844386468
Pi                | $\pi$                     | 3.1415926535897932385
Pi2               | $2\pi$                    | 6.2831853071795864769
PiOver2           | $\frac{1}{2}\pi$          | 1.5707963267948966192
Pi3Over2          | $\frac{3}{2}\pi$          | 4.7123889803846898577
PiOver4           | $\frac{1}{4}\pi$          | 0.7853981633974483096
SqrtPi            | $\sqrt{\pi}$              | 1.7724538509055160273
Sqrt2Pi           | $\sqrt{2\pi}$             | 2.5066282746310005024
Sqrt2PiE          | $\sqrt{2\pi e}$           | 4.1327313541224929385
LogSqrt2Pi        | $\log_e{\sqrt{2\pi}} = \frac{1}{2}\log_e{2\pi}$ | 0.9189385332046727418
LogSqrt2PiE       | $\log_e{\sqrt{2\pi e}}$   | 1.4189385332046727418
LogTwoSqrtEOverPi | $\log_e{2\sqrt{\frac{e}{\pi}}}$ | 0.6207822376352452223
InvPi             | $\frac{1}{\pi}$           | 0.3183098861837906715
TwoInvPi          | $\frac{2}{\pi}$           | 0.6366197723675813431
InvSqrtPi         | $\frac{1}{\sqrt{\pi}}$    | 0.5641895835477562869
InvSqrt2Pi        | $\frac{1}{\sqrt{2\pi}}$   | 0.3989422804014326779
TwoInvSqrtPi      | $\frac{2}{\sqrt{2\pi}}$   | 1.1283791670955125739
TwoSqrtEOverPi    | $2\sqrt{\frac{e}{\pi}}$   | 1.8603827342052657173
Catalan           | $\beta(2) = \sum_{k=0}^{\infty}{\frac{(-1)^k}{(2k+1)^2}}$ | 0.9159655941772190151
EulerMascheroni   | $\gamma = \lim_{n\to\infty} {\sum_{k=1}^n{(\frac{1}{k} - \log_e{n})}}$ | 0.5772156649015328606
GoldenRatio       | $\frac{1+\sqrt{5}}{2}$    | 1.6180339887498948482
Glaisher          | $\exp{(\frac{1}{12} - \zeta{(-1)})}$ | 1.2824271291006226369
Khinchin          | $K_0 = \prod_{k=1}^{\infty}{(1+\frac{1}{k(k+2)})^{\log_2{k}}}$ | 2.6854520010653064453


Universal Constants
-------------------

Constant                      | Symbol  | Unit          | Value
----------------------------- | ------- | ------------- | -----
SpeedOfLight                  | $c_0$   | $\frac{m}{s}$ | 2.99792458e+8
MagneticPermeability          | $\mu_0$ | $\frac{N}{A^2}$ = $\frac{kg\,m}{A^2s^2}$ | 1.2566370614359172954e-6
ElectricPermittivity          | $\varepsilon_0$ | $\frac{F}{m}$ = $\frac{A^2s^4}{kg\,m^3}$ | 8.8541878171937079245e-12
CharacteristicImpedanceVacuum | $Z_0$   | $\Omega = \frac{kg\,m^2}{A^2s^3}$ | 376.7303134617706554682
GravitationalConstant         | $G$     | $\frac{m^3}{kg\,s^2}$ | 6.67429e-11
PlancksConstant               | $h$     | $J\,s = \frac{kg\,m^2}{s}$ | 6.62606896e-34
DiracsConstant                | $\hbar$ | $J\,s = \frac{kg\,m^2}{s}$ | 1.054571629e-34
PlancksMass                   | $m_p$   | $kg$ | 2.17644e-8
PlancksTemperature            | $T_p$   | $K$ | 1.416786e+32
PlancksLength                 | $l_p$   | $m$ | 1.616253e-35
PlancksTime                   | $t_p$   | $s$ | 5.39124e-44


Electromagnetic Constants
-------------------------

Constant            | Symbol     | Unit                              | Value
------------------- | ---------- | --------------------------------- | -----
ElementaryCharge    | $e$        | $C = A\,s$                        | 1.602176487e-19
MagneticFluxQuantum | $\Theta_0$ | $Wb = \frac{kg\,m^2}{A\,s^2}$     | 2.067833668e-15
ConductanceQuantum  | $G_0$      | $S = \frac{A^2s^3}{kg\,m^2}$      | 7.7480917005e-5
JosephsonConstant   | $K_J$      | $\frac{Hz}{V}$                    | 483597.891e+9
VonKlitzingConstant | $R_K$      | $\Omega = \frac{kg\,m^2}{A^2s^3}$ | 25812.807557
BohrMagneton        | $\mu_B$    | $\frac{J}{T}$                     | 927.400915e-26
NuclearMagneton     | $\mu_N$    | $\frac{J}{T}$                     | 5.05078324e-27


Atomic and Nuclear Constants
----------------------------

Constant              | Symbol       | Unit              | Value
--------------------- | ------------ | ----------------- | -----
FineStructureConstant | $\alpha$     | $1$               | 7.2973525376e-3
RydbergConstant       | $T_{\infty}$ | $\frac{1}{m}$     | 10973731.568528
BohrRadius            | $a_0$        | $m$               | 0.52917720859e-10
HartreeEnergy         | $E_h$        | $J$               | 4.35974394e-18
QuantumOfCirculation  |              | $\frac{m^2}{s}$   | 3.6369475199e-4
FermiCouplingConstant |              | $\frac{1}{GeV^2}$ | 1.16637e-5
WeakMixingAngle       |              |                   | 0.22256
Avogadro              |              | $\frac{1}{mol}$   | 6.0221412927e23

#### Electron

Constant                     | Unit             | Value
---------------------------- | ---------------- | -----
ElectronMass                 | $kg$             | 9.10938215e-31
ElectronMassEnergyEquivalent | $J$              | 8.18710438e-14
ElectronMolarMass            | $\frac{kg}{mol}$ | 5.4857990943e-7
ComptonWavelength            | $m$              | 2.4263102175e-12
ClassicalElectronRadius      | $m$              | 2.8179402894e-15
ThomsonCrossSection          | $m^2$            | 0.6652458558e-28
ElectronMagneticMoment       | $\frac{J}{T}$    | -928.476377e-26
ElectronGFactor              |                  | -2.0023193043622

#### Muon

Constant                 | Unit             | Value
------------------------ | ---------------- | -----
MuonMass                 | $kg$             | 1.88353130e-28
MuonMassEnegryEquivalent | $J$              | 1.692833511e-11
MuonMolarMass            | $\frac{kg}{mol}$ | 0.1134289256e-3
MuonComptonWavelength    | $m$              | 11.73444104e-15
MuonMagneticMoment       | $\frac{J}{T}$    | -4.49044786e-26
MuonGFactor              |                  | -2.0023318414

#### Tau

Constant                | Unit             | Value
----------------------- | ---------------- | -----
TauMass                 | $kg$             | 3.16777e-27
TauMassEnergyEquivalent | $J$              | 2.84705e-10
TauMolarMass            | $\frac{kg}{mol}$ | 1.90768e-3
TauComptonWavelength    | $m$              | 0.69772e-15

#### Proton

Constant                        | Unit             | Value
------------------------------- | ---------------- | -----
ProtonMass                      | $kg$             | 1.672621637e-27
ProtonMassEnergyEquivalent      | $J$              | 1.503277359e-10
ProtonMolarMass                 | $\frac{kg}{mol}$ | 1.00727646677e-3
ProtonComptonWavelength         | $m$              | 1.3214098446e-15
ProtonMagneticMoment            | $\frac{J}{T}$    | 1.410606662e-26
ShieldedProtonMagneticMoment    | $\frac{J}{T}$    | 1.410570419e-26
ProtonGFactor                   |                  | 5.585694713
ProtonGyromagneticRatio         | $\frac{1}{T\,s}$ | 2.675222099e8
ShieldedProtonGyromagneticRatio | $\frac{1}{T\,s}$ | 2.675153362e8

#### Neutron

Constant                    | Unit             | Value
--------------------------- | ---------------- | -----
NeutronMass                 | $kg$             | 1.674927212e-27
NeutronMassEnegryEquivalent | $J$              | 1.505349506e-10
NeutronMolarMass            | $\frac{kg}{mol}$ | 1.00866491597e-3
NeutronComptonWavelength    | $m$              | 1.3195908951e-1
NeutronMagneticMoment       | $\frac{J}{T}$    | -0.96623641e-26
NeutronGFactor              |                  | -3.82608545
NeutronGyromagneticRatio    | $\frac{1}{T\,s}$ | 1.83247185e8

#### Deuteron

Constant                     | Unit             | Value
---------------------------- | ---------------- | -----
DeuteronMass                 | $kg$             | 3.34358320e-27
DeuteronMassEnegryEquivalent | $J$              |  3.00506272e-10
DeuteronMolarMass            | $\frac{kg}{mol}$ | 2.013553212725e-3
DeuteronMagneticMoment       | $\frac{J}{T}$    | 0.433073465e-26

#### Helion

Constant                   | Unit             | Value
-------------------------- | ---------------- | -----
HelionMass                 | $kg$             | 5.00641192e-27
HelionMassEnegryEquivalent | $J$              | 4.49953864e-10
HelionMolarMass            | $\frac{kg}{mol}$ | 3.0149322473e-3

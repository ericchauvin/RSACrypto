// Copyright Eric Chauvin 2019.



// These are well known standard algorithms that
// you could find in any reference, like in Wikipedia.




using System;
using System.Text;



namespace RSACrypto
{
  class StandardAlgorithms
  {
  private MainForm MForm;
  private IntegerMath IntMath;

  // I don't want to create any of these numbers
  // inside a loop so they are created just once here.
  private Integer GcdX = new Integer();
  private Integer GcdY = new Integer();
  private Integer Quotient = new Integer();
  private Integer Remainder = new Integer();
  private Integer U0 = new Integer();
  private Integer U1 = new Integer();
  private Integer U2 = new Integer();
  private Integer V0 = new Integer();
  private Integer V1 = new Integer();
  private Integer V2 = new Integer();
  private Integer T0 = new Integer();
  private Integer T1 = new Integer();
  private Integer T2 = new Integer();
  private Integer TempEuclid1 = new Integer();
  private Integer TempEuclid2 = new Integer();
  private Integer TestForModInverse1 = new Integer();
  private Integer TestForModInverse2 = new Integer();
  private Integer Fermat1 = new Integer();
  private Integer TestFermat = new Integer();
  private Integer XForModPower = new Integer();
  private Integer ExponentCopy = new Integer();
  private Integer TempForModPower = new Integer();



  private StandardAlgorithms()
    {
    }



  internal StandardAlgorithms( MainForm UseForm, IntegerMath UseIntMath )
    {
    MForm = UseForm;
    IntMath = UseIntMath;
    }




  private void ShowStatus( string ToShow )
    {
    if( MForm == null )
      return;

    MForm.ShowStatus( ToShow );
    }




  internal void GreatestCommonDivisor( Integer X, Integer Y, Integer Gcd )
    {
    // This is the basic Euclidean Algorithm.
    if( X.IsZero())
      throw( new Exception( "Doing GCD with a parameter that is zero." ));

    if( Y.IsZero())
      throw( new Exception( "Doing GCD with a parameter that is zero." ));

    if( X.IsEqual( Y ))
      {
      Gcd.Copy( X );
      return;
      }

    // Don't change the original numbers that came in as parameters.
    if( X.ParamIsGreater( Y ))
      {
      GcdX.Copy( Y );
      GcdY.Copy( X );
      }
    else
      {
      GcdX.Copy( X );
      GcdY.Copy( Y );
      }

    while( true )
      {
      IntMath.Divider.Divide( GcdX, GcdY, Quotient, Remainder );
      if( Remainder.IsZero())
        {
        Gcd.Copy( GcdY ); // It's the smaller one.
        // It can't return from this loop until the remainder is zero.
        return;
        }

      GcdX.Copy( GcdY );
      GcdY.Copy( Remainder );
      }
    }




  internal bool MultiplicativeInverse( Integer X, Integer Modulus, Integer MultInverse )
    {
    // This is the extended Euclidean Algorithm.
    // A*X + B*Y = Gcd
    // A*X + B*Y = 1 If there's a multiplicative inverse.
    // A*X = 1 - B*Y so A is the multiplicative inverse of X mod Y.
    if( X.IsZero())
      throw( new Exception( "Doing Multiplicative Inverse with a parameter that is zero." ));

    if( Modulus.IsZero())
      throw( new Exception( "Doing Multiplicative Inverse with a parameter that is zero." ));

    // This happens sometimes:
    // if( Modulus.ParamIsGreaterOrEq( X ))
      // throw( new Exception( "Modulus.ParamIsGreaterOrEq( X ) for Euclid." ));

    // Worker.ReportProgress( 0, " " );
    // Worker.ReportProgress( 0, " " );
    // Worker.ReportProgress( 0, "Top of mod inverse." );
    // U is the old part to keep.
    U0.SetToZero();
    U1.SetToOne();
    U2.Copy( Modulus ); // Don't change the original numbers that came in as parameters.
    // V is the new part.
    V0.SetToOne();
    V1.SetToZero();
    V2.Copy( X );
    T0.SetToZero();
    T1.SetToZero();
    T2.SetToZero();
    Quotient.SetToZero();
    // while( not forever if there's a problem )
    for( int Count = 0; Count < 10000; Count++ )
      {
      if( U2.IsNegative )
        throw( new Exception( "U2 was negative." ));

      if( V2.IsNegative )
        throw( new Exception( "V2 was negative." ));

      IntMath.Divider.Divide( U2, V2, Quotient, Remainder );
      if( Remainder.IsZero())
        {
        // Worker.ReportProgress( 0, "Remainder is zero. No multiplicative-inverse." );
        return false;
        }

      TempEuclid1.Copy( U0 );
      TempEuclid2.Copy( V0 );
      IntMath.Multiplier.Multiply( TempEuclid2, Quotient );
      IntMath.Subtract( TempEuclid1, TempEuclid2 );
      T0.Copy( TempEuclid1 );
      TempEuclid1.Copy( U1 );
      TempEuclid2.Copy( V1 );
      IntMath.Multiplier.Multiply( TempEuclid2, Quotient );
      IntMath.Subtract( TempEuclid1, TempEuclid2 );
      T1.Copy( TempEuclid1 );
      TempEuclid1.Copy( U2 );
      TempEuclid2.Copy( V2 );
      IntMath.Multiplier.Multiply( TempEuclid2, Quotient );
      IntMath.Subtract( TempEuclid1, TempEuclid2 );
      T2.Copy( TempEuclid1 );
      U0.Copy( V0 );
      U1.Copy( V1 );
      U2.Copy( V2 );
      V0.Copy( T0 );
      V1.Copy( T1 );
      V2.Copy( T2 );
      if( Remainder.IsOne())
        {
        // Worker.ReportProgress( 0, " " );
        // Worker.ReportProgress( 0, "Remainder is 1. There is a multiplicative-inverse." );
        break;
        }
      }

    MultInverse.Copy( T0 );
    if( MultInverse.IsNegative )
      {
      IntMath.Add( MultInverse, Modulus );
      }

    // Worker.ReportProgress( 0, "MultInverse: " + ToString10( MultInverse ));
    TestForModInverse1.Copy( MultInverse );
    TestForModInverse2.Copy( X );
    IntMath.Multiplier.Multiply( TestForModInverse1, TestForModInverse2 );
    IntMath.Divider.Divide( TestForModInverse1, Modulus, Quotient, Remainder );
    if( !Remainder.IsOne())  // By the definition of Multiplicative inverse:
      throw( new Exception( "MultInverse is wrong: " + IntMath.ToString10( Remainder )));

    // Worker.ReportProgress( 0, "MultInverse is the right number: " + ToString10( MultInverse ));
    return true;
    }




  internal bool IsFermatPrime( Integer ToTest, int HowMany )
    {
    // Use bigger primes for Fermat test because the
    // modulus can't be too small.  And also it's
    // more likely to be congruent to 1 with a very
    // small modulus.  In other words it's a lot more
    // likely to appear to be a prime when it isn't.
    // This Fermat primality test is usually
    // described as using random primes to test with,
    // and you could do it that way too.
    // A common way of doing this is to use a multiple
    // of several primes as the base, like
    // 2 * 3 * 5 * 7 = 210.

    int PArrayLength = IntMath.GetPrimeArrayLength();
    if( PArrayLength < 2 * (1024 * 16))
      throw( new Exception( "The PrimeArray length is too short for doing IsFermatPrime()." ));

    int StartAt = PArrayLength - (1024 * 16); // Or much bigger.
    if( StartAt < 100 )
      StartAt = 100;

    for( int Count = StartAt; Count < (HowMany + StartAt); Count++ )
      {
      if( !IsFermatPrimeForOneValue( ToTest, IntMath.GetPrimeAt( Count )))
        return false;

      }

    // It _might_ be a prime if it passed this test.
    // Increasing HowMany increases the probability that it's a prime.
    return true;
    }



  internal bool IsFermatPrimeForOneValue( Integer ToTest, ulong Base )
    {

    // Assume ToTest is not a small number.  (Not the size of a small prime.)
    // Normally it would be something like a 1024 bit number or bigger,
    // but I assume it's at least bigger than a 32 bit number.
    // Assume this has already been checked to see if it's divisible
    // by a small prime.
    // A has to be coprime to P and it is here because ToTest is not
    // divisible by a small prime.
    // Fermat's little theorem:
    // A ^ (P - 1) is congruent to 1 mod P if P is a prime.
    // Or: A^P - A is congrunt to A mod P.
    // If you multiply A by itself P times then divide it by P,
    // the remainder is A.  (A^P / P)
    // 5^3 = 125.  125 - 5 = 120.  A multiple of 5.
    // 2^7 = 128.  128 - 2 = 7 * 18 (a multiple of 7.)
    Fermat1.Copy( ToTest );
    IntMath.SubtractULong( Fermat1, 1 );
    TestFermat.SetFromULong( Base );

    // ModularPower( Result, Exponent, Modulus, UsePresetBaseArray )
    ModularPower( TestFermat, Fermat1, ToTest, false );
    // if( !TestFermat.IsEqual( Fermat2 ))
      // throw( new Exception( "!TestFermat.IsEqual( Fermat2 )." ));

    if( TestFermat.IsOne())
      return true; // It passed the test. It _might_ be a prime.
    else
      return false; // It is _definitely_ a composite number.

    }



  internal ulong GetFactorial( uint Value )
    {
    if( Value == 0 )
      return 1;

    if( Value == 1 )
      return 1;

    uint Factorial = 1;
    for( uint Count = 2; Count <= Value; Count++ )
      Factorial = Factorial * Count;

    return Factorial;
    }



  internal string ShowBinomialCoefficients( uint Exponent )
    {
    try
    {
    // The expansion of: (X + Y)^N
    // A binomial coefficient is  N! / (K!*(N - K)!).
    // 0 raised to the 0 power is 1.  It is defined that way.
    // So (0 + 2)^3 has all terms set to zero except for 2^3.
    StringBuilder SBuilder = new StringBuilder();
    ulong ExponentFactorial = GetFactorial( Exponent );
    SBuilder.Append( "ExponentFactorial: " + ExponentFactorial.ToString() + "\r\n" );
    for( uint Count = 0; Count <= Exponent; Count++ )
      {
      uint K = Count;
      SBuilder.Append( "\r\n" );
      SBuilder.Append( "K: " + K.ToString() + "\r\n" );
      ulong KFactorial = GetFactorial( K );
      uint ExponentMinusK = (uint)((int)Exponent - (int)K);
      ulong ExponentMinusKFactorial = GetFactorial( ExponentMinusK );
      ulong Denom = KFactorial * ExponentMinusKFactorial;
      ulong Coefficient = ExponentFactorial / Denom;
      SBuilder.Append( "Coefficient: " + Coefficient.ToString() + "\r\n");
      }

    return SBuilder.ToString();
    }
    catch( Exception Except )
      {
      throw( new Exception( "Exception in ShowBinomialCoefficients()." + Except.Message ));
      }
    }




  // This is the standard modular power algorithm that
  // you could find in any reference, but its use of
  // my modular reduction algorithm in it is new (in 2015).
  // (I mean as opposed to using some other modular reduction
  // algorithm.)
  // The square and multiply method is in Wikipedia:
  // https://en.wikipedia.org/wiki/Exponentiation_by_squaring
  internal void ModularPower( Integer Result, Integer Exponent, Integer Modulus, bool UsePresetBaseArray )
    {
    if( Result.IsZero())
      return; // With Result still zero.

    if( Result.IsEqual( Modulus ))
      {
      // It is congruent to zero % ModN.
      Result.SetToZero();
      return;
      }

    // Result is not zero at this point.
    if( Exponent.IsZero() )
      {
      Result.SetFromULong( 1 );
      return;
      }

    if( Modulus.ParamIsGreater( Result ))
      {
      // throw( new Exception( "This is not supposed to be input for RSA plain text." ));
      IntMath.Divider.Divide( Result, Modulus, Quotient, Remainder );
      Result.Copy( Remainder );
      }

    if( Exponent.IsOne())
      {
      // Result stays the same.
      return;
      }

    if( !UsePresetBaseArray )
      IntMath.ModReduction.SetupGeneralBaseArray( Modulus );

    XForModPower.Copy( Result );
    ExponentCopy.Copy( Exponent );
    // int TestIndex = 0;
    Result.SetFromULong( 1 );
    while( true )
      {
      if( (ExponentCopy.GetD( 0 ) & 1) == 1 ) // If the bottom bit is 1.
        {
        IntMath.Multiplier.Multiply( Result, XForModPower );

        // if( Result.ParamIsGreater( CurrentModReductionBase ))
        // TestForModReduction2.Copy( Result );

        IntMath.ModReduction.Reduce( TempForModPower, Result );
        // ModularReduction2( TestForModReduction2ForModPower, TestForModReduction2 );
        // if( !TestForModReduction2ForModPower.IsEqual( TempForModPower ))
          // {
          // throw( new Exception( "Mod Reduction 2 is not right." ));
          // }

        Result.Copy( TempForModPower );
        }

      ExponentCopy.ShiftRight( 1 ); // Divide by 2.
      if( ExponentCopy.IsZero())
        break;

      // Square it.
      IntMath.Multiplier.Multiply( XForModPower, XForModPower );

      // if( XForModPower.ParamIsGreater( CurrentModReductionBase ))
      IntMath.ModReduction.Reduce( TempForModPower, XForModPower );
      XForModPower.Copy( TempForModPower );
      }

    // When ModularReduction() gets called it multiplies a base number
    // by a uint sized digit.  So that can make the result one digit bigger
    // than GeneralBase.  Then when they are added up you can get carry
    // bits that can make it a little bigger.
    int HowBig = Result.GetIndex() - Modulus.GetIndex();
    // if( HowBig > 1 )
      // throw( new Exception( "This does happen. Diff: " + HowBig.ToString() ));

    // Do a proof for how big this can be.
    if( HowBig > 2 )
      throw( new Exception( "This never happens. Diff: " + HowBig.ToString() ));

    IntMath.ModReduction.Reduce( TempForModPower, Result );
    Result.Copy( TempForModPower );

    // Notice that this Divide() is done once.  Not
    // a thousand or two thousand times.
/*
    Integer ResultTest = new Integer();
    Integer ModulusTest = new Integer();
    Integer QuotientTest = new Integer();
    Integer RemainderTest = new Integer();

    ResultTest.Copy( Result );
    ModulusTest.Copy( Modulus );
    IntMath.Divider.DivideForSmallQuotient( ResultTest,
                            ModulusTest,
                            QuotientTest,
                            RemainderTest );

*/

    IntMath.Divider.Divide( Result, Modulus, Quotient, Remainder );

    // if( !RemainderTest.IsEqual( Remainder ))
      // throw( new Exception( "DivideForSmallQuotient() !RemainderTest.IsEqual( Remainder )." ));

    // if( !QuotientTest.IsEqual( Quotient ))
      // throw( new Exception( "DivideForSmallQuotient() !QuotientTest.IsEqual( Quotient )." ));


    Result.Copy( Remainder );
    if( Quotient.GetIndex() > 1 )
      throw( new Exception( "This never happens. The quotient index is never more than 1." ));

    }



  }
}


// Copyright Eric Chauvin 2019.



using System;
using System.Text;



namespace RSACrypto
{
  class IntegerMath
  {
  private MainForm MForm;
  internal Division Divider;
  internal SquareRoot SquareRt;
  internal Multiplication Multiplier;
  internal ModularReduction ModReduction;
  internal StandardAlgorithms StdAlgorithms;

  private long[] SignedD; // Signed digits for use in subtraction.

  // I don't want to create any of these numbers
  // inside a loop so they are created just once here.
  private Integer TempAdd1 = new Integer();
  private Integer TempAdd2 = new Integer();
  private Integer TempSub1 = new Integer();
  private Integer TempSub2 = new Integer();
  private Integer ToDivide = new Integer();
  private Integer Quotient = new Integer();
  private Integer Remainder = new Integer();
  private Integer TestForModInverse1 = new Integer();

  private uint[] PrimeArray;

  // PrimeArrayLength is used when testing to see if a large
  // integer is divisible by a small prime. (Among
  // other things.)  There would be an optimum length for this,
  // because following the test for small primes is the Fermat
  // primality test, which takes a lot more computation.  In
  // other words is it more efficient to test just the first
  // 100 small primes, or a million small primes, or what?
  // This length is set in the constructor.
  private int PrimeArrayLength = 1024 * 32;



  private IntegerMath()
    {
    }



  internal IntegerMath( MainForm UseForm, int UsePrimeArrayLength )
    {
    MForm = UseForm;
    PrimeArrayLength = UsePrimeArrayLength;
    if( PrimeArrayLength < 100 )
      PrimeArrayLength = 100;

    Divider = new Division( MForm, this );
    SquareRt = new SquareRoot( MForm, this );
    Multiplier = new Multiplication( MForm );
    ModReduction = new ModularReduction( MForm, this );
    StdAlgorithms = new StandardAlgorithms( MForm, this );

    SignedD = new long[Integer.DigitArraySize];

    MakePrimeArray();
    }



  internal int GetPrimeArrayLength()
    {
    return PrimeArrayLength;
    }



  private void ShowStatus( string ToShow )
    {
    if( MForm == null )
      return;

    MForm.ShowStatus( ToShow );
    }



  internal uint GetBiggestPrime()
    {
    return PrimeArray[PrimeArrayLength - 1];
    }



  internal uint GetPrimeAt( int Index )
    {
    if( Index >= PrimeArrayLength )
      return 0;

    return PrimeArray[Index];
    }



  internal uint GetFirstPrimeFactor( ulong ToTest )
    {
    if( ToTest <= 3 )
      return 0;

    // If it's an even number.
    if( (ToTest & 1) == 0 )
      return 2;

    uint Max = (uint)SquareRt.FindULSqrRoot( ToTest );
    for( int Count = 0; Count < PrimeArrayLength; Count++ )
      {
      uint TestN = PrimeArray[Count];
      if( TestN > Max )
        return 0;

      if( (ToTest % TestN) == 0 )
        return TestN;


      }

    return 0;
    }



  private void MakePrimeArray()
    {
    // try
    PrimeArray = new uint[PrimeArrayLength];
    // catch
    PrimeArray[0] = 2;
    PrimeArray[1] = 3;
    PrimeArray[2] = 5;
    PrimeArray[3] = 7;
    PrimeArray[4] = 11;
    PrimeArray[5] = 13;
    PrimeArray[6] = 17;
    PrimeArray[7] = 19;
    PrimeArray[8] = 23;

    int Last = 9;
    for( uint TestN = 29; ; TestN += 2 )
      {
      if( (TestN % 3) == 0 )
        continue;

      // If it has no prime factors then add it to the array.
      if( 0 == GetFirstPrimeFactor( TestN ))
        {
        PrimeArray[Last] = TestN;
        // if( (Last + 100) > PrimeArray.Length )
        // if( Last < 160 )
          // StatusString += Last.ToString() + ") " + PrimeArray[Last].ToString() + ", ";

        Last++;
        if( Last >= PrimeArrayLength )
          return;

        }
      }
    }



  internal uint IsDivisibleBySmallPrime( Integer ToTest )
    {
    if( (ToTest.GetD( 0 ) & 1) == 0 )
      return 2; // It's divisible by 2.

    for( int Count = 1; Count < PrimeArrayLength; Count++ )
      {
      if( 0 == Divider.GetMod32( ToTest, PrimeArray[Count] ))
        return PrimeArray[Count];

      }

    // No small primes divide it.
    return 0;
    }




  internal void SubtractULong( Integer Result, ulong ToSub )
    {
    if( Result.IsULong())
      {
      ulong ResultU = Result.GetAsULong();
      if( ToSub > ResultU )
        throw( new Exception( "SubULong() (IsULong() and (ToSub > Result)." ));

      ResultU = ResultU - ToSub;
      Result.SetD( 0, ResultU & 0xFFFFFFFF );
      Result.SetD( 1, ResultU >> 32 );
      if( Result.GetD( 1 ) == 0 )
        Result.SetIndex( 0 );
      else
        Result.SetIndex( 1 );

      return;
      }

    // If it got this far then Index is at least 2.
    SignedD[0] = (long)Result.GetD( 0 ) - (long)(ToSub & 0xFFFFFFFF);
    SignedD[1] = (long)Result.GetD( 1 ) - (long)(ToSub >> 32);
    if( (SignedD[0] >= 0) && (SignedD[1] >= 0) )
      {
      // No need to reorganize it.
      Result.SetD( 0, (ulong)SignedD[0] );
      Result.SetD( 1, (ulong)SignedD[1] );
      return;
      }

    int Max = Result.GetIndex();
    for( int Count = 2; Count <= Max; Count++ )
      SignedD[Count] = (long)Result.GetD( Count );

    Max = Result.GetIndex();
    for( int Count = 0; Count < Max; Count++ )
      {
      if( SignedD[Count] < 0 )
        {
        SignedD[Count] += (long)0xFFFFFFFF + 1;
        SignedD[Count + 1]--;
        }
      }

    if( SignedD[Result.GetIndex()] < 0 )
      throw( new Exception( "SubULong() SignedD[Index] < 0." ));

    Max = Result.GetIndex();
    for( int Count = 0; Count <= Max; Count++ )
      Result.SetD( Count, (ulong)SignedD[Count] );

    Max = Result.GetIndex();
    for( int Count = Max; Count >= 0; Count-- )
      {
      if( Result.GetD( Count ) != 0 )
        {
        Result.SetIndex( Count );
        return;
        }
      }

    // If this was zero it wouldn't find a nonzero
    // digit to set the Index to and it would end up down here.
    Result.SetIndex( 0 );
    }



  internal void Add( Integer Result, Integer ToAdd )
    {
    if( ToAdd.IsZero())
      return;

    // The most common form.  They are both positive.
    if( !Result.IsNegative && !ToAdd.IsNegative )
      {
      Result.Add( ToAdd );
      return;
      }

    if( !Result.IsNegative && ToAdd.IsNegative )
      {
      TempAdd1.Copy( ToAdd );
      TempAdd1.IsNegative = false;
      if( TempAdd1.ParamIsGreater( Result ))
        {
        Subtract( Result, TempAdd1 );
        return;
        }
      else
        {
        Subtract( TempAdd1, Result );
        Result.Copy( TempAdd1 );
        Result.IsNegative = true;
        return;
        }
      }

    if( Result.IsNegative && !ToAdd.IsNegative )
      {
      TempAdd1.Copy( Result );
      TempAdd1.IsNegative = false;
      TempAdd2.Copy( ToAdd );
      if( TempAdd1.ParamIsGreater( TempAdd2 ))
        {
        Subtract( TempAdd2, TempAdd1 );
        Result.Copy( TempAdd2 );
        return;
        }
      else
        {
        Subtract( TempAdd1, TempAdd2 );
        Result.Copy( TempAdd2 );
        Result.IsNegative = true;
        return;
        }
      }

    if( Result.IsNegative && ToAdd.IsNegative )
      {
      TempAdd1.Copy( Result );
      TempAdd1.IsNegative = false;
      TempAdd2.Copy( ToAdd );
      TempAdd2.IsNegative = false;
      TempAdd1.Add( TempAdd2 );
      Result.Copy( TempAdd1 );
      Result.IsNegative = true;
      return;
      }
    }



  internal void Subtract( Integer Result, Integer ToSub )
    {
    // This checks that the sign is equal too.
    if( Result.IsEqual( ToSub ))
      {
      Result.SetToZero();
      return;
      }

    // ParamIsGreater() handles positive and negative values, so if the
    // parameter is more toward the positive side then it's true.  It's greater.
    // The most common form.  They are both positive.
    if( !Result.IsNegative && !ToSub.IsNegative )
      {
      if( ToSub.ParamIsGreater( Result ))
        {
        SubtractPositive( Result, ToSub );
        return;
        }

      // ToSub is bigger.
      TempSub1.Copy( Result );
      TempSub2.Copy( ToSub );
      SubtractPositive( TempSub2, TempSub1 );
      Result.Copy( TempSub2 );
      Result.IsNegative = true;
      return;
      }

    if( Result.IsNegative && !ToSub.IsNegative )
      {
      TempSub1.Copy( Result );
      TempSub1.IsNegative = false;
      TempSub1.Add( ToSub );
      Result.Copy( TempSub1 );
      Result.IsNegative = true;
      return;
      }

    if( !Result.IsNegative && ToSub.IsNegative )
      {
      TempSub1.Copy( ToSub );
      TempSub1.IsNegative = false;
      Result.Add( TempSub1 );
      return;
      }

    if( Result.IsNegative && ToSub.IsNegative )
      {
      TempSub1.Copy( Result );
      TempSub1.IsNegative = false;
      TempSub2.Copy( ToSub );
      TempSub2.IsNegative = false;
      // -12 - -7 = -12 + 7 = -5
      // Comparing the positive numbers here.
      if( TempSub2.ParamIsGreater( TempSub1 ))
        {
        SubtractPositive( TempSub1, TempSub2 );
        Result.Copy( TempSub1 );
        Result.IsNegative = true;
        return;
        }

      // -7 - -12 = -7 + 12 = 5
      SubtractPositive( TempSub2, TempSub1 );
      Result.Copy( TempSub2 );
      Result.IsNegative = false;
      return;
      }
    }



  internal void SubtractPositive( Integer Result, Integer ToSub )
    {
    if( ToSub.IsULong() )
      {
      SubtractULong( Result, ToSub.GetAsULong());
      return;
      }

    if( ToSub.GetIndex() > Result.GetIndex() )
      throw( new Exception( "In Subtract() ToSub.Index > Index." ));

    int Max = ToSub.GetIndex();
    for( int Count = 0; Count <= Max; Count++ )
      SignedD[Count] = (long)Result.GetD( Count ) - (long)ToSub.GetD( Count );

    Max = Result.GetIndex();
    for( int Count = ToSub.GetIndex() + 1; Count <= Max; Count++ )
      SignedD[Count] = (long)Result.GetD( Count );

    Max = Result.GetIndex();
    for( int Count = 0; Count < Max; Count++ )
      {
      if( SignedD[Count] < 0 )
        {
        SignedD[Count] += (long)0xFFFFFFFF + 1;
        SignedD[Count + 1]--;
        }
      }

    if( SignedD[Result.GetIndex()] < 0 )
      throw( new Exception( "Subtract() SignedD[Index] < 0." ));

    Max = Result.GetIndex();
    for( int Count = 0; Count <= Max; Count++ )
      Result.SetD( Count, (ulong)SignedD[Count] );

    for( int Count = Result.GetIndex(); Count >= 0; Count-- )
      {
      if( Result.GetD( Count ) != 0 )
        {
        Result.SetIndex( Count );
        return;
        }
      }

    // If it never found a non-zero digit it would get down to here.
    Result.SetIndex( 0 );
    }




  internal void SetFromString( Integer Result, string InString )
    {
    if( InString == null )
      throw( new Exception( "InString was null in SetFromString()." ));

    if( InString.Length < 1 )
      {
      Result.SetToZero();
      return;
      }

    Base10Number Base10N = new Base10Number();
    Integer Tens = new Integer();
    Integer OnePart = new Integer();
    // This might throw an exception if the string is bad.
    Base10N.SetFromString( InString );
    Result.SetFromULong( Base10N.GetD( 0 ));
    Tens.SetFromULong( 10 );
    for( int Count = 1; Count <= Base10N.GetIndex(); Count++ )
      {
      OnePart.SetFromULong( Base10N.GetD( Count ));
      Multiplier.Multiply( OnePart, Tens );
      Result.Add( OnePart );
      Multiplier.MultiplyULong( Tens, 10 );
      }
    }



  internal string ToString10( Integer From )
    {
    if( From.IsULong())
      {
      ulong N = From.GetAsULong();
      if( From.IsNegative )
        return "-" + N.ToString( "N0" );
      else
        return N.ToString( "N0" );

      }

    string Result = "";
    ToDivide.Copy( From );
    int CommaCount = 0;
    while( !ToDivide.IsZero())
      {
      uint Digit = (uint)Divider.ShortDivideRem( ToDivide, 10, Quotient );
      ToDivide.Copy( Quotient );
      if( ((CommaCount % 3) == 0) && (CommaCount != 0) )
        Result = Digit.ToString() + "," + Result; // Or use a StringBuilder.
      else
        Result = Digit.ToString() + Result;

      CommaCount++;
      }

    if( From.IsNegative )
      return "-" + Result;
    else
      return Result;

    }



  internal static bool IsSmallQuadResidue( uint Number )
    {
    // For mod 2:
    // 1 * 1 = 1 % 2 = 1
    // 0 * 0 = 0 % 2 = 0

    uint Test = Number % 3; // 0, 1, 1, 0
    if( Test == 2 )
      return false;

    Test = Number % 5;
    if( (Test == 2) || (Test == 3))  // 0, 1, 4, 4, 1, 0
      return false;

    Test = Number % 7;
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 2)) )
      return false;

    Test = Number % 11;
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 5) ||
          (Test == 3)) )
      return false;

    Test = Number % 13;
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 3) ||
          (Test == 12) ||
          (Test == 10)) )
      return false;

    // If it made it this far...
    return true;
    }



  internal static bool IsQuadResidue17To23( uint Number )
    {
    uint Test = Number % 17;
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 16) ||
          (Test == 8) ||
          (Test == 2) ||
          (Test == 15) ||
          (Test == 13)) )
      return false;

    Test = Number % 19;
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 16) ||
          (Test == 6) ||
          (Test == 17) ||
          (Test == 11) ||
          (Test == 7) ||
          (Test == 5)) )
      return false;

    Test = Number % 23;
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 16) ||
          (Test == 2) ||
          (Test == 13) ||
          (Test == 3) ||
          (Test == 18) ||
          (Test == 12) ||
          (Test == 8) ||
          (Test == 6)) )
      return false;

    // If it made it this far...
    return true;
    }



  internal static bool IsQuadResidue29( ulong Number )
    {
    uint Test = (uint)(Number % 29);
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 16) ||
          (Test == 25) ||
          (Test == 7) ||
          (Test == 20) ||
          (Test == 6) ||
          (Test == 23) ||
          (Test == 13) ||
          (Test == 5) ||
          (Test == 28) ||
          (Test == 24) ||
          (Test == 22)) )
      return false;

    return true;
    }



  internal static bool IsQuadResidue31( ulong Number )
    {
    uint Test = (uint)(Number % 31);
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 16) ||
          (Test == 25) ||
          (Test == 5) ||
          (Test == 18) ||
          (Test == 2) ||
          (Test == 19) ||
          (Test == 7) ||
          (Test == 28) ||
          (Test == 20) ||
          (Test == 14) ||
          (Test == 10) ||
          (Test == 8)))
      return false;

    return true;
    }



  internal static bool IsQuadResidue37( ulong Number )
    {
    uint Test = (uint)(Number % 37);
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 16) ||
          (Test == 25) ||
          (Test == 36) ||
          (Test == 12) ||
          (Test == 27) ||
          (Test == 7) ||
          (Test == 26) ||
          (Test == 10) ||
          (Test == 33) ||
          (Test == 21) ||
          (Test == 11) ||
          (Test == 3) ||
          (Test == 34) ||
          (Test == 30) ||
          (Test == 28)))
      return false;

    return true;
    }



  internal static bool FirstBytesAreQuadRes( uint Test )
    {
    // Is this number a square mod 2^12?
    // (Quadratic residue mod 2^12)
    uint FirstByte = Test;
    uint SecondByte = (FirstByte & 0x0F00) >> 8;
    // The bottom 4 bits can only be 0, 1, 4 or 9
    // 0000, 0001, 0100 or 1001
    // The bottom 2 bits can only be 00 or 01
    FirstByte = FirstByte & 0x0FF;
    switch( FirstByte )
      {
      case 0x00: // return true;
        if( (SecondByte == 0) ||
            (SecondByte == 1) ||
            (SecondByte == 4) ||
            (SecondByte == 9))
          return true;
        else
          return false;

      case 0x01: return true;
      case 0x04: return true;
      case 0x09: return true;
      case 0x10: return true;
      case 0x11: return true;
      case 0x19: return true;
      case 0x21: return true;
      case 0x24: return true;
      case 0x29: return true;
      case 0x31: return true;
      case 0x39: return true;
      case 0x40: // return true;
        // 0x40, 0, 2, 4, 6, 8, 10, 12, 14
        if( (SecondByte & 0x01) == 0x01 )
          return false;
        else
          return true;

      case 0x41: return true;
      case 0x44: return true;
      case 0x49: return true;
      case 0x51: return true;
      case 0x59: return true;
      case 0x61: return true;
      case 0x64: return true;
      case 0x69: return true;
      case 0x71: return true;
      case 0x79: return true;
      case 0x81: return true;
      case 0x84: return true;
      case 0x89: return true;
      case 0x90: return true;
      case 0x91: return true;
      case 0x99: return true;
      case 0xA1: return true;
      case 0xA4: return true;
      case 0xA9: return true;
      case 0xB1: return true;
      case 0xB9: return true;
      case 0xC1: return true;
      case 0xC4: return true;
      case 0xC9: return true;
      case 0xD1: return true;
      case 0xD9: return true;
      case 0xE1: return true;
      case 0xE4: return true;
      case 0xE9: return true;
      case 0xF1: return true;
      case 0xF9: return true;  // 44 out of 256.

      default: return false;
      }
    }




  internal bool FindMultiplicativeInverseSmall( Integer ToFind, Integer KnownNumber, Integer Modulus )
    {
    // This method is for: KnownNumber * ToFind = 1 mod Modulus
    // An example:
    // PublicKeyExponent * X = 1 mod PhiN.
    // PublicKeyExponent * X = 1 mod (P - 1)(Q - 1).
    // This means that
    // (PublicKeyExponent * X) = (Y * PhiN) + 1
    // X is less than PhiN.
    // So Y is less than PublicKExponent.
    // Y can't be zero.
    // If this equation can be solved then it can be solved modulo
    // any number.  So it has to be solvable mod PublicKExponent.
    // See: Hasse Principle.
    // This also depends on the idea that the KnownNumber is prime and
    // that there is one unique modular inverse.
    // if( !KnownNumber-is-a-prime )
    //    then it won't work.
    if( !KnownNumber.IsULong())
      throw( new Exception( "FindMultiplicativeInverseSmall() was called with too big of a KnownNumber." ));

    ulong KnownNumberULong  = KnownNumber.GetAsULong();
    //                       65537
    if( KnownNumberULong > 1000000 )
      throw( new Exception( "KnownNumberULong > 1000000. FindMultiplicativeInverseSmall() was called with too big of an exponent." ));

    // (Y * PhiN) + 1 mod PubKExponent has to be zero if Y is a solution.
    ulong ModulusModKnown = Divider.GetMod32( Modulus, KnownNumberULong );
    // Worker.ReportProgress( 0, "ModulusModExponent: " + ModulusModKnown.ToString( "N0" ));
    // if( Worker.CancellationPending )
      // return false;

    // Y can't be zero.
    // The exponent is a small number like 65537.
    for( uint Y = 1; Y < (uint)KnownNumberULong; Y++ )
      {
      ulong X = (ulong)Y * ModulusModKnown;
      X++; // Add 1 to it for (Y * PhiN) + 1.
      X = X % KnownNumberULong;
      if( X == 0 )
        {
        // if( Worker.CancellationPending )
          // return false;

        // What is PhiN mod 65537?
        // That gives me Y.
        // The private key exponent is X*65537 + ModPart
        // The CipherText raised to that is the PlainText.
        // P + zN = C^(X*65537 + ModPart)
        // P + zN = C^(X*65537)(C^ModPart)
        // P + zN = ((C^65537)^X)(C^ModPart)
        // Worker.ReportProgress( 0, "Found Y at: " + Y.ToString( "N0" ));
        ToFind.Copy( Modulus );
        Multiplier.MultiplyULong( ToFind, Y );
        ToFind.AddULong( 1 );
        Divider.Divide( ToFind, KnownNumber, Quotient, Remainder );
        if( !Remainder.IsZero())
          throw( new Exception( "This can't happen. !Remainder.IsZero()" ));

        ToFind.Copy( Quotient );
        // Worker.ReportProgress( 0, "ToFind: " + ToString10( ToFind ));
        break;
        }
      }

    // if( Worker.CancellationPending )
      // return false;

    TestForModInverse1.Copy( ToFind );
    Multiplier.MultiplyULong( TestForModInverse1, KnownNumberULong );
    Divider.Divide( TestForModInverse1, Modulus, Quotient, Remainder );
    if( !Remainder.IsOne())
      {
      // The definition is that it's congruent to 1 mod the modulus,
      // so this has to be 1.
      // I've only seen this happen once.  Were the primes P and Q not
      // really primes?
      throw( new Exception( "Remainder has to be 1: " + ToString10( Remainder ) ));
      }

    return true;
    }




  }
}

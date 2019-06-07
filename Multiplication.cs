// Copyright Eric Chauvin 2019.


using System;
using System.Text;


namespace RSACrypto
{
  class Multiplication
  {
  private MainForm MForm;
  private ulong[,] M; // Scratch pad, just like you would do on paper.
  private ulong[] Scratch; // Scratch pad.




  private Multiplication()
    {
    }


  internal Multiplication( MainForm UseForm )
    {
    MForm = UseForm;

    M = new ulong[Integer.DigitArraySize, Integer.DigitArraySize];
    Scratch = new ulong[Integer.DigitArraySize];
    }



  private void ShowStatus( string ToShow )
    {
    if( MForm == null )
      return;

    MForm.ShowStatus( ToShow );
    }



  internal void MultiplyUInt( Integer Result, ulong ToMul )
    {
    try
    {
    if( ToMul == 0 )
      {
      Result.SetToZero();
      return;
      }

    if( ToMul == 1 )
      return;

    int CountTo = Result.GetIndex();
    for( int Column = 0; Column <= CountTo; Column++ )
      M[Column, 0] = ToMul * Result.GetD( Column );

    // Add these up with a carry.
    Result.SetD( 0, M[0, 0] & 0xFFFFFFFF );
    ulong Carry = M[0, 0] >> 32;
    CountTo = Result.GetIndex();
    for( int Column = 1; Column <= CountTo; Column++ )
      {
      // Using a compile-time check on this constant,
      // this Test value does not overflow:
      // const ulong Test = ((ulong)0xFFFFFFFF * (ulong)(0xFFFFFFFF)) + 0xFFFFFFFF;
      // ulong Total = checked( M[Column, 0] + Carry );
      ulong Total = M[Column, 0] + Carry;
      Result.SetD( Column, Total & 0xFFFFFFFF );
      Carry = Total >> 32;
      }

    if( Carry != 0 )
      {
      Result.IncrementIndex(); // This might throw an exception if it overflows.
      Result.SetD( Result.GetIndex(), Carry );
      }
    }
    catch( Exception Except )
      {
      throw( new Exception( "Exception in MultiplyUInt(): " + Except.Message ));
      }
    }



  internal int MultiplyUIntFromCopy( Integer Result, Integer FromCopy, ulong ToMul )
    {
    int FromCopyIndex = FromCopy.GetIndex();
    Result.SetIndex( FromCopyIndex );
    for( int Column = 0; Column <= FromCopyIndex; Column++ )
      Scratch[Column] = ToMul * FromCopy.GetD( Column );

    // Add these up with a carry.
    Result.SetD( 0, Scratch[0] & 0xFFFFFFFF );
    ulong Carry = Scratch[0] >> 32;
    for( int Column = 1; Column <= FromCopyIndex; Column++ )
      {
      ulong Total = Scratch[Column] + Carry;
      Result.SetD( Column, Total & 0xFFFFFFFF );
      Carry = Total >> 32;
      }

    if( Carry != 0 )
      {
      Result.IncrementIndex(); // This might throw an exception if it overflows.
      Result.SetD( FromCopyIndex + 1, Carry );
      }

    return Result.GetIndex();
    }



  internal void MultiplyULong( Integer Result, ulong ToMul )
    {
    // Using compile-time checks, this one overflows:
    // const ulong Test = ((ulong)0xFFFFFFFF + 1) * ((ulong)0xFFFFFFFF + 1);
    // This one doesn't:
    // const ulong Test = (ulong)0xFFFFFFFF * ((ulong)0xFFFFFFFF + 1);
    if( Result.IsZero())
      return; // Then the answer is zero, which it already is.

    if( ToMul == 0 )
      {
      Result.SetToZero();
      return;
      }

    ulong B0 = ToMul & 0xFFFFFFFF;
    ulong B1 = ToMul >> 32;
    if( B1 == 0 )
      {
      MultiplyUInt( Result, (uint)B0 );
      return;
      }

    // Since B1 is not zero:
    if( (Result.GetIndex() + 1) >= Integer.DigitArraySize )
      throw( new Exception( "Overflow in MultiplyULong." ));

    int CountTo = Result.GetIndex();
    for( int Column = 0; Column <= CountTo; Column++ )
      {
      ulong Digit = Result.GetD( Column );
      M[Column, 0] = B0 * Digit;
      // Column + 1 and Row is 1, so it's just like pen and paper.
      M[Column + 1, 1] = B1 * Digit;
      }

    // Since B1 is not zero, the index is set one higher.
    Result.IncrementIndex(); // Might throw an exception if it goes out of range.
    M[Result.GetIndex(), 0] = 0; // Otherwise it would be undefined
                                 // when it's added up below.
    // Add these up with a carry.
    Result.SetD( 0, M[0, 0] & 0xFFFFFFFF );
    ulong Carry = M[0, 0] >> 32;
    CountTo = Result.GetIndex();
    for( int Column = 1; Column <= CountTo; Column++ )
      {
      // This does overflow:
      // const ulong Test = ((ulong)0xFFFFFFFF * (ulong)(0xFFFFFFFF))
      //                  + ((ulong)0xFFFFFFFF * (ulong)(0xFFFFFFFF));
      // Split the ulongs into right and left sides
      // so that they don't overflow.
      ulong TotalLeft = 0;
      ulong TotalRight = 0;
      // There's only the two rows for this.
      for( int Row = 0; Row <= 1; Row++ )
        {
        ulong MValue = M[Column, Row];
        TotalRight += MValue & 0xFFFFFFFF;
        TotalLeft += MValue >> 32;
        }

      TotalRight += Carry;
      Result.SetD( Column, TotalRight & 0xFFFFFFFF );
      Carry = TotalRight >> 32;
      Carry += TotalLeft;
      }

    if( Carry != 0 )
      {
      Result.IncrementIndex(); // This can throw an exception.
      Result.SetD( Result.GetIndex(), Carry );
      }
    }



  private void SetMultiplySign( Integer Result, Integer ToMul )
    {
    if( Result.IsNegative == ToMul.IsNegative )
      Result.IsNegative = false;
    else
      Result.IsNegative = true;
    }



  internal void Multiply( Integer Result, Integer ToMul )
    {
    // try
    // {
    if( Result.IsZero())
      return;

    if( ToMul.IsULong())
      {
      MultiplyULong( Result, ToMul.GetAsULong());
      SetMultiplySign( Result, ToMul );
      return;
      }

    // It could never get here if ToMul is zero because GetIsULong()
    // would be true for zero.
    // if( ToMul.IsZero())
    int TotalIndex = Result.GetIndex() + ToMul.GetIndex();
    if( TotalIndex >= Integer.DigitArraySize )
      throw( new Exception( "Multiply() overflow." ));

    int CountTo = ToMul.GetIndex();
    for( int Row = 0; Row <= CountTo; Row++ )
      {
      if( ToMul.GetD( Row ) == 0 )
        {
        int CountZeros = Result.GetIndex();
        for( int Column = 0; Column <= CountZeros; Column++ )
          M[Column + Row, Row] = 0;

        }
      else
        {
        int CountMult = Result.GetIndex();
        for( int Column = 0; Column <= CountMult; Column++ )
          M[Column + Row, Row] = ToMul.GetD( Row ) * Result.GetD( Column );

        }
      }

    // Add the columns up with a carry.
    Result.SetD( 0, M[0, 0] & 0xFFFFFFFF );
    ulong Carry = M[0, 0] >> 32;
    int ResultIndex = Result.GetIndex();
    int MulIndex = ToMul.GetIndex();
    for( int Column = 1; Column <= TotalIndex; Column++ )
      {
      ulong TotalLeft = 0;
      ulong TotalRight = 0;
      for( int Row = 0; Row <= MulIndex; Row++ )
        {
        if( Row > Column )
          break;

        if( Column > (ResultIndex + Row) )
          continue;

        // Split the ulongs into right and left sides
        // so that they don't overflow.
        TotalRight += M[Column, Row] & 0xFFFFFFFF;
        TotalLeft += M[Column, Row] >> 32;
        }

      TotalRight += Carry;
      Result.SetD( Column, TotalRight & 0xFFFFFFFF );
      Carry = TotalRight >> 32;
      Carry += TotalLeft;
      }

    Result.SetIndex( TotalIndex );
    if( Carry != 0 )
      {
      Result.IncrementIndex(); // This can throw an exception if it overflowed the index.
      Result.SetD( Result.GetIndex(), Carry );
      }

    SetMultiplySign( Result, ToMul );
    }




  internal void DoSquare( Integer ToSquare )
    {
    if( ToSquare.GetIndex() == 0 )
      {
      ToSquare.Square0();
      return;
      }

    if( ToSquare.GetIndex() == 1 )
      {
      ToSquare.Square1();
      return;
      }

    if( ToSquare.GetIndex() == 2 )
      {
      ToSquare.Square2();
      return;
      }

    // Now Index is at least 3:
    int DoubleIndex = ToSquare.GetIndex() << 1;
    if( DoubleIndex >= Integer.DigitArraySize )
      {
      throw( new Exception( "Square() overflowed." ));
      }

    for( int Row = 0; Row <= ToSquare.GetIndex(); Row++ )
      {
      if( ToSquare.GetD( Row ) == 0 )
        {
        for( int Column = 0; Column <= ToSquare.GetIndex(); Column++ )
          M[Column + Row, Row] = 0;

        }
      else
        {
        for( int Column = 0; Column <= ToSquare.GetIndex(); Column++ )
          M[Column + Row, Row] = ToSquare.GetD( Row ) * ToSquare.GetD( Column );

        }
      }

    // Add the columns up with a carry.
    ToSquare.SetD( 0, M[0, 0] & 0xFFFFFFFF );
    ulong Carry = M[0, 0] >> 32;
    for( int Column = 1; Column <= DoubleIndex; Column++ )
      {
      ulong TotalLeft = 0;
      ulong TotalRight = 0;
      for( int Row = 0; Row <= Column; Row++ )
        {
        if( Row > ToSquare.GetIndex() )
          break;

        if( Column > (ToSquare.GetIndex() + Row) )
          continue;

        TotalRight += M[Column, Row] & 0xFFFFFFFF;
        TotalLeft += M[Column, Row] >> 32;
        }

      TotalRight += Carry;
      ToSquare.SetD( Column, TotalRight & 0xFFFFFFFF );
      Carry = TotalRight >> 32;
      Carry += TotalLeft;
      }

    ToSquare.SetIndex( DoubleIndex );
    if( Carry != 0 )
      {
      ToSquare.SetIndex( ToSquare.GetIndex() + 1 );
      if( ToSquare.GetIndex() >= Integer.DigitArraySize )
        throw( new Exception( "Square() overflow." ));

      ToSquare.SetD( ToSquare.GetIndex(), Carry );
      }
    }



  // This is an optimization for multiplying when
  // only the top digit of a number has been set and
  // all of the other digits are zero.
  internal void MultiplyTop( Integer Result, Integer ToMul )
    {
    // try
    // {
    int TotalIndex = Result.GetIndex() + ToMul.GetIndex();
    if( TotalIndex >= Integer.DigitArraySize )
      throw( new Exception( "MultiplyTop() overflow." ));

    // Just like Multiply() except that all the other
    // rows are zero:
    int ToMulIndex = ToMul.GetIndex();
    int ResultIndex = Result.GetIndex();
    for( int Column = 0; Column <= ToMulIndex; Column++ )
      M[Column + ResultIndex, ResultIndex] = Result.GetD( ResultIndex ) * ToMul.GetD( Column );

    for( int Column = 0; Column < ResultIndex; Column++ )
      Result.SetD( Column, 0 );

    ulong Carry = 0;
    for( int Column = 0; Column <= ToMulIndex; Column++ )
      {
      ulong Total = M[Column + ResultIndex, ResultIndex] + Carry;
      Result.SetD( Column + ResultIndex, Total & 0xFFFFFFFF );
      Carry = Total >> 32;
      }

    Result.SetIndex( TotalIndex );
    if( Carry != 0 )
      {
      Result.SetIndex( Result.GetIndex() + 1 );
      if( Result.GetIndex() >= Integer.DigitArraySize )
        throw( new Exception( "MultiplyTop() overflow." ));

      Result.SetD( Result.GetIndex(), Carry );
      }

    /*
    }
    catch( Exception ) // Except )
      {
      // "Exception in MultiplyTop: " + Except.Message
      }
    */
    }



  // This is another optimization.  This is used
  // when the top digit is 1 and all of the other
  // digits are zero.  This is effectively just a
  // shift-left operation.
  internal void MultiplyTopOne( Integer Result, Integer ToMul )
    {
    // try
    // {
    int TotalIndex = Result.GetIndex() + ToMul.GetIndex();
    if( TotalIndex >= Integer.DigitArraySize )
      throw( new Exception( "MultiplyTopOne() overflow." ));

    int ToMulIndex = ToMul.GetIndex();
    int ResultIndex = Result.GetIndex();
    for( int Column = 0; Column <= ToMulIndex; Column++ )
      Result.SetD( Column + ResultIndex, ToMul.GetD( Column ));

    for( int Column = 0; Column < ResultIndex; Column++ )
      Result.SetD( Column, 0 );

    // No Carrys need to be done.
    Result.SetIndex( TotalIndex );

    /*
    }
    catch( Exception ) // Except )
      {
      // "Exception in MultiplyTopOne: " + Except.Message
      }
      */
    }




  }
}

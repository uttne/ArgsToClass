using System;
using System.Collections.Generic;
using System.Text;

namespace ArgsAnalyzer
{


    #region Token

    public abstract class TokenBase
    {

    }

    public sealed class OptionToken : TokenBase
    {

    }

    public sealed class CommandToken : TokenBase
    {

    }

    public sealed class ValueToken : TokenBase
    {

    }


    #endregion

}

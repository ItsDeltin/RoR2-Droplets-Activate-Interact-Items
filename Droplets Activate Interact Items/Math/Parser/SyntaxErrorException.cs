using System;

namespace Deltin.Math.Parse
{
    class SyntaxErrorException : Exception
    {
        public SyntaxErrorException() : base() {}

        public SyntaxErrorException(string message) : base(message) {}
    }
}
namespace Deltin.Math.Parse
{
    class Token
    {
        public TokenType TokenType { get; }
        public string Text { get; }

        public Token(TokenType tokenType, string text)
        {
            TokenType = tokenType;
            Text = text;
        }

        public static implicit operator bool(Token token) => token != null;
        public static bool operator true(Token token) => token != null;
        public static bool operator false(Token token) => token == null;
        public static bool operator !(Token token) => token == null;
    }
}
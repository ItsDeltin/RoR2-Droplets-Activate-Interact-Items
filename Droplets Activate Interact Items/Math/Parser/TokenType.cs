namespace Deltin.Math.Parse
{
    enum TokenType
    {
        Number,
        Variable,
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Pow,
        ParenthesesOpen,
        ParenthesesClose,
        EOF
    }
}
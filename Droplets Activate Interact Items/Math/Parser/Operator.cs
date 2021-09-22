using System;

namespace Deltin.Math.Parse
{
    class Operator
    {
        public static readonly Operator[] Operators = new Operator[] {
            new Operator(null, 0, null),
            new Operator(TokenType.Subtract, 1, (a, b) => a - b),
            new Operator(TokenType.Add, 1, (a, b) => a + b),
            new Operator(TokenType.Divide, 2, (a, b) => a / b),
            new Operator(TokenType.Multiply, 2, (a, b) => a * b),
            new Operator(TokenType.Modulo, 2, (a, b) => a % b),
            new Operator(TokenType.Pow, 3, (a, b) => (float)System.Math.Pow(a, b)),
        };


        public TokenType? Type { get; }
        public int Precedence { get; }
        readonly Func<float, float, float> _evaluator;

        private Operator(TokenType? tokenType, int precedence, Func<float, float, float> evaluator)
        {
            Type = tokenType;
            Precedence = precedence;
            _evaluator = evaluator;
        }

        public Expression GetExpression(Expression left, Expression right) => new Expression.Operation(Type.GetValueOrDefault(), left, right, _evaluator);
    }
}
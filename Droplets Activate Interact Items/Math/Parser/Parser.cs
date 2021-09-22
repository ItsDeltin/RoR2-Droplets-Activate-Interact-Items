using System;
using System.Linq;
using System.Collections.Generic;

namespace Deltin.Math.Parse
{
    class Parser
    {
        readonly Token[] _tokens;
        readonly Stack<Expression> _operands = new Stack<Expression>();
        readonly Stack<Operator> _operators = new Stack<Operator>();


        int _currentToken;
        

        public Parser(Tokenizer tokenizer)
        {
            _tokens = tokenizer.GetTokens();
            _operators.Push(Operator.Operators[0]);
        }


        public Expression GetExpression()
        {
            do _operands.Push(NextNode());
            while (GetOperator());

            // Pop all operators
            while (_operators.Peek().Precedence != 0)
                PopOperator();

            return _operands.Pop();
        }

        Expression NextNode()
        {
            var exprs = new List<Expression>();

            // - unary
            if (Current().TokenType == TokenType.Subtract)
            {
                Consume();
                exprs.Add(new Expression.Negate(NextNodeNoUnary()));
            }
            // Normal
            else exprs.Add(NextNodeNoUnary());

            // Get connected expressions.
            while (IsNode)
                exprs.Add(NextNode());

            if (exprs.Count == 1)
                return exprs[0];
            else
                return new Expression.MultiplyShorthand(exprs.ToArray());
        }

        Expression NextNodeNoUnary()
        {
            Token token = Consume();
            switch(token.TokenType)
            {
                // Positive number
                case TokenType.Number:
                    return new Expression.Number(ToFloat(token.Text));

                // Variable
                case TokenType.Variable:
                    return new Expression.Variable(token.Text);

                // Grouped
                case TokenType.ParenthesesOpen:
                    var result = GetExpression();
                    Expect(TokenType.ParenthesesClose);
                    return result;
                
                // Token is not an expression
                case TokenType.EOF:
                default:
                    throw new SyntaxErrorException();
            }
        }

        bool GetOperator()
        {
            var token = Current();
            var op = Operator.Operators.FirstOrDefault(o => o.Type == token.TokenType);
            if (op == null) return false;
            Consume();

            while (_operators.Peek().Precedence >= op.Precedence)
                PopOperator();

            _operators.Push(op);
            return true;
        }

        void PopOperator()
        {
            var op = _operators.Pop();
            var right = _operands.Pop();
            var left = _operands.Pop();
            _operands.Push(op.GetExpression(left, right));
        }

        Token Current() => _tokens[System.Math.Min(_currentToken, _tokens.Length - 1)];
        Token Consume() => _tokens[_currentToken++];
        bool IsNode => new[] { TokenType.Number, TokenType.Variable, TokenType.ParenthesesOpen }.Contains(Current().TokenType);

        void Expect(TokenType tokenType)
        {
            if (Consume().TokenType != tokenType)
                throw new SyntaxErrorException();
        }

        float ToFloat(string text)
        {
            try
            {
                return float.Parse(text);
            }
            catch
            {
                throw new SyntaxErrorException();
            }
        }
    }
}
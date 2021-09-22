using System;
using System.Linq;
using System.Collections.Generic;

namespace Deltin.Math.Parse
{
    class Tokenizer
    {
        readonly static char[] NumberCharacters = "0123456789.".ToCharArray();
        readonly static char[] Operators = "+-*/%^".ToCharArray();


        char Current => Input[_position];
        bool IsNumber => !IsCompleted && NumberCharacters.Contains(Current);
        bool IsOperator => !IsCompleted && Operators.Contains(Current);
        bool IsVariable => !IsCompleted && char.IsLetter(Current);
        public bool IsCompleted => _position >= Input.Length;


        public string Input { get; }
        int _position;


        public Tokenizer(string input)
        {
            Input = input;
        }

        public Token[] GetTokens()
        {
            var tokens = new List<Token>();

            // Get tokens until we reach the EOF
            do tokens.Add(Next());
            while (tokens.Last().TokenType != TokenType.EOF);

            // Done
            return tokens.ToArray();
        }

        public Token Next()
        {
            SkipWhitespace();

            if (IsCompleted)
                return new Token(TokenType.EOF, string.Empty);
            
            // Number
            if (IsNumber)
                return Number();
            
            // Operator
            if (IsOperator)
                return Operator();
            
            // Parentheses open
            if (Current == '(')
                return new Token(TokenType.ParenthesesOpen, Consume());
            
            // Parentheses close
            if (Current == ')')
                return new Token(TokenType.ParenthesesClose, Consume());
            
            // Variable
            if (IsVariable)
                return Variable();

            // Unknown
            throw new SyntaxErrorException("Unexpected character '" + Current + "'");
        }

        Token Number()
        {
            string text = string.Empty;
            while (IsNumber)
                text += Consume();

            // Number
            return new Token(TokenType.Number, text);
        }

        Token Operator()
        {
            switch (Current)
            {
                case '+': return new Token(TokenType.Add, Consume());
                case '-': return new Token(TokenType.Subtract, Consume());
                case '*': return new Token(TokenType.Multiply, Consume());
                case '/': return new Token(TokenType.Divide, Consume());
                case '%': return new Token(TokenType.Modulo, Consume());
                case '^': return new Token(TokenType.Pow, Consume());
                default: throw new Exception(Current + " is not an operator");
            }
        }

        Token Variable()
        {
            string text = string.Empty;
            while (IsVariable)
                text += Consume();

            if (text.Length == 0)
                throw new Exception("Not a variable");

            return new Token(TokenType.Variable, text);
        }

        string Consume() => Input[_position++].ToString();

        void SkipWhitespace()
        {
            while (!IsCompleted && char.IsWhiteSpace(Current))
                _position++;
        }
    }
}
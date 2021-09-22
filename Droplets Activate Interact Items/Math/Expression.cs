using System;
using System.Linq;
using Deltin.Math.Parse;

namespace Deltin.Math
{
    abstract class Expression
    {
        public abstract override string ToString();

        public abstract float Evaluate(EvaluateInfo evaluateInfo);

        public static Expression FromString(string formula, params string[] parameters) => new Parser(new Tokenizer(formula), parameters).GetExpression();

        /// <summary>A number, ex: '12'</summary>
        public class Number : Expression
        {
            public float Value { get; }

            public Number(float value) => Value = value;

            public override float Evaluate(EvaluateInfo evaluateInfo) => Value;
            public override string ToString() => Value.ToString();
        }

        /// <summary>A variable, ex: 'n'</summary>
        public class Variable : Expression
        {
            public string Name { get; }

            public Variable(string name) => Name = name;

            public override float Evaluate(EvaluateInfo evaluateInfo)
            {
                if (evaluateInfo.InputParameters.TryGetValue(Name, out float result))
                    return result;

                throw new Exception(Name + " is not a valid variable");
            }

            public override string ToString() => Name;
        }

        /// <summary>A value encapsulated in parentheses, ex: '(a)'</summary>
        public class Group : Expression
        {
            public Expression Child { get; }

            public Group(Expression child) => Child = child;

            public override float Evaluate(EvaluateInfo evaluateInfo) => Child.Evaluate(evaluateInfo);
            public override string ToString() => "(" + Child.ToString() + ")";
        }

        /// <summary>A value negated, ex: '-a'</summary>
        public class Negate : Expression
        {
            public Expression Value { get; }

            public Negate(Expression value) => Value = value;

            public override float Evaluate(EvaluateInfo evaluateInfo) => -Value.Evaluate(evaluateInfo);
            public override string ToString() => "-" + Value.ToString();
        }

        /// <summary>An operation, ex: 'a + b'</summary>
        public class Operation : Expression
        {
            public TokenType TokenType { get; }
            public Expression Left { get; }
            public Expression Right { get; }
            readonly Func<float, float, float> _evaluator;

            public Operation(TokenType tokenType, Expression left, Expression right, Func<float, float, float> evaluator)
            {
                TokenType = tokenType;
                Left = left;
                Right = right;
                _evaluator = evaluator;
            }

            public override float Evaluate(EvaluateInfo evaluateInfo) => _evaluator(Left.Evaluate(evaluateInfo), Right.Evaluate(evaluateInfo));
            public override string ToString() => Left.ToString() + " " + SymbolFromTokenType(TokenType) + " " + Right.ToString();

            static char SymbolFromTokenType(TokenType tokenType) => tokenType switch
            {
                TokenType.Add => '+',
                TokenType.Divide => '/',
                TokenType.Subtract => '-',
                TokenType.Modulo => '%',
                TokenType.Multiply => '*',
                TokenType.Pow => '^',
                _ => '?'
            };
        }

        /// <summary>A list of values to be multiplied together, ex: 'n5'</summary>
        public class MultiplyShorthand : Expression
        {
            public Expression[] Values { get; }
            
            public MultiplyShorthand(params Expression[] values) => Values = values;

            public override float Evaluate(EvaluateInfo evaluateInfo)
            {
                float result = Values[0].Evaluate(evaluateInfo);

                for (int i = 1; i < Values.Length; i++)
                    result *= Values[i].Evaluate(evaluateInfo);

                return result;
            }
            public override string ToString() => string.Join("", Values.Select(v => v.ToString()));
        }
    }
}
using System;
using System.Linq;
using Deltin.Math.Parse;

namespace Deltin.Math
{
    abstract class Expression
    {
        public static implicit operator Expression(string formula) => new Parser(new Tokenizer(formula)).GetExpression();

        public abstract override string ToString();

        public abstract float Evaluate(EvaluateInfo evaluateInfo);

        public class Number : Expression
        {
            public float Value { get; }

            public Number(float value) => Value = value;

            public override float Evaluate(EvaluateInfo evaluateInfo) => Value;
            public override string ToString() => Value.ToString();
        }

        public class Variable : Expression
        {
            public string Name { get; }

            public Variable(string name) => Name = name;

            public override float Evaluate(EvaluateInfo evaluateInfo)
            {
                if (evaluateInfo.InputParameters.TryGetValue(Name, out float result))
                    return result;

                throw new EvaluateException();
            }

            public override string ToString() => Name;
        }

        public class Group : Expression
        {
            public Expression Child { get; }

            public Group(Expression child) => Child = child;

            public override float Evaluate(EvaluateInfo evaluateInfo) => Child.Evaluate(evaluateInfo);
            public override string ToString() => "(" + Child.ToString() + ")";
        }

        public class Negate : Expression
        {
            public Expression Value { get; }

            public Negate(Expression value) => Value = value;

            public override float Evaluate(EvaluateInfo evaluateInfo) => -Value.Evaluate(evaluateInfo);
            public override string ToString() => "-" + Value.ToString();
        }

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
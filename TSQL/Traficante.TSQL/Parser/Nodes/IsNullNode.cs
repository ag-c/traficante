﻿using System;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class IsNullNode : Node
    {
        public IsNullNode(Node expression, bool isNegated)
        {
            IsNegated = isNegated;
            Id = $"{nameof(IsNullNode)}{isNegated}";
            Expression = expression;
        }

        public Node Expression { get; }

        public bool IsNegated { get; }

        public override Type ReturnType => null;

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return IsNegated ? $"{Expression.ToString()} is not null" : $"{Expression.ToString()} is null";
        }
    }
}
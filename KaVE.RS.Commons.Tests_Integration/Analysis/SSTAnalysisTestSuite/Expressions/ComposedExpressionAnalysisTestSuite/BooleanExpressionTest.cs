﻿/*
 * Copyright 2014 Technische Universität Darmstadt
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *    http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using KaVE.Commons.Model.SSTs.Expressions.Assignable;
using KaVE.Commons.Model.SSTs.Impl.Expressions.Assignable;
using NUnit.Framework;
using Fix = KaVE.RS.Commons.Tests_Integration.Analysis.SSTAnalysisTestSuite.SSTAnalysisFixture;

namespace KaVE.RS.Commons.Tests_Integration.Analysis.SSTAnalysisTestSuite.Expressions.
    ComposedExpressionAnalysisTestSuite
{
    internal class BooleanExpressionTest : BaseSSTAnalysisTest
    {
        [Test]
        public void BooleanAndOnTwoValues()
        {
            CompleteInMethod(@"
                var i = true && false;
                $");

            AssertBody(
                VarDecl("i", Fix.Bool),
                Assign(
                    "i",
                    new BinaryExpression
                    {
                        LeftOperand = Const("true"),
                        Operator = BinaryOperator.And,
                        RightOperand = Const("false")
                    }),
                Fix.EmptyCompletion);
        }

        [Test]
        public void BooleanAndWithVariables()
        {
            CompleteInMethod(@"
                var i = false;
                var j = true && i;
                $");

            AssertBody(
                VarDecl("i", Fix.Bool),
                Assign("i", Const("false")),
                VarDecl("j", Fix.Bool),
                Assign(
                    "j",
                    new BinaryExpression
                    {
                        LeftOperand = Const("true"),
                        Operator = BinaryOperator.And,
                        RightOperand = RefExpr("i")
                    }),
                Fix.EmptyCompletion);
        }

        [Test]
        public void BooleanAndOnThreeValues()
        {
            CompleteInMethod(@"
                var i = true && false && true;
                $");

            AssertBody(
                VarDecl("i", Fix.Bool),
                VarDecl("$0", Fix.Bool),
                Assign(
                    "$0",
                    new BinaryExpression
                    {
                        LeftOperand = Const("true"),
                        Operator = BinaryOperator.And,
                        RightOperand = Const("false")
                    }),
                Assign(
                    "i",
                    new BinaryExpression
                    {
                        LeftOperand = RefExpr("$0"),
                        Operator = BinaryOperator.And,
                        RightOperand = Const("true")
                    }),
                Fix.EmptyCompletion);
        }

        [Test]
        public void BooleanOrOnTwoValues()
        {
            CompleteInMethod(@"
                var i = true || false;
                $");

            AssertBody(
                VarDecl("i", Fix.Bool),
                Assign(
                    "i",
                    new BinaryExpression
                    {
                        LeftOperand = Const("true"),
                        Operator = BinaryOperator.Or,
                        RightOperand = Const("false")
                    }),
                Fix.EmptyCompletion);
        }

        [Test]
        public void BooleanEqualityOnTwoValues()
        {
            CompleteInMethod(@"
                var i = 1 == 2;
                $");

            AssertBody(
                VarDecl("i", Fix.Bool),
                Assign(
                    "i",
                    new BinaryExpression
                    {
                        LeftOperand = Const("1"),
                        Operator = BinaryOperator.Equal,
                        RightOperand = Const("2")
                    }),
                Fix.EmptyCompletion);
        }

        [Test]
        public void BooleanInequalityOnTwoValues()
        {
            CompleteInMethod(@"
                var i = 1 != 2;
                $");

            AssertBody(
                VarDecl("i", Fix.Bool),
                Assign(
                    "i",
                    new BinaryExpression
                    {
                        LeftOperand = Const("1"),
                        Operator = BinaryOperator.NotEqual,
                        RightOperand = Const("2")
                    }),
                Fix.EmptyCompletion);
        }

        [Test]
        public void NegationExpression()
        {
            CompleteInMethod(@"
                var i = !false;
                $");

            AssertBody(
                VarDecl("i", Fix.Bool),
                Assign(
                    "i",
                    new UnaryExpression
                    {
                        Operator = UnaryOperator.Not,
                        Operand = Const("false")
                    }),
                Fix.EmptyCompletion);
        }

        [Test]
        public void Relational_Greater()
        {
            CompleteInMethod(@"
                var i = 2 > 1;
                $");

            AssertBody(
                VarDecl("i", Fix.Bool),
                Assign(
                    "i",
                    new BinaryExpression
                    {
                        LeftOperand = Const("2"),
                        Operator = BinaryOperator.GreaterThan,
                        RightOperand = Const("1")
                    }),
                Fix.EmptyCompletion);
        }

        [Test]
        public void Relational_GreaterOrEqual()
        {
            CompleteInMethod(@"
                var i = 2 >= 1;
                $");

            AssertBody(
                VarDecl("i", Fix.Bool),
                Assign(
                    "i",
                    new BinaryExpression
                    {
                        LeftOperand = Const("2"),
                        Operator = BinaryOperator.GreaterThanOrEqual,
                        RightOperand = Const("1")
                    }),
                Fix.EmptyCompletion);
        }

        [Test]
        public void Relational_Less()
        {
            CompleteInMethod(@"
                var i = 2 < 1;
                $");

            AssertBody(
                VarDecl("i", Fix.Bool),
                Assign(
                    "i",
                    new BinaryExpression
                    {
                        LeftOperand = Const("2"),
                        Operator = BinaryOperator.LessThan,
                        RightOperand = Const("1")
                    }),
                Fix.EmptyCompletion);
        }

        [Test]
        public void Relational_LessOrEqual()
        {
            CompleteInMethod(@"
                var i = 2 <= 1;
                $");

            AssertBody(
                VarDecl("i", Fix.Bool),
                Assign(
                    "i",
                    new BinaryExpression
                    {
                        LeftOperand = Const("2"),
                        Operator = BinaryOperator.LessThanOrEqual,
                        RightOperand = Const("1")
                    }),
                Fix.EmptyCompletion);
        }
    }
}
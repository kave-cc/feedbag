/*
 * Copyright 2014 Technische Universitšt Darmstadt
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
 * 
 * Contributors:
 *    - Andreas Bauer
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;
using KaVE.Commons.Model.Names;
using KaVE.Commons.Model.SSTs;
using KaVE.Commons.Model.SSTs.Visitor;
using KaVE.Commons.Model.TypeShapes;
using KaVE.Commons.Utils.Collections;

namespace KaVE.Commons.Utils.SSTPrinter
{
    public class SSTPrintingContext
    {
        /// <summary>
        ///     Base indentation level to use while printing SST nodes.
        /// </summary>
        public int IndentationLevel { get; set; }

        /// <summary>
        ///     Type shape (supertype information) of the SST. If the SST is a type and a type shape is
        ///     provided, the supertypes will be included in the print result.
        /// </summary>
        public ITypeShape TypeShape { get; set; }

        /// <summary>
        ///     Collection of namespaces that have been seen by the context while processing an SST.
        /// </summary>
        public IEnumerable<INamespaceName> SeenNamespaces
        {
            get { return _seenNamespaces.AsEnumerable(); }
        }

        private readonly StringBuilder _sb;
        private readonly IKaVESet<INamespaceName> _seenNamespaces;

        public SSTPrintingContext()
        {
            _sb = new StringBuilder();
            _seenNamespaces = Sets.NewHashSet<INamespaceName>();
        }

        /// <summary>
        ///     Appends a string to the context.
        /// </summary>
        /// <param name="text">The string to append.</param>
        /// <returns>The context after appending.</returns>
        public virtual SSTPrintingContext Text(string text)
        {
            _sb.Append(text);
            return this;
        }

        /// <summary>
        ///     Appends a comment to the context. Delimiters must be provided.
        /// </summary>
        /// <param name="commentText">The comment to append.</param>
        /// <returns>The context after appending.</returns>
        public virtual SSTPrintingContext Comment(string commentText)
        {
            return Text(commentText);
        }

        /// <summary>
        ///     Appends a line break to the context.
        /// </summary>
        /// <returns>The context after appending.</returns>
        public SSTPrintingContext NewLine()
        {
            _sb.AppendLine();
            return this;
        }

        /// <summary>
        ///     Appends a space to the context.
        /// </summary>
        /// <returns>The context after appending.</returns>
        public SSTPrintingContext Space()
        {
            _sb.Append(" ");
            return this;
        }

        /// <summary>
        ///     Appends the appropriate amount of indentation to the context according to the current indentation level. Should
        ///     always be used after appending a line break.
        /// </summary>
        /// <returns>The context after appending.</returns>
        public SSTPrintingContext Indentation()
        {
            for (int i = 0; i < IndentationLevel; i++)
            {
                _sb.Append("    ");
            }
            return this;
        }

        /// <summary>
        ///     Appends a keyword (e.g. "null", "class", "static") to the context.
        /// </summary>
        /// <param name="keyword">The keyword to append.</param>
        /// <returns>The context after appending.</returns>
        public virtual SSTPrintingContext Keyword(string keyword)
        {
            return Text(keyword);
        }

        /// <summary>
        ///     Appends a marker for the current cursor position to the context.
        /// </summary>
        /// <returns>The context after appending.</returns>
        public virtual SSTPrintingContext CursorPosition()
        {
            return Text("$");
        }

        /// <summary>
        ///     Appends a marker for an unknown entity to the context.
        /// </summary>
        /// <returns>The context after appending.</returns>
        public virtual SSTPrintingContext UnknownMarker()
        {
            return Text("???");
        }

        /// <summary>
        ///     Appends a left angle bracket ("<![CDATA[<]]>") to the context.
        /// </summary>
        /// <returns>The context after appending.</returns>
        public virtual SSTPrintingContext LeftAngleBracket()
        {
            return Text("<");
        }

        /// <summary>
        ///     Appends a right angle bracket ("("<![CDATA[>]]>") to the context.
        /// </summary>
        /// <returns>The context after appending.</returns>
        public virtual SSTPrintingContext RightAngleBracket()
        {
            return Text(">");
        }

        /// <summary>
        ///     Appends a string literal to the context. Quotation marks must not be provided.
        /// </summary>
        /// <param name="value">The string to append.</param>
        /// <returns>The context after appending.</returns>
        public virtual SSTPrintingContext StringLiteral(string value)
        {
            return Text("\"").Text(value).Text("\"");
        }

        /// <summary>
        ///     Appends the name (and only the name!) of a type to the context.
        /// </summary>
        /// <param name="typeName">The type name to append.</param>
        /// <returns>The context after appending.</returns>
        public virtual SSTPrintingContext TypeName(ITypeName typeName)
        {
            return Text(typeName.Name);
        }

        protected virtual SSTPrintingContext TypeParameterShortName(string typeParameterShortName)
        {
            return Text(typeParameterShortName);
        }

        /// <summary>
        ///     Formats and appends a type name together with its generic types to the context.
        /// </summary>
        /// <param name="typeName">The type name to append.</param>
        /// <returns>The context after appending.</returns>
        public SSTPrintingContext Type(ITypeName typeName)
        {
            _seenNamespaces.Add(typeName.Namespace);

            TypeName(typeName);

            if (typeName.HasTypeParameters)
            {
                LeftAngleBracket();

                foreach (var p in typeName.TypeParameters)
                {
                    if (!p.IsUnknownType)
                    {
                        Type(p);
                    }
                    else
                    {
                        if (p.TypeParameterType.TypeParameterShortName == "?" || // C`1[[T -> ?]]
                            p.TypeParameterType.TypeParameterShortName == null) // C`1[[T]]
                        {
                            TypeParameterShortName(p.TypeParameterShortName);
                        }
                        else
                        {
                            TypeParameterShortName(p.TypeParameterType.TypeParameterShortName);
                        }
                    }

                    if (!ReferenceEquals(p, typeName.TypeParameters.Last()))
                    {
                        _sb.Append(", ");
                    }
                }

                RightAngleBracket();
            }

            return this;
        }

        /// <summary>
        ///     Formats and appends a parameter list to the context.
        /// </summary>
        /// <param name="parameters">The list of parameters to append.</param>
        /// <returns>The context after appending.</returns>
        public SSTPrintingContext ParameterList(IList<IParameterName> parameters)
        {
            Text("(");

            foreach (var parameter in parameters)
            {
                if (parameter.IsPassedByReference && parameter.ValueType.IsValueType)
                {
                    Keyword("ref").Space();
                }

                if (parameter.IsOutput)
                {
                    Keyword("out").Space();
                }

                if (parameter.IsOptional)
                {
                    Keyword("opt").Space();
                }

                if (parameter.IsParameterArray)
                {
                    Keyword("params").Space();
                }

                Type(parameter.ValueType).Space().Text(parameter.Name);

                if (!ReferenceEquals(parameter, parameters.Last()))
                {
                    Text(",").Space();
                }
            }

            _sb.Append(")");

            return this;
        }

        /// <summary>
        ///     Appends a statement block to the context with correct indentation.
        /// </summary>
        /// <param name="block">The block to append.</param>
        /// <param name="visitor">The visitor to use for printing each statement.</param>
        /// <param name="withBrackets">If false, opening and closing brackets will be omitted.</param>
        public SSTPrintingContext StatementBlock(IKaVEList<IStatement> block,
            ISSTNodeVisitor<SSTPrintingContext> visitor,
            bool withBrackets = true)
        {
            if (!block.Any())
            {
                if (withBrackets)
                {
                    Text(" { }");
                }

                return this;
            }

            if (withBrackets)
            {
                NewLine().Indentation().Text("{");
            }

            this.IndentationLevel++;

            foreach (var statement in block)
            {
                NewLine();
                statement.Accept(visitor, this);
            }

            this.IndentationLevel--;

            if (withBrackets)
            {
                NewLine().Indentation().Text("}");
            }

            return this;
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
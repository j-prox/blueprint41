﻿using Blueprint41.Neo4j.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueprint41.Query
{
	public partial class AliasResult : Result, IPlainAliasResult
    {
        private object[] emptyArguments = new object[0];
        protected internal AliasResult()
        {
            FunctionText = delegate (QueryTranslator t) { return null; };
        }
        protected AliasResult(AliasResult parent, Func<QueryTranslator, string?>? function, object[]? arguments = null, Type? type = null)
        {
            Alias = parent;
            Node = parent.Node;
            FunctionText = function ?? delegate (QueryTranslator t) { return null; };
            FunctionArgs = arguments ?? emptyArguments;
            OverridenReturnType = type;
        }
        public AliasResult? Alias { get; private set; }
        internal Func<QueryTranslator, string?> FunctionText { get; private set; }
        internal object[]? FunctionArgs { get; private set; }
        private Type? OverridenReturnType { get; set; }


        public static QueryCondition operator ==(AliasResult a, AliasResult b)
        {
            return new QueryCondition(a, Operator.Equals, b);
        }
        public static QueryCondition operator ==(AliasResult a, Parameter b)
        {
            return new QueryCondition(a, Operator.Equals, b);
        }
        public static QueryCondition operator !=(AliasResult a, AliasResult b)
        {
            return new QueryCondition(a, Operator.NotEquals, b);
        }
        public static QueryCondition operator !=(AliasResult a, Parameter b)
        {
            return new QueryCondition(a, Operator.NotEquals, b);
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string? AliasName { get; protected internal set; }
        public Node? Node { get; protected set; }

        protected internal override void Compile(CompileState state)
        {
            state.Translator.Compile(this, state);
        }

        public QueryCondition HasLabel(string label)
        {
            return new QueryCondition(this, Operator.HasLabel, new Literal(label));
        }

        public QueryCondition Not(QueryCondition condition)
        {
            return new QueryCondition(string.Empty, Operator.Not, condition);
        }

        public AsResult As(string aliasName)
        {
            return new AsResult(this, aliasName);
        }
		public virtual AsResult As(string aliasName, out AliasResult alias)
        {
            alias = new AliasResult()
            {
                AliasName = aliasName,
            };
            return new AsResult(this, aliasName);
        }

        public AsResult Properties(string alias, out PropertiesAliasResult propertiesAlias)
        {
            propertiesAlias = new PropertiesAliasResult()
            {
                AliasName = alias
            };
            return new AsResult(new MiscResult(t => t.FnProperties, new object[] { this }, null), alias);
        }

        public override string? GetFieldName()
        {
            return AliasName;
        }

        public override Type? GetResultType()
        {
            return OverridenReturnType ?? Alias?.GetResultType() ?? null;
        }

        new public StringResult ToString()
        {
            if (AliasName is null)
                throw new InvalidOperationException("You cannot use the labels function in this context.");

            return new StringResult(t => t.FnParam1, new object[] { Parameter.Constant(AliasName) }, typeof(string));
        }

        public virtual IReadOnlyDictionary<string, FieldResult> AliasFields { get { return emptyAliasFields; }  }
        private static Dictionary<string, FieldResult> emptyAliasFields = new Dictionary<string, FieldResult>();

        public StringListResult Labels()
        {
            if (AliasName is null)
                throw new InvalidOperationException("You cannot use the labels function in this context.");

			return new StringListResult(t => t.FnLabels, new object[] { Parameter.Constant(AliasName) }, typeof(string));
        }

		AsResult IResult.As<T>(string aliasName, out T alias)
		{
			AsResult retval = As(aliasName, out AliasResult genericAlias);
			alias = (T)(object)genericAlias;
			return retval;
		}
	}
	public abstract partial class AliasResult<T, TList> : AliasResult
		where T : AliasResult<T, TList>
		where TList : ListResult<TList, T>, IAliasListResult
	{
		protected internal AliasResult()
		{
		}
		protected AliasResult(AliasResult parent, Func<QueryTranslator, string?>? function, object[]? arguments = null, Type? type = null) { }

		public TList Collect()
		{
			return ResultHelper.Of<TList>().NewAliasResult(this, t=> t.FnCollect);
		}
		public TList CollectDistinct()
		{
			return ResultHelper.Of<TList>().NewAliasResult(this, t => t.FnCollectDistinct);
		}
		public T Coalesce(T other)
		{
			return ResultHelper.Of<T>().NewAliasResult(this, t => t.FnCoalesce, new object[] { other }, null);
		}
		public sealed override AsResult As(string aliasName, out AliasResult alias)
		{
			AsResult retval = As(aliasName, out T typedAlias);
			alias = typedAlias;
			return retval;
		}
		public AsResult As(string aliasName, out T alias)
		{
			//         alias = new T((<#= DALModel.Name #>Node)Node)
			//{
			//             AliasName = aliasName
			//         };
			alias = default!;

			return As(aliasName);
		}

    }
}

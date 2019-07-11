﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Traficante.TSQL.Evaluator;
using Traficante.TSQL.Parser.Nodes;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;

namespace Traficante.TSQL.Converter.Build
{
    public class BuildItems : Dictionary<string, object>
    {
        public RootNode TransformedQueryTree
        {
            get => (RootNode)this["TRANSFORMED_QUERY_TREE"];
            set => this["TRANSFORMED_QUERY_TREE"] = value;
        }

        public RootNode RawQueryTree
        {
            get => (RootNode)this["RAW_QUERY_TREE"];
            set => this["RAW_QUERY_TREE"] = value;
        }

        public string RawQuery
        {
            get => (string)this["RAW_QUERY"];
            set => this["RAW_QUERY"] = value;
        }

        public IEngine Engine
        {
            get => (IEngine) this["SCHEMA_PROVIDER"];
            set => this["SCHEMA_PROVIDER"] = value;
        }

        public IQueryable<IObjectResolver> Stream
        {
            get => (IQueryable<IObjectResolver>)this["Stream"];
            set => this["Stream"] = value;
        }

        public string[] Columns
        {
            get => (string[])this["Columns"];
            set => this["Columns"] = value;
        }

        public System.Type[] ColumnsTypes
        {
            get => (System.Type[])this["ColumnsTypes"];
            set => this["ColumnsTypes"] = value;
        }
    }
}
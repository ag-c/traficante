﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traficante.TSQL.Converter;
using Traficante.TSQL.Evaluator.Helpers;
using Traficante.TSQL.Evaluator.Visitors;
using Traficante.TSQL.Lib;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Managers;
using MethodInfo = Traficante.TSQL.Schema.Managers.MethodInfo;

namespace Traficante.TSQL
{
    public class TSQLEngine : IDisposable
    {
        public SchemaManager SchemaManager { get; set; } = new SchemaManager();
        public VariableManager VariableManager { get; set; } = new VariableManager();

        public DataManager DataManager { get; set; }

        public TSQLEngine()
        {
            DataManager = new DataManager(this);
            SchemaManager.RegisterLibraries(new Library());
        }

        private Runner runner;
        public IEnumerable Run(string script, CancellationToken ct = default)
        {
            runner = new Runner();
            return runner.Run(script, this, ct) as IEnumerable;
        }

        public void AddTable<T>(string name, IEnumerable<T> items)
        {
            AddTable(name, new string[0],  items);
        }

        public void AddTable<TResult>(string name, Func<TResult> function)
        {
            AddTable(name, new string[0], function);
        }

        public void AddTable<T>(string name, string[] path, IEnumerable<T> items)
        {
            SchemaManager.RegisterTable(new TableInfo(name, path ?? new string[0], items));
        }

        public void AddTable<TResult>(string name, string[] path, Func<TResult> function)
        {
            SchemaManager.RegisterTable(
                new TableInfo(name, path ?? new string[0],
                    new MethodInfo
                    {
                        FunctionDelegate = function,
                        FunctionMethod = function?.Method,
                    }));
        }

        public void AddFunction<TResult>(string name, string[] path, Func<TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<TResult>(string name, Func<TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, TResult>(string name, string[] path, Func<T1, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, TResult>(string name, Func<T1, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, TResult>(string name, string[] path, Func<T1, T2, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, TResult>(string name, Func<T1, T2, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, T3, TResult>(string name, string[] path, Func<T1, T2, T3, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, T3, T4, TResult>(string name, string[] path, Func<T1, T2, T3, T4, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, T3, T4, TResult>(string name, Func<T1, T2, T3, T4, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, T3, T4, T5, TResult>(string name, string[] path, Func<T1, T2, T3, T4, T5, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, T3, T4, T5, TResult>(string name, Func<T1, T2, T3, T4, T5, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, function);
        }

        public void SetVariable<T>(string name, T value)
        {
            this.VariableManager.SetVariable<T>(name, value);
        }

        public void SetVariable(string name, Type type, object value)
        {
            this.VariableManager.SetVariable(name, type, value);
        }

        public Variable GetVariable(string name)
        {
            return this.VariableManager.GetVariable(name);
        }

        public MethodInfo ResolveMethod(string method, string[] path, Type[] parameters)
        {
            return SchemaManager.ResolveMethod(method, path, parameters);
        }

        public TableInfo ResolveTable(string name, string[] path)
        {
            return SchemaManager.ResolveTable(name, path);
        }

        public void AddMethodResolver(Func<string, string[], Type[], Delegate> resolver)
        {
            SchemaManager.RegisterMethodResolver(resolver);
        }

        public void AddTableResolver(Func<string, string[], Delegate> resolver)
        {
            SchemaManager.RegisterTableResolver(resolver);
        }

        public void Dispose()
        {
        }
    }

    public class DataManager
    {
        public List<TableResult> _tableData = new List<TableResult>();
        private readonly TSQLEngine _engine;

        public DataManager(TSQLEngine engine)
        {
            this._engine = engine;
        }
        
        public void StartRequestingTable(string name, string[] path, CancellationToken ct)
        {
            var tableData = new TableResult
            {
                Name = name,
                Path = path
            };

            if (_tableData.Any(x => x.Id == tableData.Id))
                return;

            tableData.Task = Task.Run(async () =>
            {
                var tableInfo = this._engine.ResolveTable(name, path);
                if (tableInfo == null)
                    throw new TSQLException($"Table or view does not exist: {name}");

                object results = null;
                List<(string Name, Type FieldType)> resultFields = null;

                if (tableInfo.MethodInfo != null)
                {
                    var method = tableInfo.MethodInfo;
                    var callFunction = Expression.Call(Expression.Constant(method.FunctionDelegate.Target), method.FunctionMethod);
                    var resultAsObjectExpression = Expression.Convert(callFunction, typeof(object));
                    results = Expression.Lambda<Func<object>>(resultAsObjectExpression).Compile()();
                }
                else
                {
                    results = tableInfo.Result;
                }

                if (results is Task)
                {
                    Task<object> resultsTask = (Task<object>)results;
                    await Task.WhenAll(resultsTask);
                    try
                    {
                        resultsTask.Wait();
                        results = resultsTask.Result;
                    } catch (AggregateException ex)
                    {
                        throw ex.InnerException;
                    }

                }

                var resultType = results.GetType();
                var resultItemsType = results.GetType().GetElementType();
                if (resultItemsType != null)
                {
                    resultFields = resultItemsType.GetProperties().Select(x => (x.Name, x.PropertyType)).ToList();
                }
                else if (typeof(IAsyncDataReader).IsAssignableFrom(resultType))
                {
                    var resultReader = (IAsyncDataReader)results;
                    resultItemsType = typeof(object[]);
                    resultFields = Enumerable
                        .Range(0, resultReader.FieldCount)
                        .Select(x => (resultReader.GetName(x), resultReader.GetFieldType(x)))
                        .ToList();
                    results = new AsyncDataReaderEnumerable(resultReader, ct);
                }
                else if (typeof(IDataReader).IsAssignableFrom(resultType))
                {
                    var resultReader = (IDataReader)results;
                    resultItemsType = typeof(object[]);
                    resultFields = Enumerable
                        .Range(0, resultReader.FieldCount)
                        .Select(x => (resultReader.GetName(x), resultReader.GetFieldType(x)))
                        .ToList();
                    results = new DataReaderEnumerable(resultReader, ct);
                }

                tableData.Results = results;
                tableData.ResultFields = resultFields;
                tableData.ResultType = resultType;
                tableData.ResultItemsType = resultItemsType;
            }, ct);
            _tableData.Add(tableData);
        }

        public async Task<TableResult> GeTable(string name, string[] path)
        {
            var tableData = new TableResult
            {
                Name = name,
                Path = path
            };

            var featchedData = _tableData.FirstOrDefault(x => x.Id == tableData.Id);
            var taskResult = await Task.WhenAny(featchedData.Task);
            
            try
            {
                taskResult.Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }

            return featchedData;
        }
    }


    public class TableResult
    {
        public string Name { get; set; }
        public string[] Path { get; set; }
        public object Results { get; set; }
        public List<(string Name, Type FieldType)> ResultFields { get; set; }
        public Type ResultType { get; set; }
        public Type ResultItemsType { get; set; }
        public string Id => $"{string.Join(".", this.Path)}.{this.Name}".ToLower();

        public Task Task { get; internal set; }
    }

    public class VariableManager
    {
        public List<Variable> Variables { get; set; } = new List<Variable>();

        public void SetVariable<T>(string name, T value)
        {
            SetVariable(name, typeof(T), value);
        }

        public void SetVariable(string name, Type type, object value)
        {
            var variable = Variables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (variable != null)
            {
                variable.Value = value;
            }
            else
            {
                Variables.Add(new Variable(
                    name, 
                    type.IsValueType ? type.MakeNullableType() : type, 
                    value));
            }
        }

        public Variable GetVariable(string name)
        {
            return Variables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}

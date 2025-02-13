using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Infrastructure
{
    public class SchemaInterceptor : DbCommandInterceptor
    {


        public SchemaInterceptor()
        {
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            ApplySchema(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            ApplySchema(command);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        private void ApplySchema(DbCommand command)
        {

            command.CommandText = command.CommandText.Replace("public.", $"{SharedSchema.CurrentSchema.ToLower()}.");

        }
    }



    //public class SchemaInterceptor : DbCommandInterceptor
    //{
    //    private readonly CurrentSchemaProvider _schemaProvider;

    //    public SchemaInterceptor(CurrentSchemaProvider schemaProvider)
    //    {
    //        _schemaProvider = schemaProvider;
    //    }

    //    public override InterceptionResult<DbDataReader> ReaderExecuting(
    //        DbCommand command,
    //        CommandEventData eventData,
    //        InterceptionResult<DbDataReader> result)
    //    {
    //        var schema = _schemaProvider.Schema;
    //        command.CommandText = $"SET search_path TO {schema}; {command.CommandText}";
    //        return base.ReaderExecuting(command, eventData, result);
    //    }

    //    public override InterceptionResult<int> NonQueryExecuting(
    //        DbCommand command,
    //        CommandEventData eventData,
    //        InterceptionResult<int> result)
    //    {
    //        var schema = _schemaProvider.Schema;
    //        command.CommandText = $"SET search_path TO {schema}; {command.CommandText}";
    //        return base.NonQueryExecuting(command, eventData, result);
    //    }

    //    public override InterceptionResult<object> ScalarExecuting(
    //        DbCommand command,
    //        CommandEventData eventData,
    //        InterceptionResult<object> result)
    //    {
    //        var schema = _schemaProvider.Schema;
    //        command.CommandText = $"SET search_path TO {schema}; {command.CommandText}";
    //        return base.ScalarExecuting(command, eventData, result);
    //    }
    //}
    //public class CurrentSchemaProvider
    //{
    //    private static AsyncLocal<string> _currentSchema = new AsyncLocal<string>();

    //    public string Schema
    //    {
    //        get => _currentSchema.Value;
    //        set => _currentSchema.Value = value;
    //    }
    //}
}

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace nvxapp.server.data.Infrastructure
{
    //4 SCHEMA
    public class SchemaAwareMigrationSqlGenerator : NpgsqlMigrationsSqlGenerator
    {


        public SchemaAwareMigrationSqlGenerator(MigrationsSqlGeneratorDependencies dependencies ,
                                                INpgsqlSingletonOptions npgsqlSingletonOptions)
                                                : base(dependencies , npgsqlSingletonOptions)
        {
        }

        protected override void Generate(CreateTableOperation operation,
                                         IModel model,
                                         MigrationCommandListBuilder builder,
                                         bool terminate = true)
        {

            operation.Schema = SharedSchema._schema;
            base.Generate(operation, model, builder, terminate);
        }
    }
}

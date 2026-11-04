using Microsoft.EntityFrameworkCore.Migrations;

namespace nvxapp.server.data.Migrations.Public
{
    /*
     Pacchetti di spazio iniziali (ANALISI_DOCUMENTALE.md §5.3): 1, 5 e 20 GB, predefinito 1 GB.
     Inseriti senza Id: li assegna la sequenza identity.
    */
    public static class PopolateStorage
    {
        public static void PopolateStorage_UP(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "INSERT INTO public.\"StoragePlan\" (\"Descrizione\", \"QuotaMB\", \"MaxFileMB\", \"Ordine\", \"Active\", \"Predefinito\", \"CreationDate\", \"ModifiedDate\") VALUES " +
                "('Base 1 GB', 1024, NULL, 10, true, true, now(), now()), " +
                "('Plus 5 GB', 5120, NULL, 20, true, false, now(), now()), " +
                "('Pro 20 GB', 20480, NULL, 30, true, false, now(), now());");
        }
    }
}

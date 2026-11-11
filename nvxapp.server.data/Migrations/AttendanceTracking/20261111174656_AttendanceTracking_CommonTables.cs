using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /*
     AT-COM.2 (11/11/2026): sedi, reparti, competenze e autovetture passano nelle tabelle comuni (schema Common,
     create da CommonSediCompetenzeAutovetture, INF-COM). Le presenze le mappano con ExcludeFromMigrations().

     Database NUOVO: la migration AttendanceTracking_Init non crea piu' quelle tabelle e le chiavi esterne delle presenze
     puntano gia' alle tabelle comuni: qui non c'e' nulla da spostare (il blocco esce subito).

     Database ESISTENTE in modalita' multi-tenant (search_path: tenant_<id>_2, tenant_<id>_0, public): all'avvio la
     migration Common ha gia' creato le tabelle comuni VUOTE in tenant_<id>_0; i dati sono ancora in tenant_<id>_2.
     Il blocco, nella stessa transazione della migration:
        1. copia le righe nelle tabelle comuni MANTENENDO gli Id (OVERRIDING SYSTEM VALUE); se una tabella comune ha gia'
           righe con gli stessi Id si ferma con errore (serve un intervento manuale, nulla viene modificato)
        2. Dip_RapportoAutovetture: IdDip_Anagrafica preso dal rapporto di lavoro (IdDip_RapportoLavoro)
        3. Dip_Competenza: una sola riga per dipendente e competenza (si tiene quella con l'Id piu' basso)
        4. riallinea le sequenze identity delle tabelle comuni
        5. ripunta sulle tabelle comuni le chiavi esterne delle tabelle delle presenze (stesso nome e ON DELETE)
        6. elimina le vecchie tabelle dallo schema delle presenze (con la chiave esterna verso Dip_RapportoLavoro,
           l'unica modifica che EF vedeva nel modello)
     Modalita' singola (tutto in public): le tabelle sono le stesse, non c'e' nulla da spostare (per i database
     single-tenant gia' esistenti vedi ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, AT-COM.2).

     AT-COM.3: in tutti i casi si allineano Dip_Anagrafica.DataAssunzione (primo rapporto di lavoro) e DataCessazione
     (fine dell'ultimo rapporto, null se un rapporto e' aperto).

     Down: non previsto (i dati restano nelle tabelle comuni).
    */
    /// <inheritdoc />
    public partial class AttendanceTracking_CommonTables : Migration
    {
        private const string SpostaTabelleComuni = """
DO $atcom$
DECLARE
    s_app   text := current_schema();
    s_com   text;
    tab     text;
    vecchie text[] := ARRAY['Az_Sedi', 'Az_SediReparto', 'Par_Competenza', 'Dip_Competenza', 'Dip_RapportoAutovetture'];
    fk      record;
    fks     text[] := ARRAY[]::text[];
    stmt    text;
    n       integer;
BEGIN
    -- schema Common: il secondo del search_path (tenant_<id>_2, tenant_<id>_0, public)
    SELECT x INTO s_com FROM unnest(current_schemas(false)) WITH ORDINALITY AS t(x, i) WHERE i = 2;

    IF s_app IS NULL OR s_app = 'public' OR s_com IS NULL OR s_com !~ '^tenant_[0-9]+_0$' THEN
        RAISE NOTICE 'AT-COM: schema % (common %): nessuna tabella da spostare', s_app, s_com;
        RETURN;
    END IF;

    IF to_regclass(format('%I.%I', s_app, 'Az_Sedi')) IS NULL
       AND to_regclass(format('%I.%I', s_app, 'Az_SediReparto')) IS NULL
       AND to_regclass(format('%I.%I', s_app, 'Par_Competenza')) IS NULL
       AND to_regclass(format('%I.%I', s_app, 'Dip_Competenza')) IS NULL
       AND to_regclass(format('%I.%I', s_app, 'Dip_RapportoAutovetture')) IS NULL THEN
        RAISE NOTICE 'AT-COM: schema %: tabelle gia'' comuni, nulla da spostare', s_app;
        RETURN;
    END IF;

    FOREACH tab IN ARRAY vecchie LOOP
        IF to_regclass(format('%I.%I', s_com, tab)) IS NULL THEN
            RAISE EXCEPTION 'AT-COM: manca la tabella comune %.% (migration Common non applicata)', s_com, tab;
        END IF;
        IF to_regclass(format('%I.%I', s_app, tab)) IS NOT NULL THEN
            EXECUTE format('SELECT count(*) FROM %I.%I c JOIN %I.%I a ON a."Id" = c."Id"', s_com, tab, s_app, tab) INTO n;
            IF n > 0 THEN
                RAISE EXCEPTION 'AT-COM: la tabella comune %.% contiene gia'' % righe con gli stessi Id di %.%: spostamento da fare a mano',
                                s_com, tab, n, s_app, tab;
            END IF;
        END IF;
    END LOOP;

    -- 1. copia dei dati con gli stessi Id (prima le tabelle a cui puntano le altre)
    IF to_regclass(format('%I.%I', s_app, 'Az_Sedi')) IS NOT NULL THEN
        EXECUTE format('INSERT INTO %I."Az_Sedi" ("Id", "IdAz_Anagrafica", "Descrizione", "Default", "ModifiedDate", "CreationDate", "ChangeUser")
                        OVERRIDING SYSTEM VALUE
                        SELECT "Id", "IdAz_Anagrafica", "Descrizione", "Default", "ModifiedDate", "CreationDate", "ChangeUser" FROM %I."Az_Sedi"', s_com, s_app);
    END IF;
    IF to_regclass(format('%I.%I', s_app, 'Az_SediReparto')) IS NOT NULL THEN
        EXECUTE format('INSERT INTO %I."Az_SediReparto" ("Id", "IdAz_Sedi", "Descrizione", "Default", "IdAz_SediReparto", "ModifiedDate", "CreationDate", "ChangeUser")
                        OVERRIDING SYSTEM VALUE
                        SELECT "Id", "IdAz_Sedi", "Descrizione", "Default", "IdAz_SediReparto", "ModifiedDate", "CreationDate", "ChangeUser" FROM %I."Az_SediReparto"', s_com, s_app);
    END IF;
    IF to_regclass(format('%I.%I', s_app, 'Par_Competenza')) IS NOT NULL THEN
        EXECUTE format('INSERT INTO %I."Par_Competenza" ("Id", "IdAz_Anagrafica", "Descrizione", "Default", "ModifiedDate", "CreationDate", "ChangeUser")
                        OVERRIDING SYSTEM VALUE
                        SELECT "Id", "IdAz_Anagrafica", "Descrizione", "Default", "ModifiedDate", "CreationDate", "ChangeUser" FROM %I."Par_Competenza"', s_com, s_app);
    END IF;
    -- 3. una riga per dipendente e competenza (indice univoco nella tabella comune)
    IF to_regclass(format('%I.%I', s_app, 'Dip_Competenza')) IS NOT NULL THEN
        EXECUTE format('INSERT INTO %I."Dip_Competenza" ("Id", "IdDip_Anagrafica", "IdPar_Competenza", "ModifiedDate", "CreationDate", "ChangeUser")
                        OVERRIDING SYSTEM VALUE
                        SELECT DISTINCT ON ("IdDip_Anagrafica", "IdPar_Competenza")
                               "Id", "IdDip_Anagrafica", "IdPar_Competenza", "ModifiedDate", "CreationDate", "ChangeUser"
                        FROM %I."Dip_Competenza"
                        ORDER BY "IdDip_Anagrafica", "IdPar_Competenza", "Id"', s_com, s_app);
    END IF;
    -- 2. autovetture: dal rapporto di lavoro al dipendente
    IF to_regclass(format('%I.%I', s_app, 'Dip_RapportoAutovetture')) IS NOT NULL THEN
        EXECUTE format('INSERT INTO %I."Dip_RapportoAutovetture" ("Id", "IdDip_Anagrafica", "Descrizione", "Targa", "RimborsoKM", "ModifiedDate", "CreationDate", "ChangeUser")
                        OVERRIDING SYSTEM VALUE
                        SELECT a."Id", r."IdDip_Anagrafica", a."Descrizione", a."Targa", a."RimborsoKM", a."ModifiedDate", a."CreationDate", a."ChangeUser"
                        FROM %I."Dip_RapportoAutovetture" a
                        JOIN %I."Dip_RapportoLavoro" r ON r."Id" = a."IdDip_RapportoLavoro"', s_com, s_app, s_app);
    END IF;

    -- 4. sequenze identity: il prossimo Id dopo il massimo copiato
    FOREACH tab IN ARRAY vecchie LOOP
        EXECUTE format('SELECT setval(pg_get_serial_sequence(%L, %L), COALESCE((SELECT max("Id") FROM %I.%I), 0) + 1, false)',
                       format('%I.%I', s_com, tab), 'Id', s_com, tab);
    END LOOP;

    -- 5. chiavi esterne delle tabelle delle presenze verso le vecchie tabelle: si tolgono e si ricreano sulle comuni
    FOR fk IN
        SELECT c.conname, cl.relname AS tabella, ref.relname AS riferita, a.attname AS colonna,
               CASE c.confdeltype WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT'
                                  WHEN 'r' THEN 'RESTRICT' ELSE 'NO ACTION' END AS ondelete
        FROM pg_constraint c
        JOIN pg_class cl      ON cl.oid = c.conrelid
        JOIN pg_namespace ns  ON ns.oid = cl.relnamespace
        JOIN pg_class ref     ON ref.oid = c.confrelid
        JOIN pg_namespace rns ON rns.oid = ref.relnamespace
        JOIN pg_attribute a   ON a.attrelid = c.conrelid AND a.attnum = c.conkey[1]
        WHERE c.contype = 'f'
          AND rns.nspname = s_app AND ref.relname = ANY (vecchie)
          AND ns.nspname = s_app AND NOT (cl.relname = ANY (vecchie))
          AND array_length(c.conkey, 1) = 1
    LOOP
        EXECUTE format('ALTER TABLE %I.%I DROP CONSTRAINT %I', s_app, fk.tabella, fk.conname);
        fks := fks || format('ALTER TABLE %I.%I ADD CONSTRAINT %I FOREIGN KEY (%I) REFERENCES %I.%I ("Id") ON DELETE %s',
                             s_app, fk.tabella, fk.conname, fk.colonna, s_com, fk.riferita, fk.ondelete);
    END LOOP;

    -- 6. vecchie tabelle (prima quelle che puntano alle altre)
    EXECUTE format('DROP TABLE IF EXISTS %I."Dip_Competenza"', s_app);
    EXECUTE format('DROP TABLE IF EXISTS %I."Dip_RapportoAutovetture"', s_app);
    EXECUTE format('DROP TABLE IF EXISTS %I."Az_SediReparto"', s_app);
    EXECUTE format('DROP TABLE IF EXISTS %I."Az_Sedi"', s_app);
    EXECUTE format('DROP TABLE IF EXISTS %I."Par_Competenza"', s_app);

    FOREACH stmt IN ARRAY fks LOOP
        EXECUTE stmt;
    END LOOP;

    RAISE NOTICE 'AT-COM: schema %: tabelle spostate in %, % chiavi esterne ripuntate', s_app, s_com, coalesce(array_length(fks, 1), 0);
END
$atcom$;
""";

        // AT-COM.3: date di assunzione e cessazione del dipendente comune allineate con i rapporti di lavoro
        private const string AllineaDateDipendente = """
UPDATE "Dip_Anagrafica" d
SET "DataAssunzione" = x.ass,
    "DataCessazione" = x.cess
FROM (
    SELECT "IdDip_Anagrafica",
           min("DataAss") AS ass,
           CASE WHEN bool_or("DataLic" IS NULL) THEN NULL
                ELSE (array_agg("DataLic" ORDER BY "DataAss" DESC NULLS LAST, "Id" DESC))[1] END AS cess
    FROM "Dip_RapportoLavoro"
    GROUP BY "IdDip_Anagrafica"
) x
WHERE x."IdDip_Anagrafica" = d."Id";
""";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SpostaTabelleComuni);
            migrationBuilder.Sql(AllineaDateDipendente);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // non previsto: i dati restano nelle tabelle comuni
        }
    }
}

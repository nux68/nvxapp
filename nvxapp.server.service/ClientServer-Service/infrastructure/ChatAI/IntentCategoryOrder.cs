namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Posizione delle sezioni nell'elenco dei comandi dell'help (IntentDefinition.CategoryOrder):
    // prima gli argomenti degli applicativi (da 100 a 799, es. le presenze), poi i comandi comuni.
    public static class IntentCategoryOrder
    {
        public const int Navigation = 800;
        public const int Settings = 900;
    }
}

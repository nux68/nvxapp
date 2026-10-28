# Prova delle frasi della chat AI

Programma che fa passare un elenco di frasi dal motore della chat (`ChatAIService`) con il LLM configurato e
controlla che ogni frase produca l'intent e i valori attesi. Va lanciato dopo ogni modifica a prompt,
modello, comandi o normalizzatori, per accorgersi subito se qualcosa smette di funzionare.

Non fa parte della solution né dell'immagine Docker.

## Come si lancia

```powershell
dotnet run --project tools/ChatAiPhraseTest
dotnet run --project tools/ChatAiPhraseTest -- --filtro tema          # solo le frasi che contengono "tema"
dotnet run --project tools/ChatAiPhraseTest -- --pausa 5 --provider AI_openrouter
```

- **Configurazione**: `nvxapp.server/appsettings.json` (+ `appsettings.Development.json`) e le variabili
  d'ambiente `NVXAPP_...`. La chiave del provider si legge da `NVXAPP_AI__<provider>__ApiKey`
  (processo, utente o macchina).
- `--pausa N`: secondi tra una frase e l'altra (default 20: il piano gratuito di Groq ha un limite di
  token al minuto). Se il provider risponde "troppe richieste" il programma aspetta 60 s e ripete.
- `--provider`: sostituisce `AI:UseLLM`. `--lingua`: lingua della richiesta (default `it-IT`).
- Esce con codice 1 se almeno una frase fallisce.

## Come funziona

- Comandi veri, servizi finti (anagrafica: Rossi Mario, Bianchi Luca, Verdi Giuseppe e l'utente corrente
  Pugliese Antonio); i comandi **non si eseguono**: si registrano intent e valori.
- Si confronta la prima azione del piano: in attesa (domanda o conferma) oppure eseguita subito.
- Le frasi il cui intent non esiste nel ramo (es. comandi presenze su Infrastructure) si saltano.

## Formato delle frasi (`frasi/*.json`)

```json
[
  { "frase": "metti il tema scuro", "intent": "ChangeTheme", "slots": { "mode": "dark" } },
  { "messaggi": ["timbratura per Rossi", "ieri alle 8, in uscita"], "intent": "RegisterClocking",
    "slots": { "employeeName": "Rossi Mario", "time": "08:00", "date": "=ieri", "direction": "OUT" } },
  { "frase": "quante ore ho fatto?", "intent": "HoursWorked", "ruolo": "User", "pagina": "/usertimesheet" },
  { "frase": "che tempo fa domani?", "intent": "nessuno" }
]
```

- `intent`: nome atteso, oppure `nessuno` se la frase non deve essere capita.
- `slots`: solo i valori da controllare (gli altri non contano). Un valore che inizia con `=` è
  un'espressione di data (`=ieri`, `=lunedì scorso`, `=3 novembre`) convertita come fa la chat.
- `messaggi`: più turni dello stesso dialogo; si controlla il piano dopo l'ultimo.
- `ruolo` (default `CompanyPowerAdmin`) e `pagina` (route del client) come nell'app.

Le frasi di Infrastructure stanno in `infrastruttura.json`, quelle di ogni modulo in un file proprio
(es. `presenze.json` sul ramo AttendanceTracking), così i merge tra i rami non vanno in conflitto.

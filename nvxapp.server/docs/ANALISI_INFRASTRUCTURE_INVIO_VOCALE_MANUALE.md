# ANALISI INFRASTRUCTURE - Invio manuale dei comandi vocali

> **Obiettivo**: quando si dettano a voce uno o più comandi, il testo si invia all'assistente quando lo decide l'utente, non a metà frase.

## 1. Situazione attuale

Flusso: `recognition.onresult` (solo risultati definitivi) → `collect()` → timer di 900 ms (`PhrasePauseMs`) → `flush()` → `Message$` → `FabMenuComponent.sendVoiceMessage()` → `callServer()`.

Il testo parte da solo in tre casi:

| Punto | Quando | Effetto |
|---|---|---|
| timer di `collect()` | pausa di 0,9 s mentre si parla | parte la prima metà del comando |
| `collect()`: testo che non è il seguito di quello in attesa | Chrome consegna una frase nuova | il pezzo precedente parte subito |
| `onend` → `flush()` | Chrome chiude la sessione per silenzio, rilascio del push-to-talk | parte quello che c'è |

Nella modalità vocale non c'è un campo che mostri il testo riconosciuto prima dell'invio.
Il timer da 900 ms resta necessario: unisce i pezzi crescenti che Chrome (soprattutto Android) consegna più volte come definitivi.

## 2. Soluzione

1. **`SpeechService` consegna frasi, non comandi**: tiene la logica attuale (pausa, duplicati, riavvii) ma emette `Phrase$`. `interimResults = true` alimenta solo l'anteprima (`Interim$`), che non entra mai nella bozza.
2. **Bozza vocale** (`FabMenuComponent`): le frasi si accodano (separate da `. `, così il server, che gestisce già più azioni per messaggio con `ActionResults`, le legge come frasi distinte). Il controllo dell'eco (`isEcho`) si fa su ogni frase. La bozza non si perde se Chrome chiude l'ascolto; si svuota a invio, annullamento o chiusura della modale.
3. **Invio deciso dall'utente**:
   - pulsante Invia nel composer vocale (bozza modificabile a mano);
   - parola chiave alla fine della frase: "invia", "esegui", "vai" (en: "send", "go", "run"), tolta dal testo; "annulla" / "cancella" (en: "cancel", "clear") detta da sola svuota la bozza. Le parole sono nei JSON i18n;
   - push-to-talk: il rilascio aggiunge alla bozza.
4. **Modale chiusa** (ascolto continuo): riquadro persistente in alto con la bozza, l'anteprima e i pulsanti Invia / Svuota.
5. **Risposte brevi**: con chip suggeriti dal server e bozza vuota, una frase uguale a un chip ("sì", "no") parte subito; con bozza vuota e una domanda in sospeso anche "annulla" parte come risposta.
6. **Preferenza** "invio manuale / automatico" salvata nel browser (`nvx-voice-send`), pulsante nell'header della modale vocale. Default: manuale. In automatico il comportamento è quello precedente.

Nessuna modifica lato server.

## 3. Checklist

- [x] `SpeechService`: `Phrase$` al posto di `Message$`; `flush()` di `onend` e del push-to-talk consegna alla bozza, non invia
- [x] `SpeechService`: `interimResults = true` solo per l'anteprima (`Interim$`)
- [x] Bozza vocale: accumulo, controllo `isEcho` per frase, nessuna perdita alla disattivazione automatica
- [x] Composer vocale: bozza modificabile, anteprima in grigio, pulsante Invia e pulsante Svuota
- [x] Parole chiave "invia" / "annulla" con traduzioni nei JSON i18n (it/en)
- [x] Modale chiusa: riquadro persistente con la bozza e i pulsanti Invia / Svuota
- [x] Invio diretto quando la frase corrisponde a un chip suggerito e la bozza è vuota
- [x] Preferenza manuale/automatico salvata nel browser
- [x] Verifica lato server: più comandi nello stesso messaggio (`ChatAIOutModel.ActionResults`, piano di azioni)
- [ ] Test su Chrome desktop e Android: frasi ripetute, pause lunghe, riavvii della sessione (da fare a mano, serve il microfono)
- [x] Catena dei merge (Infrastructure_Dev → Infrastructure → rami → AllFunction), 2026-11-17

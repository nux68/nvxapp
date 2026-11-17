import { Injectable } from '@angular/core';
import { LanguageService } from './language.service';
import { Subject, BehaviorSubject, Observable } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class SpeechService {
  private recognition!: any;

  // frase riconosciuta (definitiva): chi la riceve decide se inviarla subito o accodarla a una bozza
  private _Phrase$ = new Subject<string>();
  public get Phrase$(): Observable<string> {
    return this._Phrase$.asObservable();
  }

  // anteprima della frase in corso (risultati non definitivi): solo da mostrare, mai da inviare
  private _Interim$ = new BehaviorSubject<string>('');
  public get Interim$(): Observable<string> {
    return this._Interim$.asObservable();
  }
  public get interim(): string {
    return this._Interim$.value;
  }

  // invio dei comandi vocali: manuale (bozza, si invia con il pulsante o dicendo "invia") o automatico
  // (ogni frase parte da sola, comportamento storico). Scelta salvata nel browser
  private static readonly SendModeKey = 'nvx-voice-send';
  private _manualSend = true;
  public get manualSend(): boolean {
    return this._manualSend;
  }
  public set manualSend(value: boolean) {
    this._manualSend = value;
    try { localStorage.setItem(SpeechService.SendModeKey, value ? 'manual' : 'auto'); } catch { }
  }

  
  private isVoiceCommandActive = false;
  private _VoiceCommandActive$: BehaviorSubject<boolean > = new BehaviorSubject<boolean>(false);
  public get VoiceCommandActive$(): Observable<boolean > {
    return this._VoiceCommandActive$.asObservable();
  }
  public set VoiceCommandActive(value: boolean) {

    if (this.isVoiceCommandActive != value) {
      this.isVoiceCommandActive = value;
      this._VoiceCommandActive$.next(value);

      if (this.isVoiceCommandActive) {
        this.silentRestarts = 0;
        this.fatalError = false;
        this.recognition.start();
      }
      else {
        this.recognition.stop();
      }
    }

  }
  public get VoiceCommandActive(): boolean {
    return this.isVoiceCommandActive;
  }

  // emesso quando il riconoscimento si chiude da solo (silenzio prolungato, errore) mentre era attivo,
  // non quando e' l'utente a disattivarlo
  private _AutoDeactivated$ = new Subject<void>();
  public get AutoDeactivated$(): Observable<void> {
    return this._AutoDeactivated$.asObservable();
  }

  // "tieni premuto per parlare": ascolto finche' il pulsante resta premuto, senza attivare l'ascolto continuo
  private pushToTalk = false;
  private pushToTalkAt = 0;
  private static readonly ShortPressMs = 500;

  // ascolto continuo: Chrome lo chiude da solo dopo alcuni secondi di silenzio ("no-speech") e lo si
  // riavvia; si spegne per un errore vero (permesso, microfono, rete) o dopo troppi riavvii senza parole
  private static readonly MaxSilentRestarts = 15;
  private static readonly RecoverableErrors = ['no-speech', 'aborted'];
  private silentRestarts = 0;
  private fatalError = false;

  // frase in arrivo (vedi collect)
  private static readonly PhrasePauseMs = 900;
  private static readonly DuplicateMs = 4000;
  private pendingFull = '';
  private pendingTimer: ReturnType<typeof setTimeout> | null = null;
  private heardInSession = '';     // testo gia' consegnato nella sessione di riconoscimento corrente
  private lastSent = '';
  private lastSentAt = 0;

  private _PushToTalk$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  public get PushToTalk$(): Observable<boolean> {
    return this._PushToTalk$.asObservable();
  }
  public get isPushToTalk(): boolean {
    return this.pushToTalk;
  }

  public startPushToTalk() {
    if (!this.recognition || this.isVoiceCommandActive) return;
    // ascolto lasciato aperto da un click breve: la nuova pressione lo chiude
    if (this.pushToTalk) {
      this.recognition.stop();
      return;
    }
    this.pushToTalk = true;
    this.pushToTalkAt = Date.now();
    this._PushToTalk$.next(true);
    try {
      this.recognition.start();
    } catch {
      this.endPushToTalk();
    }
  }

  // rilasciato il pulsante: il browser consegna il testo riconosciuto e chiude (onend).
  // Un click breve (non tenuto premuto) lascia l'ascolto aperto fino al click successivo o al silenzio:
  // chiuderlo subito darebbe solo "no-speech"
  public stopPushToTalk() {
    if (!this.pushToTalk || !this.recognition) return;
    if (Date.now() - this.pushToTalkAt < SpeechService.ShortPressMs) return;
    this.recognition.stop();
  }

  private endPushToTalk() {
    this.pushToTalk = false;
    this._PushToTalk$.next(false);
  }

  // chiusura del riconoscimento decisa dal browser: se era attivo, e' una disattivazione automatica
  private onRecognitionClosed() {
    // fine di un "tieni premuto per parlare": non e' una disattivazione dei comandi vocali
    if (this.pushToTalk) {
      this.endPushToTalk();
      return;
    }
    const eraAttivo = this.isVoiceCommandActive;
    this.VoiceCommandActive = false;
    if (eraAttivo)
      this._AutoDeactivated$.next();
  }

  /*
   Una frase detta, una consegna (Phrase$). In ascolto continuo Chrome (soprattutto su Android) consegna la stessa
   frase piu' volte come definitiva, a pezzi crescenti ("piano", "piano fe", "piano ferie", "piano ferie"),
   e nei risultati successivi ripete anche le frasi gia' consegnate. Si tiene l'ultimo testo e lo si
   consegna dopo una breve pausa senza nuovi risultati, togliendo la parte gia' consegnata nella sessione.
   Consegnare non vuol dire inviare: con l'invio manuale la frase si accoda alla bozza (FabMenuComponent).
  */
  private collect(text: string) {
    // non e' il seguito della frase in attesa: quella si manda subito
    if (this.pendingFull && !text.toLowerCase().startsWith(this.pendingFull.toLowerCase())) this.flush();
    this.pendingFull = text;
    if (this.pendingTimer) clearTimeout(this.pendingTimer);
    this.pendingTimer = setTimeout(() => this.flush(), SpeechService.PhrasePauseMs);
  }

  private flush() {
    if (this.pendingTimer) { clearTimeout(this.pendingTimer); this.pendingTimer = null; }
    const full = this.pendingFull;
    this.pendingFull = '';
    if (!full) return;

    let text = full;
    const before = this.heardInSession;
    if (before && full.length > before.length && full.toLowerCase().startsWith(before.toLowerCase()))
      text = full.substring(before.length).trim();
    this.heardInSession = full;
    if (!text) return;

    // stessa frase consegnata due volte di seguito
    const now = Date.now();
    if (text.toLowerCase() === this.lastSent.toLowerCase() && now - this.lastSentAt < SpeechService.DuplicateMs) return;
    this.lastSent = text;
    this.lastSentAt = now;

    console.log("📨 Frase vocale:", text);
    this._Phrase$.next(text);
  }

  // consegna subito la frase in attesa (es. l'utente preme Invia mentre la pausa non e' ancora scaduta)
  public flushPending() {
    this.flush();
  }

  private setInterim(text: string) {
    if (this._Interim$.value !== text) this._Interim$.next(text);
  }

  // anteprima: i risultati non definitivi dell'evento, senza la parte gia' consegnata nella sessione
  private interimOf(event: any): string {
    let text = '';
    for (let i = event.resultIndex; i < event.results.length; i++) {
      if (!event.results[i].isFinal) text += event.results[i][0].transcript;
    }
    text = text.trim();
    const before = this.pendingFull || this.heardInSession;
    if (before && text.toLowerCase().startsWith(before.toLowerCase()))
      text = text.substring(before.length).trim();
    return text;
  }

  // ascolto continuo chiuso dal browser mentre l'utente lo vuole acceso: si riavvia
  private restartContinuous(): boolean {
    if (this.pushToTalk || !this.isVoiceCommandActive || this.fatalError) return false;
    if (++this.silentRestarts > SpeechService.MaxSilentRestarts) return false;
    try {
      this.recognition.start();
      console.log("🔁 Riconoscimento riavviato.");
      return true;
    } catch {
      return false;
    }
  }

  public stop() {
    this.VoiceCommandActive = false;
  }
  

  constructor(private languageService: LanguageService) {
    try { this._manualSend = localStorage.getItem(SpeechService.SendModeKey) !== 'auto'; } catch { }
    const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
    if (SpeechRecognition) {
      this.recognition = new SpeechRecognition();
      this.recognition.lang = this.languageService.speechLanguage; // lingua dell'interfaccia
      this.recognition.continuous = true; // Continua ad ascoltare
      this.recognition.interimResults = true; // risultati parziali: solo anteprima (Interim$)

      this.recognition.onstart = () => {
        console.log("🎤 Riconoscimento attivato.");
        this.heardInSession = '';
        if (!this.pushToTalk) this.VoiceCommandActive = true;
      };

      this.recognition.onresult = (event: any) => {
        // risultati definitivi di questo evento; quelli non definitivi solo per l'anteprima
        let resultText = '';
        for (let i = event.resultIndex; i < event.results.length; i++) {
          if (event.results[i].isFinal) resultText = event.results[i][0].transcript.trim();
        }
        if (!resultText) {
          this.setInterim(this.interimOf(event));
          return;
        }
        console.log("🗣️ Riconosciuto:", resultText);
        this.silentRestarts = 0;
        this.collect(resultText);
        this.setInterim(this.interimOf(event));
      };

      // dopo onerror arriva sempre onend: la decisione (riavvio o chiusura) si prende li'
      this.recognition.onerror = (event: any) => {
        if (SpeechService.RecoverableErrors.includes(event.error)) {
          console.warn("⚠️ Riconoscimento vocale:", event.error, "(nessuna voce ricevuta dal microfono)");
          return;
        }
        console.error("⚠️ Errore nel riconoscimento vocale:", event.error);
        this.fatalError = true;
      };

      this.recognition.onend = () => {
        console.log("⏹️ Riconoscimento terminato.");
        this.flush();
        this.setInterim('');
        if (this.restartContinuous()) return;
        this.onRecognitionClosed();
      };
    } else {
      console.error("❌ Il browser non supporta il riconoscimento vocale.");
    }
  }

  

}





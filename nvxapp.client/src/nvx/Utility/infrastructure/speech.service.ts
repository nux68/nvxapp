import { Injectable } from '@angular/core';
import { LanguageService } from './language.service';
import { Subject, BehaviorSubject, Observable } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class SpeechService {
  private recognition!: any;

  private _Message$: BehaviorSubject<string | null> = new BehaviorSubject<string>(null);
  public get Message$(): Observable<string | null> {
    return this._Message$.asObservable();
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
  private _PushToTalk$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  public get PushToTalk$(): Observable<boolean> {
    return this._PushToTalk$.asObservable();
  }
  public get isPushToTalk(): boolean {
    return this.pushToTalk;
  }

  public startPushToTalk() {
    if (!this.recognition || this.isVoiceCommandActive || this.pushToTalk) return;
    this.pushToTalk = true;
    this._PushToTalk$.next(true);
    try {
      this.recognition.start();
    } catch {
      this.endPushToTalk();
    }
  }

  // rilasciato il pulsante: il browser consegna il testo riconosciuto e chiude (onend)
  public stopPushToTalk() {
    if (this.pushToTalk && this.recognition) this.recognition.stop();
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

  public stop() {
    this.VoiceCommandActive = false;
  }
  

  constructor(private languageService: LanguageService) {
    const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
    if (SpeechRecognition) {
      this.recognition = new SpeechRecognition();
      this.recognition.lang = this.languageService.speechLanguage; // lingua dell'interfaccia
      this.recognition.continuous = true; // Continua ad ascoltare
      this.recognition.interimResults = false; // Mostra risultati parziali

      this.recognition.onstart = () => {
        console.log("🎤 Riconoscimento attivato.");
        if (!this.pushToTalk) this.VoiceCommandActive = true;
      };

      this.recognition.onresult = (event: any) => {
        const resultText = event.results[event.results.length - 1][0].transcript.trim();
        console.log("🗣️ Riconosciuto:", resultText);

        this._Message$.next(resultText);
      };

      this.recognition.onerror = (event: any) => {
        console.error("⚠️ Errore nel riconoscimento vocale:", event.error);
        this.onRecognitionClosed();
      };

      this.recognition.onend = () => {
        console.log("⏹️ Riconoscimento terminato.");
        this.onRecognitionClosed();
      };
    } else {
      console.error("❌ Il browser non supporta il riconoscimento vocale.");
    }
  }

  

}





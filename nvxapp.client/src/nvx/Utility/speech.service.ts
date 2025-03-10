import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { Observable } from 'rxjs/internal/Observable';

declare var SpeechRecognition: any;
declare var webkitSpeechRecognition: any;

@Injectable({
  providedIn: 'root'
})
export class SpeechService {
  private recognition!: any;
  
  private silenceTimeout!: any;
  private transcriptBuffer: string = ''; // Buffer per le trascrizioni parziali
  private onCommandRecognized!: (text: string) => void;

  private _Message$: BehaviorSubject<string | null> = new BehaviorSubject<string>(null);
  public get Message$(): Observable<string | null> {
    return this._Message$.asObservable();
  }

  

  ///////
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

  ///////


  constructor() {
    const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
    if (SpeechRecognition) {
      this.recognition = new SpeechRecognition();
      this.recognition.lang = 'it-IT';
      this.recognition.continuous = true; // Continua ad ascoltare
      this.recognition.interimResults = false; // Mostra risultati parziali

      this.recognition.onstart = () => {
        console.log("🎤 Riconoscimento attivato.");
        this.VoiceCommandActive = true;
        this.transcriptBuffer = ''; // Resetta il buffer
      };

      this.recognition.onresult = (event: any) => {
        const resultText = event.results[event.results.length - 1][0].transcript.trim();
        console.log("🗣️ Riconosciuto:", resultText);

        this._Message$.next(resultText);

        //if (resultText) {
        //  this.transcriptBuffer = resultText;
        //  this.resetSilenceTimeout(); // Avvia il timer per il silenzio
        //}

      };

      this.recognition.onerror = (event: any) => {
        console.error("⚠️ Errore nel riconoscimento vocale:", event.error);
        //this.stopListening();
        this.VoiceCommandActive = false;
      };

      this.recognition.onend = () => {
        console.log("⏹️ Riconoscimento terminato.");
        //this.VoiceCommandActive = false;
        this.VoiceCommandActive = false;
      };
    } else {
      console.error("❌ Il browser non supporta il riconoscimento vocale.");
    }
  }

  //startListening(onCommand: (text: string) => void) {
  //  if (!this.recognition) return;

  //  this.onCommandRecognized = onCommand;

  //  if (!this.isVoiceCommandActive) {
  //    this.recognition.start();
  //  }
  //}

  //private resetSilenceTimeout() {
  //  clearTimeout(this.silenceTimeout);

  //  // Timeout di 3 secondi di silenzio per inviare il comando
  //  this.silenceTimeout = setTimeout(() => {
  //    if (this.transcriptBuffer) {
  //      console.log("📤 Invio comando:", this.transcriptBuffer);
  //      this.onCommandRecognized(this.transcriptBuffer); // Invio del comando
  //      this.transcriptBuffer = ''; // Reset del buffer per il prossimo comando
  //    }
  //  }, 3000); // 3 secondi di silenzio
  //}

  //stopListening() {
  //  if (this.recognition) {
  //    this.recognition.stop();
  //  }
  //  this.VoiceCommandActive = false;
  //}

}





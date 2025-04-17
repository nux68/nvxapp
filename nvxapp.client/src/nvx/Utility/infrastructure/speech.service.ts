import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { Observable } from 'rxjs/internal/Observable';


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
      };

      this.recognition.onresult = (event: any) => {
        const resultText = event.results[event.results.length - 1][0].transcript.trim();
        console.log("🗣️ Riconosciuto:", resultText);

        this._Message$.next(resultText);
      };

      this.recognition.onerror = (event: any) => {
        console.error("⚠️ Errore nel riconoscimento vocale:", event.error);
        this.VoiceCommandActive = false;
      };

      this.recognition.onend = () => {
        console.log("⏹️ Riconoscimento terminato.");
        this.VoiceCommandActive = false;
      };
    } else {
      console.error("❌ Il browser non supporta il riconoscimento vocale.");
    }
  }

  

}





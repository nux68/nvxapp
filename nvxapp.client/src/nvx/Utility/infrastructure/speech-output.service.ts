import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { LanguageService } from './language.service';

/*
 Risposte vocali dell'assistente (Web Speech API, speechSynthesis) nella lingua dell'interfaccia.
 - Il testo si pronuncia a frasi: Chrome interrompe le frasi molto lunghe.
 - Mentre parla il microfono resta acceso: quello che riconosce e' la sua stessa voce (eco) oppure
   l'utente che lo interrompe. isEcho() distingue i due casi confrontando le parole.
 - La scelta di attivare/disattivare le risposte vocali e' salvata nel browser (localStorage).
*/
@Injectable({
  providedIn: 'root'
})
export class SpeechOutputService {

  private static readonly StorageKey = 'nvx-voice-reply';
  // dopo la fine della voce il riconoscimento puo' consegnare ancora la sua eco
  private static readonly EchoTailMs = 1500;

  private _Speaking$ = new BehaviorSubject<boolean>(false);
  public get Speaking$(): Observable<boolean> {
    return this._Speaking$.asObservable();
  }
  public get isSpeaking(): boolean {
    return this._Speaking$.value;
  }

  private _enabled: boolean;
  private spokenWords = new Set<string>();   // parole del testo in pronuncia (riconoscimento dell'eco)
  private endedAt = 0;
  private queue: string[] = [];

  constructor(private languageService: LanguageService) {
    let saved: string | null = null;
    try { saved = localStorage.getItem(SpeechOutputService.StorageKey); } catch { }
    this._enabled = saved !== 'off';
  }

  /** il browser sa parlare */
  public get supported(): boolean {
    return typeof window !== 'undefined' && 'speechSynthesis' in window && 'SpeechSynthesisUtterance' in window;
  }

  /** risposte vocali attive (scelta dell'utente) e possibili */
  public get enabled(): boolean {
    return this._enabled && this.supported;
  }

  public set enabled(value: boolean) {
    this._enabled = value;
    try { localStorage.setItem(SpeechOutputService.StorageKey, value ? 'on' : 'off'); } catch { }
    if (!value) this.cancel();
  }

  /** pronuncia il testo (interrompe quello in corso) */
  public speak(text: string): void {
    if (!this.enabled) return;
    this.cancel();

    const clean = this.clean(text);
    if (!clean) return;

    this.spokenWords = new Set(this.words(clean));
    this.queue = this.sentences(clean);
    this._Speaking$.next(true);
    this.next();
  }

  /** interrompe la voce (l'utente parla o chiude) */
  public cancel(): void {
    this.queue = [];
    if (this.supported) window.speechSynthesis.cancel();
    if (this.isSpeaking) this.finished();
  }

  /**
   Vero se il testo riconosciuto e' l'eco della voce dell'assistente: arriva mentre parla
   (o subito dopo) e quasi tutte le sue parole sono nel testo pronunciato.
  */
  public isEcho(heard: string): boolean {
    const recent = this.isSpeaking || Date.now() - this.endedAt < SpeechOutputService.EchoTailMs;
    if (!recent || this.spokenWords.size === 0) return false;
    const words = this.words(heard);
    if (words.length === 0) return true;
    // a voce finita una risposta breve ("sì", "alle 9") e' dell'utente, anche se quelle parole erano nella frase
    if (!this.isSpeaking && words.length <= 2) return false;
    const inSpoken = words.filter(w => this.spokenWords.has(w)).length;
    return inSpoken / words.length >= 0.6;
  }

  // -------------------------------------------------------------------------

  private next(): void {
    const sentence = this.queue.shift();
    if (!sentence) {
      this.finished();
      return;
    }
    const utterance = new SpeechSynthesisUtterance(sentence);
    utterance.lang = this.languageService.speechLanguage;
    const voice = this.voice(utterance.lang);
    if (voice) utterance.voice = voice;
    utterance.onend = () => this.next();
    utterance.onerror = () => this.finished();
    window.speechSynthesis.speak(utterance);
  }

  private finished(): void {
    this.queue = [];
    this.endedAt = Date.now();
    this._Speaking$.next(false);
  }

  // voce del browser per la lingua ("it-IT"), altrimenti una della stessa lingua ("it")
  private voice(lang: string): SpeechSynthesisVoice | undefined {
    const voices = window.speechSynthesis.getVoices();
    const base = lang.split('-')[0];
    return voices.find(v => v.lang === lang) ?? voices.find(v => v.lang.toLowerCase().startsWith(base));
  }

  // via simboli e icone del testo mostrato (①, ℹ️, •), righe -> frasi
  private clean(text: string): string {
    return (text ?? '')
      .replace(/[①-⑳ℹ️•]/g, ' ')
      .replace(/\(\s*(sì|si|yes)\s*\/\s*(no)\s*\)/gi, '')
      .replace(/\s*\n+\s*/g, '. ')
      .replace(/\s{2,}/g, ' ')
      .replace(/(\.\s*){2,}/g, '. ')
      .trim();
  }

  // frasi di al massimo ~200 caratteri
  private sentences(text: string): string[] {
    const parts = text.match(/[^.!?;]+[.!?;]*/g) ?? [text];
    const out: string[] = [];
    let current = '';
    for (const p of parts.map(s => s.trim()).filter(s => s.length > 0)) {
      if ((current + ' ' + p).length > 200 && current) {
        out.push(current);
        current = p;
      } else {
        current = current ? `${current} ${p}` : p;
      }
    }
    if (current) out.push(current);
    return out;
  }

  private words(text: string): string[] {
    return (text ?? '').toLowerCase().normalize('NFD').replace(/[̀-ͯ]/g, '')
      .split(/[^a-z0-9]+/).filter(w => w.length > 1);
  }
}

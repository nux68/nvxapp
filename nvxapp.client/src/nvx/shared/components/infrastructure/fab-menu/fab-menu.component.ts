import { AfterViewInit, Component, OnInit, ViewChild, ElementRef, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { IonContent, IonModal, NavController, ToastController } from '@ionic/angular';
import { SpeechService } from '../../../../Utility/infrastructure/speech.service';
import { SpeechOutputService } from '../../../../Utility/infrastructure/speech-output.service';
import { ChatAIService } from '../../../../ClientServer-Service/Infrastructure/ChatAI/chat-ai.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { ChatAIInModel, ClientActionPayload } from '../../../../ClientServer-Service/Infrastructure/ChatAI/Models/chat-AI-model';
import { FabMenuService } from '../../../../Utility/infrastructure/fab-menu.service';
import { ThemeService } from '../../../../Utility/infrastructure/theme.service';
import { Language, LanguageService, SUPPORTED_LANGUAGES } from '../../../../Utility/infrastructure/language.service';
import { Router } from '@angular/router';
import { IonFab } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { MainMenuService } from '../../../../Utility/infrastructure/main-menu.service';

@Component({
  selector: 'app-fab-menu',
  templateUrl: './fab-menu.component.html',
  styleUrls: ['./fab-menu.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class FabMenuComponent implements OnInit, AfterViewInit {

  // -------------------------------------------------------------------------
  // Stato chat
  // -------------------------------------------------------------------------

  public messages: Array<{ sender: string; text: string; type?: string }> = [];
  public newMessage:    string  = '';
  public isChatLoading: boolean = false;
  public suggestions:   string[] = [];   // chip suggeriti dal server (es. "Sì", "No")

  // SessionId della conversazione corrente — null = nessuna sessione attiva
  private currentSessionId: string | null = null;

  // l'ultimo messaggio e' arrivato a voce: la risposta si pronuncia
  private lastInputVoice = false;

  // -------------------------------------------------------------------------
  // Stato modale
  // -------------------------------------------------------------------------

  public isModalOpen = false;
  public modalType: string | null = null;

  // -------------------------------------------------------------------------
  // Avviso "comandi vocali disattivati" (disattivazione automatica per inattivita')
  // -------------------------------------------------------------------------

  public voiceOffNotice = false;   // avviso presente
  public voiceOffLeaving = false;  // dissolvenza in uscita in corso
  private voiceOffTimers: ReturnType<typeof setTimeout>[] = [];
  private audioCtx: AudioContext | null = null;

  // -------------------------------------------------------------------------
  // ViewChild
  // -------------------------------------------------------------------------

  @ViewChild('chatContainer', { static: false }) chatContainer!: ElementRef;
  @ViewChild(IonContent,      { static: false }) chatContent!: IonContent;
  @ViewChild('chatModal',     { static: true  }) chatModal!: IonModal;
  @ViewChild('fab',           { static: false }) fab: IonFab;

  // -------------------------------------------------------------------------
  // Constructor / lifecycle
  // -------------------------------------------------------------------------

  constructor(
    public  speechService: SpeechService,
    public  speechOutput:  SpeechOutputService,
    private chatAIService: ChatAIService,
    private cdRef:         ChangeDetectorRef,
    public  fabMenuService: FabMenuService,
    public  themeService:  ThemeService,
    public  languageService: LanguageService,
    private toastCtrl:     ToastController,
    private navCtrl:       NavController,
    private router:        Router,
    private transloco:     TranslocoService,
    private mainMenuService: MainMenuService
  ) {
    this.fabMenuService.fabMenuItem$.subscribe(() => {
      if (this.fab) this.fab.close();
    });

    // apertura della chat richiesta da altre pagine (es. home)
    this.fabMenuService.openChat$.subscribe(() => {
      this.openModal('text');
      this.cdRef.markForCheck();
    });
  }

  ngOnInit() {
    this.speechService.Message$.subscribe(msg => {
      if (msg) {
        // la voce dell'assistente captata dal microfono non e' un messaggio dell'utente
        if (this.speechOutput.isEcho(msg)) return;
        // l'utente parla: la voce dell'assistente si interrompe
        this.speechOutput.cancel();
        this.sendVoiceMessage(msg);
        this.cdRef.detectChanges();
      }
    });

    this.speechService.VoiceCommandActive$.subscribe(active => {
      // comandi vocali spenti: anche la voce dell'assistente tace
      if (!active && !this.speechService.isPushToTalk) this.speechOutput.cancel();
      this.cdRef.detectChanges();
    });

    this.speechService.PushToTalk$.subscribe(() => this.cdRef.detectChanges());
    this.speechOutput.Speaking$.subscribe(() => this.cdRef.detectChanges());

    // il browser ha chiuso il riconoscimento vocale (inattivita'/errore): avviso con suono
    this.speechService.AutoDeactivated$.subscribe(() => {
      this.showVoiceOffNotice();
    });

    // icona del pulsante tema aggiornata anche quando il tema segue il sistema operativo
    this.themeService.isDark$.subscribe(() => {
      this.cdRef.markForCheck();
    });
  }

  ngAfterViewInit() {
    if (this.chatContainer) this.scrollToBottom();
  }

  // -------------------------------------------------------------------------
  // Invio messaggio testo
  // -------------------------------------------------------------------------

  sendMessage(byVoice: boolean = false) {
    const text = this.newMessage.trim();
    if (!text) return;

    this.lastInputVoice = byVoice;
    this.speechOutput.cancel();

    this.pushMessage('You', text);
    this.newMessage  = '';
    this.suggestions = [];   // nasconde i chip mentre si aspetta la risposta
    this.scrollToBottom();

    this.callServer(text);
  }

  // -------------------------------------------------------------------------
  // Invio messaggio vocale
  // -------------------------------------------------------------------------

  sendVoiceMessage(message: string) {
    const text = message.trim();
    if (!text) return;

    this.lastInputVoice = true;

    this.pushMessage('You', text);
    this.suggestions = [];
    this.scrollToBottom();

    this.callServer(text);
  }

  // -------------------------------------------------------------------------
  // Click su un chip suggerito (es. "Sì" / "No")
  // -------------------------------------------------------------------------

  // byVoice: conferma automatica di un dialogo vocale (la risposta si pronuncia ancora)
  sendSuggestion(suggestion: string, byVoice: boolean = false) {
    this.newMessage = suggestion;
    this.sendMessage(byVoice);
  }

  // -------------------------------------------------------------------------
  // Chiamata al server
  // -------------------------------------------------------------------------

  private callServer(text: string) {
    const request = new GenericRequest<ChatAIInModel>(ChatAIInModel);
    request.data.request   = text;
    request.data.sessionId = this.currentSessionId ?? '';  // stringa vuota = nuova sessione
    request.data.currentRoute = this.router.url;            // pagina corrente: help contestuale

    this.isChatLoading = true;

    this.chatAIService.SendMessage(request).subscribe({
      next: (res) => {
        this.isChatLoading = false;

        // Salva il sessionId restituito dal server per i turni successivi.
        // La sessione continua anche dopo un risultato: il server ricorda le ultime azioni
        // ("anche per Bianchi") e decide lui quando ripartire (reset, scadenza).
        if (res.data?.sessionId) {
          this.currentSessionId = res.data.sessionId;
        }

        // Navigazione lato client se il server restituisce un payload navigate
        const navigatePayload = res.data?.navigate
          ?? res.data?.actionResults?.find(r => r.navigate != null)?.navigate
          ?? null;

        // nel testo della navigazione il server mette la route ("/requestlistadmin"): si mostra e si
        // pronuncia il nome della voce di menu ("Richieste")
        const responce = this.withPageTitle(res.data?.responce ?? '', navigatePayload?.route);
        const speechText = this.withPageTitle(res.data?.speechText ?? '', navigatePayload?.route);

        // Mostra la risposta dell'assistente solo se non è vuota
        if (responce.trim().length > 0) {
          this.pushMessage('Assistant', responce, res.data?.responseType);
        }

        // Mostra i chip di suggerimento se presenti
        this.suggestions = res.data?.suggestions ?? [];
        if (navigatePayload?.route) {
          const navOptions = navigatePayload.state ? { state: navigatePayload.state } : {};
          if (this.isModalOpen) {
            this.chatModal.dismiss(null, 'cancel').then(() =>
              this.navCtrl.navigateForward(navigatePayload.route, navOptions)
            );
          } else {
            this.navCtrl.navigateForward(navigatePayload.route, navOptions);
          }
        }

        // Azione da eseguire nel client (cambio tema o lingua)
        const clientAction = res.data?.clientAction
          ?? res.data?.actionResults?.find(r => r.clientAction != null)?.clientAction
          ?? null;
        if (clientAction) {
          this.runClientAction(clientAction);
        }

        const responseType = res.data?.responseType ?? '';
        const autoConfirm = responseType === 'confirmation' && !!res.data?.autoConfirm;

        // risposta vocale (se il messaggio e' arrivato a voce): domanda, riepilogo in forma di frase o risultato.
        // La conferma automatica della navigazione non si annuncia: segue subito il risultato.
        if (this.lastInputVoice && !autoConfirm) {
          this.speechOutput.speak(speechText || responce);
        }

        // comando eseguito (anche solo in parte): la pagina corrente rilegge i dati
        if (responseType === 'result' || res.data?.actionResults?.some(r => r.success)) {
          this.fabMenuService.commandExecuted();
        }

        if (this.speechService.VoiceCommandActive) {
          if (!this.isModalOpen) {
            if (responseType === 'result' || responseType === 'error') {
              // Comando completato: mostra toast con la risposta dell'assistente
              this.toastCtrl.create({
                message:  responce,
                duration: 3000,
                position: 'bottom'
              }).then(t => t.present());
            } else if (autoConfirm) {
              // Modal chiusa + vocale + piano di sola navigazione: auto-conferma senza aprire la modal.
              // Usa il primo chip del server (conferma, gia' nella lingua dell'utente: "Sì"/"Yes");
              // il server accetta comunque le parole di conferma italiane e inglesi.
              const confirmText = res.data?.suggestions?.[0] ?? 'Sì';
              setTimeout(() => this.sendSuggestion(confirmText, true), 600);
            } else if (this.speechOutput.enabled) {
              // domanda o riepilogo da confermare: l'assistente lo dice e si risponde a voce
              // ("alle 9", "sì"), senza aprire la modal; il testo resta visibile in un avviso
              this.toastCtrl.create({
                message:  responce,
                duration: 10000,
                position: 'bottom'
              }).then(t => t.present());
            } else {
              // browser senza sintesi vocale o risposte vocali spente: l'utente vede la domanda
              // o il riepilogo nella modal e risponde (anche a voce)
              this.openModal('vocal');
            }
          }
          // Modal aperta: l'utente vede il riepilogo e deve confermare manualmente
        }

        this.cdRef.detectChanges();
        this.scrollToBottom();
      },
      error: () => {
        this.isChatLoading = false;
        this.pushMessage('Assistant', this.transloco.translate('components.fabMenu.serverError'), 'error');
        this.currentSessionId = null;
        this.cdRef.detectChanges();
        this.scrollToBottom();
      }
    });
  }

  // -------------------------------------------------------------------------
  // Helper messaggi
  // -------------------------------------------------------------------------

  private pushMessage(sender: string, text: string, type: string = '') {
    this.messages.push({ sender, text, type });
  }

  // route della pagina nel testo -> nome della voce di menu nella lingua dell'utente (route non nel menu: invariato)
  private withPageTitle(text: string, route: string | null | undefined): string {
    if (!text || !route) return text;
    const title = this.mainMenuService.TitoloPagina(route);
    return title ? text.split(route).join(this.transloco.translate(title)) : text;
  }

  // Converte \n in <br> per la visualizzazione HTML nel template
  // testo dell'assistente: a capo e **grassetto** (titoli delle sezioni dell'help); l'HTML del testo non si interpreta
  formatText(text: string): string {
    return (text ?? '')
      .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
      .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
      .replace(/\n/g, '<br>');
  }

  // -------------------------------------------------------------------------
  // Scroll
  // -------------------------------------------------------------------------

  scrollToBottom() {
    setTimeout(() => {
      this.chatContent?.scrollToBottom(300);
    }, 100);
  }

  // -------------------------------------------------------------------------
  // Modale
  // -------------------------------------------------------------------------

  openModal(type: string) {
    this.modalType   = type;
    this.isModalOpen = true;
    this.scrollToBottom();
  }

  cancelChat() {
    this.chatModal.dismiss(null, 'cancel');
  }

  onWillDismissChat(event: any) {
    this.isModalOpen      = false;
    this.modalType        = null;
    this.currentSessionId = null;   // reset sessione alla chiusura del modale
    this.suggestions      = [];
    // i comandi vocali rimangono attivi anche dopo la chiusura del modale
  }

  // -------------------------------------------------------------------------
  // Azioni client richieste dalla chat
  // -------------------------------------------------------------------------

  private runClientAction(action: ClientActionPayload) {
    if (action.action === 'theme') {
      // "dark" / "light" impostano il tema richiesto, "toggle" lo alterna
      const dark = action.value === 'dark' ? true : action.value === 'light' ? false : !this.themeService.isDark;
      if (dark !== this.themeService.isDark) {
        this.themeService.toggle();
      }
    } else if (action.action === 'language' && (SUPPORTED_LANGUAGES as readonly string[]).includes(action.value)) {
      // il cambio di lingua ricarica la pagina: si lascia il tempo di leggere la risposta
      setTimeout(() => this.languageService.setLanguage(action.value as Language), 1500);
    }
  }

  // -------------------------------------------------------------------------
  // Comandi vocali
  // -------------------------------------------------------------------------

  toggleVoiceCommand() {
    this.speechService.VoiceCommandActive = !this.speechService.VoiceCommandActive;
  }

  // "tieni premuto per parlare" (alternativa all'ascolto continuo)
  startPushToTalk(event: Event) {
    event.preventDefault();
    this.speechOutput.cancel();
    this.speechService.startPushToTalk();
  }

  stopPushToTalk() {
    this.speechService.stopPushToTalk();
  }

  // risposte vocali attive / disattivate (scelta salvata nel browser)
  toggleVoiceReply() {
    this.speechOutput.enabled = !this.speechOutput.enabled;
  }

  stopSpeaking() {
    this.speechOutput.cancel();
  }

  // Chiamato dal FAB mic: se voice attivo lo ferma, altrimenti apre la chat vocale (mic da avviare manualmente)
  toggleVocalFromFab() {
    if (this.speechService.VoiceCommandActive) {
      this.speechService.stop();
    } else {
      this.openModal('vocal');
    }
  }

  // -------------------------------------------------------------------------
  // Avviso di disattivazione automatica dei comandi vocali
  // -------------------------------------------------------------------------

  private showVoiceOffNotice() {
    this.voiceOffTimers.forEach(t => clearTimeout(t));
    this.voiceOffNotice = true;
    this.voiceOffLeaving = false;
    this.cdRef.detectChanges();
    this.playVoiceOffSound();

    // dopo qualche secondo scompare in dissolvenza
    this.voiceOffTimers = [setTimeout(() => this.hideVoiceOffNotice(), 5000)];
  }

  hideVoiceOffNotice() {
    this.voiceOffTimers.forEach(t => clearTimeout(t));
    this.voiceOffLeaving = true;
    this.cdRef.detectChanges();
    this.voiceOffTimers = [setTimeout(() => {
      this.voiceOffNotice = false;
      this.voiceOffLeaving = false;
      this.cdRef.detectChanges();
    }, 400)];   // durata dell'animazione di uscita (fab-menu.component.scss)
  }

  reactivateVoiceCommand() {
    this.hideVoiceOffNotice();
    this.speechService.VoiceCommandActive = true;
  }

  // due note discendenti (Mi5 -> Si4) generate con Web Audio, senza file audio
  private playVoiceOffSound() {
    try {
      const Ctx = window.AudioContext || (window as any).webkitAudioContext;
      if (!Ctx) return;
      this.audioCtx ??= new Ctx();
      const ctx = this.audioCtx!;
      if (ctx.state === 'suspended') ctx.resume();

      const note = (freq: number, start: number, durata: number) => {
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = 'sine';
        osc.frequency.value = freq;
        const t0 = ctx.currentTime + start;
        gain.gain.setValueAtTime(0.0001, t0);
        gain.gain.exponentialRampToValueAtTime(0.18, t0 + 0.02);
        gain.gain.exponentialRampToValueAtTime(0.0001, t0 + durata);
        osc.connect(gain).connect(ctx.destination);
        osc.start(t0);
        osc.stop(t0 + durata + 0.05);
      };
      note(659.25, 0, 0.22);     // Mi5
      note(493.88, 0.16, 0.35);  // Si4
    } catch {
      // audio non disponibile: resta l'avviso visivo
    }
  }

}

import { AfterViewInit, Component, OnInit, ViewChild, ElementRef, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { IonContent, IonModal, NavController, ToastController } from '@ionic/angular/lazy';
import { SpeechService } from '../../../../Utility/infrastructure/speech.service';
import { ChatAIService } from '../../../../ClientServer-Service/Infrastructure/ChatAI/chat-ai.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { ChatAIInModel } from '../../../../ClientServer-Service/Infrastructure/ChatAI/Models/chat-AI-model';
import { FabMenuService } from '../../../../Utility/infrastructure/fab-menu.service';
import { ThemeService } from '../../../../Utility/infrastructure/theme.service';
import { IonFab } from '@ionic/angular/lazy';

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
    private chatAIService: ChatAIService,
    private cdRef:         ChangeDetectorRef,
    public  fabMenuService: FabMenuService,
    public  themeService:  ThemeService,
    private toastCtrl:     ToastController,
    private navCtrl:       NavController
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
        this.sendVoiceMessage(msg);
        this.cdRef.detectChanges();
      }
    });

    this.speechService.VoiceCommandActive$.subscribe(() => {
      this.cdRef.detectChanges();
    });

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

  sendMessage() {
    const text = this.newMessage.trim();
    if (!text) return;

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

    this.pushMessage('You', text);
    this.suggestions = [];
    this.scrollToBottom();

    this.callServer(text);
  }

  // -------------------------------------------------------------------------
  // Click su un chip suggerito (es. "Sì" / "No")
  // -------------------------------------------------------------------------

  sendSuggestion(suggestion: string) {
    this.newMessage = suggestion;
    this.sendMessage();
  }

  // -------------------------------------------------------------------------
  // Chiamata al server
  // -------------------------------------------------------------------------

  private callServer(text: string) {
    const request = new GenericRequest<ChatAIInModel>(ChatAIInModel);
    request.data.request   = text;
    request.data.sessionId = this.currentSessionId ?? '';  // stringa vuota = nuova sessione

    this.isChatLoading = true;

    this.chatAIService.SendMessage(request).subscribe({
      next: (res) => {
        this.isChatLoading = false;

        // Salva il sessionId restituito dal server per i turni successivi
        if (res.data?.sessionId) {
          this.currentSessionId = res.data.sessionId;
        }

        // Se la sessione è conclusa (result/error) resetta il sessionId
        if (res.data?.responseType === 'result' || res.data?.responseType === 'error') {
          this.currentSessionId = null;
        }

        // Mostra la risposta dell'assistente solo se non è vuota
        const responce = res.data?.responce ?? '';
        if (responce.trim().length > 0) {
          this.pushMessage('Assistant', responce, res.data?.responseType);
        }

        // Mostra i chip di suggerimento se presenti
        this.suggestions = res.data?.suggestions ?? [];

        // Navigazione lato client se il server restituisce un payload navigate
        const navigatePayload = res.data?.navigate
          ?? res.data?.actionResults?.find(r => r.navigate != null)?.navigate
          ?? null;
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

        const responseType = res.data?.responseType ?? '';

        if (this.speechService.VoiceCommandActive) {
          if (!this.isModalOpen) {
            if (responseType === 'result' || responseType === 'error') {
              // Comando completato: mostra toast con la risposta dell'assistente
              this.toastCtrl.create({
                message:  responce,
                duration: 3000,
                position: 'bottom'
              }).then(t => t.present());
            } else if (responseType === 'confirmation') {
              // Modal chiusa + vocale: auto-conferma senza aprire la modal
              setTimeout(() => this.sendSuggestion('Sì'), 600);
            } else {
              // question: la conversazione richiede input manuale → apri la modal
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
        this.pushMessage('Assistant', 'Errore di comunicazione con il server.', 'error');
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

  // Converte \n in <br> per la visualizzazione HTML nel template
  formatText(text: string): string {
    return (text ?? '').replace(/\n/g, '<br>');
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
  // Comandi vocali
  // -------------------------------------------------------------------------

  toggleVoiceCommand() {
    this.speechService.VoiceCommandActive = !this.speechService.VoiceCommandActive;
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

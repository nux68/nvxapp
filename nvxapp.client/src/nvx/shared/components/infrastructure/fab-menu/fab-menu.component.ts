import { Component, OnInit, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { IonContent, IonModal, ToastController } from '@ionic/angular';
import { SpeechService } from '../../../../Utility/infrastructure/speech.service';
import { ChatAIService } from '../../../../ClientServer-Service/Infrastructure/ChatAI/chat-ai.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { ChatAIInModel } from '../../../../ClientServer-Service/Infrastructure/ChatAI/Models/chat-AI-model';
import { FabMenuService } from '../../../../Utility/infrastructure/fab-menu.service';
import { IonFab } from '@ionic/angular';

@Component({
  selector: 'app-fab-menu',
  templateUrl: './fab-menu.component.html',
  styleUrls: ['./fab-menu.component.scss'],
  standalone: false
})
export class FabMenuComponent implements OnInit {

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
    private toastCtrl:     ToastController
  ) {
    this.fabMenuService.fabMenuItem$.subscribe(() => {
      if (this.fab) this.fab.close();
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

    // Mostra toast con il testo riconosciuto solo se la modal è chiusa
    if (!this.isModalOpen) {
      this.toastCtrl.create({
        message: text,
        duration: 1000,
        position: 'middle'
      }).then(t => t.present());
    }

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

        // In modalità vocale, auto-conferma senza chiedere all'utente
        if (this.speechService.VoiceCommandActive && res.data?.responseType === 'confirmation') {
          setTimeout(() => this.sendSuggestion('Sì'), 600);
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

}

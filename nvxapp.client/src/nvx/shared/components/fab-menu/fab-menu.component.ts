import { Component, OnInit, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { IonContent, IonModal } from '@ionic/angular';
import { SpeechService } from '../../../Utility/speech.service';
import { ChatAIService } from '../../../ClientServer-Service/ChatAI/chat-ai.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ChatAIInModel } from '../../../ClientServer-Service/ChatAI/Models/chat-AI-model';
import { FabMenuService } from '../../../Utility/fab-menu.service';
import { IonFab } from '@ionic/angular';

@Component({
  selector: 'app-fab-menu',
  templateUrl: './fab-menu.component.html',
  styleUrls: ['./fab-menu.component.scss'],
  standalone: false
})
export class FabMenuComponent implements OnInit {
  public messages: Array<{ sender: string, text: string }> = [];
  public newMessage: string = '';
  @ViewChild('chatContainer', { static: false }) chatContainer!: ElementRef;
  @ViewChild(IonContent, { static: false }) chatContent!: IonContent;
  @ViewChild('chatModal', { static: true }) chatModal!: IonModal;
  public isChatLoading: boolean = false;
  @ViewChild('fab', { static: false }) fab: IonFab;

  isModalOpen = false; // Stato del modale
  modalType: string | null = null; // Tipo di modale (chat o altro)

  constructor(public speechService: SpeechService,
              private chatAIService: ChatAIService,
              private cdRef: ChangeDetectorRef,
              public fabMenuService: FabMenuService) {

    

              this.fabMenuService.fabMenuItem$.subscribe((data) => {
                if (this.fab) {
                  this.fab.close();
                }
              });


  }

  ngOnInit() {

    this.speechService.Message$.subscribe(msg => {
      if (msg) {
        this.sendVoiceMessage(msg);
        this.cdRef.detectChanges();
      }
    });

    this.speechService.VoiceCommandActive$.subscribe(res => {

      this.cdRef.detectChanges();

    });

  }

  

  // Funzione per inviare un messaggio
  sendMessage() {
    if (this.newMessage.trim() !== '') {
      this.messages.push({ sender: 'You', text: this.newMessage });
      this.scrollToBottom(); // Scroll automatico

      let request: GenericRequest<ChatAIInModel> = new GenericRequest<ChatAIInModel>(ChatAIInModel);
      request.data.request = this.newMessage;

      this.isChatLoading = true;
      this.chatAIService.SendMessage(request).subscribe((res) => {
        this.isChatLoading = false;
        this.scrollToBottom(); // Scroll automatico
        this.AssistantReply(res.data.responce); // Simula una risposta dell'assistente
        this.newMessage = ''; // Resetta il campo di input

      }
        //,
        //(error) => {
        //this.isChatLoading = false;
        //}

      );
      
      
    }
  }

  // Funzione per inviare un messaggio vocale
  sendVoiceMessage(message: string) {
    if (message.trim() !== '') {
      this.messages.push({ sender: 'You', text: message });
      this.scrollToBottom(); // Scroll automatico

      let request: GenericRequest<ChatAIInModel> = new GenericRequest<ChatAIInModel>(ChatAIInModel);
      request.data.request = message;

      this.isChatLoading = true;
      this.chatAIService.SendMessage(request).subscribe(res => {
        this.isChatLoading = false;
        this.scrollToBottom(); // Scroll automatico
        this.AssistantReply(message); // Simula una risposta dell'assistente
        //this.newMessage = ''; // Resetta il campo di input

      }
        //,
        //(error) => {
        //  this.isChatLoading = false;
        //}
      );
      
    }
  }

  // Simula una risposta automatica dell'assistente
  AssistantReply(myMessage:string) {
    //setTimeout(() => {
      this.messages.push({ sender: 'Assistant', text: myMessage });
      this.cdRef.detectChanges();
      this.scrollToBottom(); // Scroll automatico dopo la risposta
    //}, 100);
  }

  // Funzione per scrollare in basso
  scrollToBottom() {
    setTimeout(() => {
      this.chatContent.scrollToBottom(300); // Scroll fluido
    }, 100);
  }

  ngAfterViewInit() {
    if (this.chatContainer) {
      this.scrollToBottom();
    }
  }

  // Apertura del modale
  openModal(type: string) {
    this.modalType = type; // Imposta il tipo di modale (chat o altro)
    this.isModalOpen = true; // Mostra il modale
  }

  // Chiusura del modale
  cancelChat() {
    this.chatModal.dismiss(null, 'cancel');
  }

  // Quando il modale viene chiuso
  onWillDismissChat(event: any) {
    console.log('Modale chiuso con tipo:', this.modalType);
    this.isModalOpen = false;
    this.modalType = null;
  }

  // Gestione dell'attivazione/disattivazione dei comandi vocali
  toggleVoiceCommand() {

    this.speechService.VoiceCommandActive = !this.speechService.VoiceCommandActive;

  }



}

import { Component, OnInit, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { IonContent, IonModal } from '@ionic/angular';
import { SpeechService } from '../../../Utility/speech.service';

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

  isModalOpen = false; // Stato del modale
  modalType: string | null = null; // Tipo di modale (chat o altro)

  isVoiceCommandActive = false; // Stato dei comandi vocali

  constructor(private speechService: SpeechService, private cdRef: ChangeDetectorRef) {


  }

  ngOnInit() {

    this.speechService.Message$.subscribe(msg => {
      if (msg) {
        this.sendVoiceMessage(msg);
        this.cdRef.detectChanges();
      }
    });

    this.speechService.VoiceCommandActive$.subscribe(res => {
      this.isVoiceCommandActive = res;
      this.cdRef.detectChanges();


      //if (this.isVoiceCommandActive) {
      ////  console.log('🎤 Comandi vocali attivati.');
      ////  this.speechService.startListening((text) => {
      ////    // Invia il messaggio appena viene riconosciuto il comando
      ////    //this.sendVoiceMessage(text);
      ////  });
      //} else {
      ////  console.log('⏹️ Comandi vocali disattivati.');
      ////  this.speechService.stopListening();
      //}


    });

    

  }

  

  // Funzione per inviare un messaggio
  sendMessage() {
    if (this.newMessage.trim() !== '') {
      this.messages.push({ sender: 'You', text: this.newMessage });
      this.newMessage = ''; // Resetta il campo di input
      this.scrollToBottom(); // Scroll automatico
      this.fakeAssistantReply(); // Simula una risposta dell'assistente
    }
  }

  // Funzione per inviare un messaggio vocale
  sendVoiceMessage(message: string) {
    if (message.trim() !== '') {
      this.messages.push({ sender: 'You', text: message });
      this.newMessage = ''; // Resetta il campo di input
      this.scrollToBottom(); // Scroll automatico
      this.fakeAssistantReply(); // Simula una risposta dell'assistente
    }
  }

  // Simula una risposta automatica dell'assistente
  fakeAssistantReply() {
    setTimeout(() => {
      this.messages.push({ sender: 'Assistant', text: 'Comando ricevuto!' });
      this.cdRef.detectChanges();
      this.scrollToBottom(); // Scroll automatico dopo la risposta
    }, 100);
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

    this.isVoiceCommandActive = !this.isVoiceCommandActive;

    this.speechService.VoiceCommandActive = this.isVoiceCommandActive;


    //this.isVoiceCommandActive = !this.isVoiceCommandActive;

    //if (this.isVoiceCommandActive) {
    //  console.log('🎤 Comandi vocali attivati.');
    //  this.speechService.startListening((text) => {
    //    // Invia il messaggio appena viene riconosciuto il comando
    //    //this.sendVoiceMessage(text);
    //  });
    //} else {
    //  console.log('⏹️ Comandi vocali disattivati.');
    //  this.speechService.stopListening();
    //}
  }



}

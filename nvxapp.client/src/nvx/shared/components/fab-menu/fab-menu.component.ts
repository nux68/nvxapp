import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { IonContent, IonModal } from '@ionic/angular';

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

  isModalOpen = false; // Controlla se il modal è aperto
  modalType: string | null = null; // Per salvare il tipo (chat o mic)
  isVoiceCommandActive = false; // Stato iniziale dei comandi vocali

  constructor() { }

  ngOnInit() { }

  // Funzione per inviare un messaggio
  sendMessage() {
    if (this.newMessage.trim() !== '') {
      this.messages.push({ sender: 'You', text: this.newMessage });
      this.newMessage = ''; // Resetta il campo di input
      this.scrollToBottom(); // Scroll automatico
      this.fakeAssistantReply(); // Simula una risposta dell'assistente
    }
  }

  // Simula una risposta automatica dell'assistente
  fakeAssistantReply() {
    setTimeout(() => {
      this.messages.push({ sender: 'Assistant', text: 'Grazie per il messaggio!' });
      this.scrollToBottom(); // Scroll automatico dopo la risposta
    }, 1000);
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


  openModal(type: string) {
    this.modalType = type; // Imposta il valore della variabile
    this.isModalOpen = true; // Mostra il modal
  }

  cancelChat() {
    this.chatModal.dismiss(null, 'cancel');
  }

  onWillDismissChat(event: any) {
    // Logica aggiuntiva qui, se necessaria
    this.isModalOpen = false;
    this.modalType = null;
  }

  toggleVoiceCommand() {
    this.isVoiceCommandActive = !this.isVoiceCommandActive;
    if (this.isVoiceCommandActive) {
      console.log('Comandi vocali attivati.');
      // Qui puoi aggiungere la logica per avviare i comandi vocali
    } else {
      console.log('Comandi vocali disattivati.');
      // Qui puoi aggiungere la logica per fermare i comandi vocali
    }
  }

}

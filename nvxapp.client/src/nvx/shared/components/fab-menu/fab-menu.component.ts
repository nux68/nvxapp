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
  @ViewChild(IonModal) modal!: IonModal;

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



  cancel() {
    this.modal.dismiss(null, 'cancel');
  }

  onWillDismiss(event: any) {
    // Logica aggiuntiva qui, se necessaria
  }
}

import { Injectable } from '@angular/core';
import { Platform } from '@ionic/angular';

@Injectable({
  providedIn: 'root'
})
export class UserInterfaceService {

  constructor(private platform: Platform) { }

  public get isMobile(): boolean {
    return this.platform.width() < 576;
  }

  get Btn_Conferma(): ButtonItem {
    return new ButtonItem(
      'Conferma',
      'checkmark-circle-outline',
      'primary',
      false,
      () => { console.log('Btn_Conferma cliccato'); }
    );
  }

  get Btn_Annulla(): ButtonItem {
    return new ButtonItem(
      'Annulla',
      'close-circle-outline',
      'medium',
      false,
      () => { console.log('Btn_Annulla cliccato'); }
    );
  }

  get Btn_ConfermaAnnulla(): ButtonItem[] {

    let buttonItem: ButtonItem[] = [];

    const btn_Conferma = this.Btn_Conferma;
    buttonItem.push(btn_Conferma);

    const btn_Annulla = this.Btn_Annulla;
    buttonItem.push(btn_Annulla);

    return buttonItem;

  }

  get Btn_Modifica(): ButtonItem {
    return new ButtonItem(
      'Modifica',
      'create-outline',
      'medium',
      false,
      () => { console.log('Btn_Modifica cliccato'); }
    );
  }

  get Btn_Impersona(): ButtonItem {
    return new ButtonItem(
      'Impersona',
      'person-outline',
      'primary',
      false,
      () => { console.log('Btn_Impersona cliccato'); }
    );
  }

  get Btn_Chiudi(): ButtonItem {
    return new ButtonItem(
      'Chiudi',
      'close-outline',
      'medium',
      false,
      () => { console.log('Btn_Chiudi cliccato'); }
    );
  }



  get Btn_LogIn(): ButtonItem {
    return new ButtonItem(
      'Login',
      'log-in-outline',
      'primary',
      false,
      () => { console.log('Btn_LogIn cliccato'); }
    );
  }
  get Btn_LogOut(): ButtonItem {
    return new ButtonItem(
      'Logout',
      'log-out-outline',
      'primary',
      false,
      () => { console.log('Btn_LogOut cliccato'); }
    );
  }

  get Btn_LogInAnnulla(): ButtonItem[] {

    let buttonItem: ButtonItem[] = [];

    const btn_LogIn = this.Btn_LogIn;
    buttonItem.push(btn_LogIn);

    const btn_Annulla = this.Btn_Annulla;
    buttonItem.push(btn_Annulla);

    return buttonItem;

  }

  get Btn_LogOutAnnulla(): ButtonItem[] {

    let buttonItem: ButtonItem[] = [];

    const btn_LogOut = this.Btn_LogOut;
    buttonItem.push(btn_LogOut);

    const btn_Annulla = this.Btn_Annulla;
    buttonItem.push(btn_Annulla);

    return buttonItem;

  }

  get Btn_Invia(): ButtonItem {
    return new ButtonItem(
      'Invia',
      'save',
      'primary',
      false,
      () => { console.log('Btn_Invia cliccato'); }
    );
  }
  get Btn_Clock(): ButtonItem {
    return new ButtonItem(
      'Clock',
      'time-outline',
      'primary',
      false,
      () => { console.log('Btn_Clock cliccato'); }
    );
  }



  get Btn_Approva(): ButtonItem {
    return new ButtonItem(
      'Approva',
      'checkmark-outline',
      'primary',
      false,
      () => { console.log('Btn_Approva cliccato'); }
    );
  }

  get Btn_Rifiuta(): ButtonItem {
    return new ButtonItem(
      'Rifiuta',
      'close-outline',
      'danger',
      false,
      () => { console.log('Btn_Rifiuta cliccato'); }
    );
  }

  get Btn_Cancella(): ButtonItem {
    return new ButtonItem(
      'Cancella',
      'trash-outline',
      'danger',
      false,
      () => { console.log('Btn_Cancella cliccato'); }
    );
  }

  get Btn_Esegui(): ButtonItem {
    return new ButtonItem(
      'Esegui',
      'flash-outline',
      'secondary',
      false,
      () => { console.log('Btn_Esegui cliccato'); }
    );
  }

  get Btn_Aggiungi(): ButtonItem {
    return new ButtonItem(
      'Aggiungi',
      'add-circle-outline',
      'primary',
      false,
      () => { console.log('Btn_Aggiungi cliccato'); }
    );
  }

  //trash-outline

}


export class ButtonItem {

  constructor(
    public text: string | null = null,
    public image: string | null = null,
    public color: string | null = null,
    public disabled: boolean = false,
    public event: (param: any) => void = (param: any) => { } // Arrow function di default

  ) { }

}

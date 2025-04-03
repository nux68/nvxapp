import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UserInterfaceService {

  constructor() { }


  get Btn_Conferma(): ButtonItem {
    return new ButtonItem(
      'Conferma',
      'checkmark-circle-outline',
      'primary',                 
      () => { console.log('Btn_Conferma cliccato'); } 
    );
  }

  get Btn_Annulla(): ButtonItem {
    return new ButtonItem(
      'Annulla',
      'close-circle-outline',
      'medium',
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

}


export class ButtonItem {

  constructor(
    public text: string | null = null,
    public image: string | null = null,
    public color: string | null = null,
    public event: () => void = () => { } // Arrow function di default
  ) { }

}

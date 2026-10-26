import { Injectable, Signal, signal } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

@Injectable({
  providedIn: 'root'
})
export class UserInterfaceService {

  // stesso limite delle classi ion-hide-sm-* di Ionic (576px); segue il ridimensionamento della finestra
  private readonly mobileQuery = window.matchMedia('(max-width: 575.98px)');
  private readonly _isMobile = signal(this.mobileQuery.matches);
  public readonly isMobileSignal: Signal<boolean> = this._isMobile.asReadonly();

  constructor(private transloco: TranslocoService) {
    this.mobileQuery.addEventListener('change', e => this._isMobile.set(e.matches));
  }

  public get isMobile(): boolean {
    return this._isMobile();
  }

  get Btn_Conferma(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.confirm'),
      'checkmark-circle-outline',
      'primary',
      false,
      () => { console.log('Btn_Conferma cliccato'); }
    );
  }

  get Btn_Annulla(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.cancel'),
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
      this.transloco.translate('common.buttons.edit'),
      'create-outline',
      'medium',
      false,
      () => { console.log('Btn_Modifica cliccato'); }
    );
  }

  get Btn_Impersona(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.impersonate'),
      'person-outline',
      'primary',
      false,
      () => { console.log('Btn_Impersona cliccato'); }
    );
  }

  get Btn_Chiudi(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.close'),
      'close-outline',
      'medium',
      false,
      () => { console.log('Btn_Chiudi cliccato'); }
    );
  }



  get Btn_LogIn(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.login'),
      'log-in-outline',
      'primary',
      false,
      () => { console.log('Btn_LogIn cliccato'); }
    );
  }
  get Btn_LogOut(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.logout'),
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
      this.transloco.translate('common.buttons.send'),
      'save',
      'primary',
      false,
      () => { console.log('Btn_Invia cliccato'); }
    );
  }
  get Btn_Clock(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.clock'),
      'time-outline',
      'primary',
      false,
      () => { console.log('Btn_Clock cliccato'); }
    );
  }



  get Btn_Approva(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.approve'),
      'checkmark-outline',
      'primary',
      false,
      () => { console.log('Btn_Approva cliccato'); }
    );
  }

  get Btn_Rifiuta(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.reject'),
      'close-outline',
      'danger',
      false,
      () => { console.log('Btn_Rifiuta cliccato'); }
    );
  }

  get Btn_Cancella(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.delete'),
      'trash-outline',
      'danger',
      false,
      () => { console.log('Btn_Cancella cliccato'); }
    );
  }

  get Btn_Esegui(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.execute'),
      'flash-outline',
      'secondary',
      false,
      () => { console.log('Btn_Esegui cliccato'); }
    );
  }

  get Btn_Aggiungi(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.add'),
      'add-circle-outline',
      'primary',
      false,
      () => { console.log('Btn_Aggiungi cliccato'); }
    );
  }

  // dettaglio di un elemento in un dialog (es. la nota spese dalle liste e dai calendari delle presenze)
  get Btn_Dettaglio(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.detail'),
      'receipt-outline',
      'medium',
      false,
      () => { console.log('Btn_Dettaglio cliccato'); }
    );
  }

  get Btn_Apri(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.open'),
      'open-outline',
      'primary',
      false,
      () => { console.log('Btn_Apri cliccato'); }
    );
  }

  // ritorno all'elenco da una pagina di dettaglio (distinto da Btn_Chiudi e Btn_ChiudiTicket): bordo grigio come Modifica
  get Btn_Elenco(): ButtonItem {
    const button = new ButtonItem(
      this.transloco.translate('common.buttons.backToList'),
      'list-outline',
      'medium',
      false,
      () => { console.log('Btn_Elenco cliccato'); }
    );
    button.fill = 'outline';
    return button;
  }

  // azioni sui ticket di assistenza
  get Btn_ChiudiTicket(): ButtonItem {
    const button = new ButtonItem(
      this.transloco.translate('common.buttons.closeTicket'),
      'lock-closed-outline',
      'danger',
      false,
      () => { console.log('Btn_ChiudiTicket cliccato'); }
    );
    button.fill = 'outline';   // stesso stile a bordo delle altre azioni secondarie (il colore resta)
    return button;
  }

  get Btn_Assegna(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.assign'),
      'person-add-outline',
      'primary',
      false,
      () => { console.log('Btn_Assegna cliccato'); }
    );
  }

  get Btn_Inoltra(): ButtonItem {
    const button = new ButtonItem(
      this.transloco.translate('common.buttons.forward'),
      'arrow-up-circle-outline',
      'warning',
      false,
      () => { console.log('Btn_Inoltra cliccato'); }
    );
    button.fill = 'outline';   // stesso stile a bordo delle altre azioni secondarie (il colore resta)
    return button;
  }

  get Btn_RichiediInfo(): ButtonItem {
    const button = new ButtonItem(
      this.transloco.translate('common.buttons.requestInfo'),
      'help-circle-outline',
      'medium',
      false,
      () => { console.log('Btn_RichiediInfo cliccato'); }
    );
    button.fill = 'outline';   // azione secondaria: bordo grigio come Btn_Elenco
    return button;
  }

  get Btn_Rispondi(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.reply'),
      'arrow-undo-outline',
      'primary',
      false,
      () => { console.log('Btn_Rispondi cliccato'); }
    );
  }

  get Btn_Nota(): ButtonItem {
    const button = new ButtonItem(
      this.transloco.translate('common.buttons.note'),
      'document-text-outline',
      'medium',
      false,
      () => { console.log('Btn_Nota cliccato'); }
    );
    button.fill = 'outline';   // azione secondaria: bordo grigio come Btn_Elenco
    return button;
  }

  get Btn_Risolvi(): ButtonItem {
    return new ButtonItem(
      this.transloco.translate('common.buttons.resolve'),
      'checkmark-done-outline',
      'primary',
      false,
      () => { console.log('Btn_Risolvi cliccato'); }
    );
  }

  get Btn_Riapri(): ButtonItem {
    const button = new ButtonItem(
      this.transloco.translate('common.buttons.reopen'),
      'refresh-outline',
      'warning',
      false,
      () => { console.log('Btn_Riapri cliccato'); }
    );
    button.fill = 'outline';   // stesso stile a bordo delle altre azioni secondarie (il colore resta)
    return button;
  }

  get Btn_Riclassifica(): ButtonItem {
    const button = new ButtonItem(
      this.transloco.translate('common.buttons.reclassify'),
      'pricetags-outline',
      'medium',
      false,
      () => { console.log('Btn_Riclassifica cliccato'); }
    );
    button.fill = 'outline';   // azione secondaria: bordo grigio come Btn_Elenco
    return button;
  }

  // esportazione dei dati: generica, in PDF, in Excel
  get Btn_Export(): ButtonItem {
    const button = new ButtonItem(
      this.transloco.translate('common.buttons.export'),
      'download-outline',
      'medium',
      false,
      () => { console.log('Btn_Export cliccato'); }
    );
    button.fill = 'outline';   // azione secondaria: bordo grigio come Btn_Elenco
    return button;
  }

  get Btn_Pdf(): ButtonItem {
    const button = new ButtonItem(
      this.transloco.translate('common.buttons.pdf'),
      'document-outline',
      'danger',
      false,
      () => { console.log('Btn_Pdf cliccato'); }
    );
    button.fill = 'outline';   // stesso stile a bordo delle altre azioni secondarie (rosso come i file PDF)
    return button;
  }

  get Btn_Excel(): ButtonItem {
    const button = new ButtonItem(
      this.transloco.translate('common.buttons.excel'),
      'grid-outline',
      'success',
      false,
      () => { console.log('Btn_Excel cliccato'); }
    );
    button.fill = 'outline';   // stesso stile a bordo delle altre azioni secondarie (verde come Excel)
    return button;
  }

  //trash-outline

}


export class ButtonItem {

  // stile nella buttonbar; null = di default (pieno se primary, altrimenti senza bordo)
  public fill: 'solid' | 'outline' | 'clear' | null = null;

  constructor(
    public text: string | null = null,
    public image: string | null = null,
    public color: string | null = null,
    public disabled: boolean = false,
    public event: (param: any) => void = (param: any) => { } // Arrow function di default

  ) { }

}

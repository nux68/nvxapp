import { Injectable } from '@angular/core';
import { ModalController } from '@ionic/angular';
import { NotaSpesaDettaglioDialogComponent } from './nota-spesa-dettaglio-dialog.component';

@Injectable({
  providedIn: 'root'
})
// apre il dettaglio di una nota spese dalle pagine che la mostrano (esame spese, richieste, calendari)
export class NotaSpesaDettaglioDialogService {

  constructor(private modalCtrl: ModalController) { }

  /**
   * Mostra la nota spese (richiesta idDip_GG_Richiesta); con consentiDecisione anche approva / rifiuta.
   * Restituisce true se la nota e' stata approvata o rifiutata (la pagina che l'ha aperta si rilegge).
   */
  async Apri(idDip_GG_Richiesta: number, dipendente: string = '', consentiDecisione: boolean = false): Promise<boolean> {
    const modal = await this.modalCtrl.create({
      component: NotaSpesaDettaglioDialogComponent,
      componentProps: { idDip_GG_Richiesta, dipendente, consentiDecisione },
    });
    await modal.present();
    const { role } = await modal.onWillDismiss();
    return role === 'confirm';
  }
}

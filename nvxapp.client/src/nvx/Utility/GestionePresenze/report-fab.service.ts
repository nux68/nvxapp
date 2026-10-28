import { Injectable } from '@angular/core';
import { ActionSheetController, ToastController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { ReportService } from '../../ClientServer-Service/GestionePresenze/Report/report.service';
import { Report_InModel, ReportModel, ReportTipo } from '../../ClientServer-Service/GestionePresenze/Report/Models/report-model';
import { FabMenuItem } from '../infrastructure/fab-menu.service';
import { UserInterfaceService } from '../infrastructure/user-interface.service';

/** report offerto da una pagina: tipo e chiave della traduzione del nome (per la scelta tra piu' report) */
export interface ReportScelta {
  tipo: ReportTipo;
  testo: string;
}

/*
 Pulsante Report del FAB delle pagine HR (statistiche attivita', piano ferie, quadratura mensile, note spese).
 Con un solo report il PDF parte subito, con piu' report si sceglie quale da un action sheet.
 La pagina fornisce i dati della richiesta (periodo, dipendenti o reparti selezionati, filtri): il server
 avvia il job e il file si scarica dalla notifica di fine job (barra della pagina), come il cartellino PDF.
*/
@Injectable({
  providedIn: 'root'
})
export class ReportFabService {

  constructor(private reportService: ReportService,
              private actionSheetCtrl: ActionSheetController,
              private toastCtrl: ToastController,
              private userInterfaceService: UserInterfaceService,
              private transloco: TranslocoService) { }

  /** voce del FAB: <paramref name="richiesta"/> restituisce i dati del report scelto (null = niente da stampare) */
  FabItem(reports: ReportScelta[], richiesta: (tipo: ReportTipo) => Omit<ReportModel, 'tipo'> | null): FabMenuItem {
    const btnPdf = this.userInterfaceService.Btn_Pdf;
    return new FabMenuItem(this.transloco.translate('attendance.services.report.titolo'), btnPdf.image, () => {
      this.scegli(reports, richiesta);
    });
  }

  private async scegli(reports: ReportScelta[], richiesta: (tipo: ReportTipo) => Omit<ReportModel, 'tipo'> | null) {
    if (reports.length === 1) {
      this.avvia(reports[0].tipo, richiesta);
      return;
    }

    const sheet = await this.actionSheetCtrl.create({
      header: this.transloco.translate('attendance.services.report.titolo'),
      buttons: [
        ...reports.map(r => ({
          text: this.transloco.translate(r.testo),
          icon: 'document-text-outline',
          handler: () => this.avvia(r.tipo, richiesta)
        })),
        { text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' }
      ]
    });
    await sheet.present();
  }

  private avvia(tipo: ReportTipo, richiesta: (tipo: ReportTipo) => Omit<ReportModel, 'tipo'> | null) {
    const dati = richiesta(tipo);
    if (!dati || !dati.year || !dati.month)
      return;

    const request = new GenericRequest<Report_InModel>(Report_InModel);
    Object.assign(request.data.report, dati, { tipo });
    this.reportService.ReportExportPdf(request).subscribe(async res => {
      // job non avviato (es. nessun dipendente da stampare): avviso del server
      const messaggi = res.success ? (res.data?.messages ?? []) : [];
      if (messaggi.length > 0) {
        const toast = await this.toastCtrl.create({ message: messaggi.map(m => m.text).join(' '), duration: 4000, color: 'warning', position: 'top' });
        await toast.present();
      }
    });
  }
}

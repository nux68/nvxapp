import { Injectable } from '@angular/core';
import { iRequestUiProvider, RequestNewItem, RequestUiAction } from './request-ui.model';
import { UserInterfaceService } from './user-interface.service';
import { NotaSpesaDettaglioDialogService } from '../../shared/components/GestionePresenze/nota-spesa-dettaglio-dialog/nota-spesa-dettaglio-dialog.service';

/*
 Azioni e pagine di invio delle richieste di Gestione Presenze nelle pagine comuni delle richieste
 (ANALISI_INFRASTRUCTURE_RICHIESTE.md, RICH-AT.2):
   - "at.notaSpesa": dettaglio della nota spese (spese e allegati) in sola lettura; si approva e si rifiuta dalla riga
   - nuove richieste: giustificativo, timbratura, rimborso spese (come le voci del menu del dipendente)
*/
@Injectable({
  providedIn: 'root'
})
export class RequestUiAttendanceTrackingService implements iRequestUiProvider {

  constructor(private userInterfaceService: UserInterfaceService,
              private notaSpesaDettaglioDialogService: NotaSpesaDettaglioDialogService) { }

  public get Actions(): RequestUiAction[] {
    return [
      {
        extra: 'at.notaSpesa',
        button: () => this.userInterfaceService.Btn_Dettaglio,
        run: item => { this.notaSpesaDettaglioDialogService.Apri(Number(item.id), item.richiedente ?? ''); },
      },
    ];
  }

  public get NewRequests(): RequestNewItem[] {
    return [
      { title: 'attendance.menu.richiestaGiustificativo', url: '/requestjustificationuser', roles: ['User'] },
      { title: 'attendance.menu.richiestaTimbratura', url: '/requestclockinguser', roles: ['User'] },
      { title: 'attendance.menu.richiestaRimborso', url: '/notaspesarequestuser', roles: ['User'] },
    ];
  }
}

import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular/lazy';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { Dip_GG_Richiesta_GetAll4User_InModel, Dip_GG_Richiesta_SetState_InModel, Dip_GG_RichiestaModel, notaSpesaModificabile, riepilogoNotaSpesa, StatoRichiesta, TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { DipGGNotaSpesaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_NotaSpesa/dip-gg-nota-spesa.service';
import { Dip_GG_NotaSpesa_AllegatoModel, Dip_GG_NotaSpesa_Get4Richiesta_InModel, Dip_GG_NotaSpesaModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_NotaSpesa/Models/dip-gg-nota-spesa-model';
import { ParTipoSpesaService } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/par-tipo-spesa.service';
import { Par_TipoSpesaInModel, Par_TipoSpesaModel } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/Models/par-tipo-spesa-model';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';


/*
 Le mie note spese (utente): elenco delle note inviate con stato, totale e, aperta una nota,
 le sue spese con gli allegati. Le note in attesa si possono annullare, quelle approvate revocare.
*/
@Component({
  selector: 'app-nota-spese-user-page',
  templateUrl: './nota-spese-user-page.component.html',
  styleUrls: ['./nota-spese-user-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class NotaSpeseUserPageComponent {

  public readonly title = 'Le mie note spese';
  public note: Dip_GG_RichiestaModel[] | null = null;
  public tipiSpesa: Par_TipoSpesaModel[] = [];

  /** spese delle note aperte, per id richiesta */
  public dettagli = new Map<number, Dip_GG_NotaSpesaModel[]>();
  public aperta: number | null = null;

  constructor(private navCtrl: NavController,
              private dipGGRichiestaService: DipGGRichiestaService,
              private dipGGNotaSpesaService: DipGGNotaSpesaService,
              private parTipoSpesaService: ParTipoSpesaService,
              public timeSheetService: TimeSheetService,
              private collectionDialogService: CollectionDialogService,
              private fabMenuService: FabMenuService) { }

  ionViewWillEnter() {
    this.carica();
    this.parTipoSpesaService.GetAll(new GenericRequest<Par_TipoSpesaInModel>(Par_TipoSpesaInModel)).subscribe(res => {
      this.tipiSpesa = res.data?.par_TipoSpesa ?? [];
    });
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuova nota spese', 'add-circle-outline', () => this.navCtrl.navigateForward('/notaspesarequestuser')),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  private carica(): void {
    const request = new GenericRequest<Dip_GG_Richiesta_GetAll4User_InModel>(Dip_GG_Richiesta_GetAll4User_InModel);
    request.data.year = 0;
    request.data.month = 0;
    this.dipGGRichiestaService.GetAll4User(request).subscribe(res => {
      this.note = (res.data?.dip_GG_Richiesta ?? [])
        .filter(x => x.richiestaTipo === TipoRichiesta.NotaSpesa)
        .sort((a, b) => b.id - a.id);
      this.dettagli.clear();
      this.aperta = null;
    });
  }

  public nuova(): void {
    this.navCtrl.navigateForward('/notaspesarequestuser');
  }

  public riepilogo(nota: Dip_GG_RichiestaModel) {
    return riepilogoNotaSpesa(nota.dati);
  }

  public euro(valore: number): string {
    return '€ ' + (valore ?? 0).toFixed(2).replace('.', ',');
  }

  public tipo(id: number): string {
    return this.tipiSpesa.find(t => t.id === id)?.descrizione ?? '';
  }

  public apriChiudi(nota: Dip_GG_RichiestaModel): void {
    if (this.aperta === nota.id) {
      this.aperta = null;
      return;
    }
    this.aperta = nota.id;
    if (this.dettagli.has(nota.id))
      return;
    const request = new GenericRequest<Dip_GG_NotaSpesa_Get4Richiesta_InModel>(Dip_GG_NotaSpesa_Get4Richiesta_InModel);
    request.data.idDip_GG_Richiesta = nota.id;
    this.dipGGNotaSpesaService.Get4Richiesta(request).subscribe(res => {
      this.dettagli.set(nota.id, res.data?.dip_GG_NotaSpesa ?? []);
    });
  }

  public apriAllegato(allegato: Dip_GG_NotaSpesa_AllegatoModel): void {
    this.dipGGNotaSpesaService.ApriAllegato(allegato);
  }

  /** stato della nota da mostrare, tenendo conto della revoca di una nota approvata */
  public stato(nota: Dip_GG_RichiestaModel): { testo: string | null, colore: string } {
    if (nota.revocaStato === StatoRichiesta.Approvata)
      return { testo: 'Revocata', colore: 'dark' };
    if (nota.revocaStato === StatoRichiesta.Immessa || nota.revocaStato === StatoRichiesta.ApprovazioneInCorso)
      return { testo: 'Revoca in attesa', colore: 'warning' };
    switch (nota.richiestaStato) {
      case StatoRichiesta.Approvata: return { testo: null, colore: 'success' };
      case StatoRichiesta.Rifiutata: return { testo: null, colore: 'danger' };
      case StatoRichiesta.Cancellata: return { testo: null, colore: 'dark' };
      default: return { testo: null, colore: 'medium' };
    }
  }

  /** finche' non e' approvata la nota si puo' modificare */
  public puoModificare(nota: Dip_GG_RichiestaModel): boolean {
    return notaSpesaModificabile(nota);
  }

  public modifica(nota: Dip_GG_RichiestaModel): void {
    this.navCtrl.navigateForward('/notaspesarequestuser', { state: { idRichiesta: nota.id } });
  }

  public puoAnnullare(nota: Dip_GG_RichiestaModel): boolean {
    return this.timeSheetService.Dip_GG_Richiesta_User_Can_Delete(nota);
  }

  /** annulla una nota in attesa, oppure chiede la revoca di una nota gia' approvata */
  public async annulla(nota: Dip_GG_RichiestaModel): Promise<void> {
    const revoca = nota.richiestaStato === StatoRichiesta.Approvata;
    const conferma = await this.collectionDialogService.ConfirmCancelDialog(revoca
      ? 'La nota spese è già approvata: confermi la richiesta di revoca?'
      : 'Confermi l\'annullamento della nota spese?');
    if (!conferma)
      return;

    const request = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>(Dip_GG_Richiesta_SetState_InModel);
    request.data.idDip_GG_Richiesta = [nota.id];
    request.data.richiestaStato = revoca ? StatoRichiesta.Immessa : StatoRichiesta.Cancellata;
    this.dipGGRichiestaService.SetState(request).subscribe(() => this.carica());
  }
}

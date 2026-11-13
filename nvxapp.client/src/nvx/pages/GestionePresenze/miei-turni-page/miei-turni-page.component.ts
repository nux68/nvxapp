import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ActionSheetController, ModalController, ToastController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { SeletionPeriodoDialogComponent, SeletionPeriodoDialogResult } from '../../../shared/components/GestionePresenze/seletion-periodo-dialog/seletion-periodo-dialog.component';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TurniService } from '../../../ClientServer-Service/GestionePresenze/Turni/turni.service';
import {
  Dip_TurnoScambioModel, Dip_TurnoScambio_AnnullaInModel, Dip_TurnoScambio_ProponiInModel, Dip_TurnoScambio_RispondiInModel,
  Dip_TurnoUserModel, Dip_Turno_GetAll4UserInModel, Dip_Turno_GetAll4UserOutModel, TurnoScambioStato, dataLocale,
  Dip_Turno_ReportInModel, TurniReportTipo
} from '../../../ClientServer-Service/GestionePresenze/Turni/Models/turni-model';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

/*
 I miei turni (AT-TURNI.3): turni delle prossime 4 settimane, proposta di scambio con il turno di un collega dello stesso
 reparto (o cessione), risposta alle proposte ricevute; dopo l'accettazione lo scambio va in approvazione (pagina Richieste).
 Report (FAB): settimana o mese scelti con il dialog standard del periodo, PDF con il job (il file si scarica dalla notifica).
*/
@Component({
  selector: 'app-miei-turni-page',
  templateUrl: './miei-turni-page.component.html',
  styleUrls: ['./miei-turni-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class MieiTurniPageComponent {

  public title: string;
  public dati: Dip_Turno_GetAll4UserOutModel | null = null;
  public readonly stato = TurnoScambioStato;

  constructor(private turniService: TurniService,
              private actionSheetCtrl: ActionSheetController,
              private collectionDialogService: CollectionDialogService,
              private modalCtrl: ModalController,
              private toastCtrl: ToastController,
              private fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.mieiTurni.title');
  }

  ionViewWillEnter() {
    this.carica();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.services.report.titolo'), this.userInterfaceService.Btn_Pdf.image, () => this.report()),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  /** report PDF dei miei turni: settimana o mese dal dialog standard, poi il job */
  async report() {
    const modal = await this.modalCtrl.create({
      component: SeletionPeriodoDialogComponent,
      componentProps: { title: this.transloco.translate('attendance.pages.mieiTurni.reportTitle') }
    });
    await modal.present();
    const { data, role } = await modal.onWillDismiss<SeletionPeriodoDialogResult>();
    if (role !== 'confirm' || !data)
      return;
    const request = new GenericRequest<Dip_Turno_ReportInModel>(Dip_Turno_ReportInModel);
    request.data.tipo = TurniReportTipo.Miei;
    request.data.dal = data.dal;
    request.data.al = data.al;
    this.turniService.ReportPdf(request).subscribe(async res => {
      const messaggi = res.success ? (res.data?.messages ?? []) : [];
      if (messaggi.length > 0) {
        const toast = await this.toastCtrl.create({ message: messaggi.map(m => m.text).join(' '), duration: 4000, color: 'warning', position: 'top' });
        await toast.present();
      }
    });
  }

  carica() {
    const oggi = new Date();
    const al = new Date(oggi);
    al.setDate(al.getDate() + 27);
    const request = new GenericRequest<Dip_Turno_GetAll4UserInModel>(Dip_Turno_GetAll4UserInModel);
    request.data.dal = dataLocale(oggi);
    request.data.al = dataLocale(al);
    this.turniService.GetAll4User(request).subscribe(res => this.dati = res.data ?? null);
  }

  get daRispondere(): Dip_TurnoScambioModel[] { return this.dati?.scambi.filter(s => s.daRispondere) ?? []; }
  get altriScambi(): Dip_TurnoScambioModel[] { return this.dati?.scambi.filter(s => !s.daRispondere) ?? []; }

  statoTesto(s: Dip_TurnoScambioModel): string {
    return this.transloco.translate('attendance.pages.mieiTurni.state' + s.stato);
  }

  async proponi(t: Dip_TurnoUserModel) {
    if (!this.dati)
      return;
    const buttons: any[] = this.dati.turniColleghi.map(c => ({
      text: `${c.nominativo}: ${c.data.substring(8, 10)}/${c.data.substring(5, 7)} ${c.codice}`,
      icon: 'swap-horizontal-outline',
      handler: () => { this.invia(t, c.id, null); }
    }));
    // cessione a un collega: con piu' turni nel giorno anche a chi ne ha gia' (indicati); la compatibilita' la controlla il server
    const delGiorno = this.dati.turniColleghi.filter(c => c.data.substring(0, 10) === t.data.substring(0, 10));
    this.dati.colleghi.forEach(c => buttons.push({
      text: this.transloco.translate('attendance.pages.mieiTurni.giveTo', { name: c.nominativo })
            + (delGiorno.some(x => x.idAspNetUsers === c.idAspNetUsers)
               ? ' (' + delGiorno.filter(x => x.idAspNetUsers === c.idAspNetUsers).map(x => x.codice).join(', ') + ')' : ''),
      icon: 'arrow-forward-outline',
      handler: () => { this.invia(t, null, c.idAspNetUsers); }
    }));
    buttons.push({ text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' });
    const sheet = await this.actionSheetCtrl.create({
      header: this.transloco.translate('attendance.pages.mieiTurni.proposeHeader', { shift: t.codice, date: t.data.substring(8, 10) + '/' + t.data.substring(5, 7) }),
      buttons
    });
    await sheet.present();
  }

  private invia(t: Dip_TurnoUserModel, idTurnoCollega: number | null, idAspNetUsersCollega: string | null) {
    const request = new GenericRequest<Dip_TurnoScambio_ProponiInModel>(Dip_TurnoScambio_ProponiInModel);
    request.data.idDip_Turno = t.id;
    request.data.idDip_Turno_Collega = idTurnoCollega;
    request.data.idAspNetUsers_Collega = idAspNetUsersCollega;
    this.turniService.Proponi(request).subscribe(() => this.carica());
  }

  rispondi(s: Dip_TurnoScambioModel, accetta: boolean) {
    const request = new GenericRequest<Dip_TurnoScambio_RispondiInModel>(Dip_TurnoScambio_RispondiInModel);
    request.data.id = s.id;
    request.data.accetta = accetta;
    this.turniService.Rispondi(request).subscribe(() => this.carica());
  }

  async ritira(s: Dip_TurnoScambioModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.mieiTurni.withdrawConfirm')))
      return;
    const request = new GenericRequest<Dip_TurnoScambio_AnnullaInModel>(Dip_TurnoScambio_AnnullaInModel);
    request.data.id = s.id;
    this.turniService.Annulla(request).subscribe(() => this.carica());
  }
}

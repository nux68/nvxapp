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
 Vista: Tutti (da oggi per 62 giorni, il limite del server), Settimana o Mese con le frecce per spostarsi; i turni passati
 si vedono ma non si scambiano.
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
  /** turni mostrati: tutti i prossimi, una settimana o un mese */
  public vista: 'tutti' | 'settimana' | 'mese' = 'tutti';
  public inizio: Date = new Date();
  /** giorni della vista Tutti (limite del periodo del server: 62) */
  private static readonly GIORNI_TUTTI = 61;

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

  private oggiData(): Date {
    const d = new Date();
    return new Date(d.getFullYear(), d.getMonth(), d.getDate());
  }

  private lunedi(d: Date): Date {
    return new Date(d.getFullYear(), d.getMonth(), d.getDate() - ((d.getDay() + 6) % 7));
  }

  private periodo(): { dal: Date, al: Date } {
    if (this.vista === 'settimana')
      return { dal: this.inizio, al: new Date(this.inizio.getFullYear(), this.inizio.getMonth(), this.inizio.getDate() + 6) };
    if (this.vista === 'mese')
      return { dal: this.inizio, al: new Date(this.inizio.getFullYear(), this.inizio.getMonth() + 1, 0) };
    const oggi = this.oggiData();
    return { dal: oggi, al: new Date(oggi.getFullYear(), oggi.getMonth(), oggi.getDate() + MieiTurniPageComponent.GIORNI_TUTTI) };
  }

  get periodoTesto(): string {
    const { dal, al } = this.periodo();
    const lang = this.transloco.getActiveLang();
    const f = (d: Date) => d.toLocaleDateString(lang, { day: '2-digit', month: '2-digit', year: 'numeric' });
    if (this.vista === 'mese') {
      const nome = dal.toLocaleDateString(lang, { month: 'long', year: 'numeric' });
      return nome.charAt(0).toUpperCase() + nome.slice(1);
    }
    return `${f(dal)} - ${f(al)}`;
  }

  cambiaVista(v: any) {
    this.vista = v === 'settimana' || v === 'mese' ? v : 'tutti';
    const oggi = this.oggiData();
    this.inizio = this.vista === 'mese' ? new Date(oggi.getFullYear(), oggi.getMonth(), 1) : this.lunedi(oggi);
    this.carica();
  }

  sposta(verso: number) {
    this.inizio = this.vista === 'mese'
      ? new Date(this.inizio.getFullYear(), this.inizio.getMonth() + verso, 1)
      : new Date(this.inizio.getFullYear(), this.inizio.getMonth(), this.inizio.getDate() + 7 * verso);
    this.carica();
  }

  oggi() {
    this.cambiaVista(this.vista);
  }

  /** si scambiano solo i turni da oggi in poi (quelli passati il server li rifiuta) */
  puoScambiare(t: Dip_TurnoUserModel): boolean {
    return t.data.substring(0, 10) >= dataLocale(this.oggiData()).substring(0, 10);
  }

  carica() {
    const { dal, al } = this.periodo();
    const request = new GenericRequest<Dip_Turno_GetAll4UserInModel>(Dip_Turno_GetAll4UserInModel);
    request.data.dal = dataLocale(dal);
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
    const giorno = (x: { data: string }) => x.data.substring(0, 10);
    const colleghi = this.dati.turniColleghi;
    // il collega ha gia' quel turno in quel giorno (esclusa la riga indicata)
    const haTurno = (idAspNetUsers: string | null, data: string, idPar_Turno: number, esclusa?: number) =>
      colleghi.some(x => x.idAspNetUsers === idAspNetUsers && giorno(x) === data && x.idPar_Turno === idPar_Turno && x.id !== esclusa);
    // io ho gia' quel turno in quel giorno (esclusa la riga che cedo)
    const hoTurno = (data: string, idPar_Turno: number) =>
      this.dati!.mieiTurni.some(x => giorno(x) === data && x.idPar_Turno === idPar_Turno && x.id !== t.id);

    // scambio: turni dei colleghi, tolti quelli inutili (stesso turno nello stesso giorno) o che creerebbero un turno doppio
    // (il collega ha gia' il mio turno nel mio giorno, oppure io ho gia' il suo nel suo giorno); la compatibilita' degli
    // orari la controlla il server
    const buttons: any[] = colleghi
      .filter(c => this.puoScambiare(c))   // niente turni passati (vista settimana o mese)
      .filter(c => !(giorno(c) === giorno(t) && c.idPar_Turno === t.idPar_Turno))
      .filter(c => !haTurno(c.idAspNetUsers, giorno(t), t.idPar_Turno, c.id))
      .filter(c => !hoTurno(giorno(c), c.idPar_Turno))
      .sort((a, b) => giorno(a).localeCompare(giorno(b)) || (a.nominativo ?? '').localeCompare(b.nominativo ?? ''))
      .map(c => ({
        text: `${c.nominativo}: ${c.data.substring(8, 10)}/${c.data.substring(5, 7)} ${c.codice}`,
        icon: 'swap-horizontal-outline',
        handler: () => { this.invia(t, c.id, null); }
      }));
    // cessione a un collega che quel giorno non ha gia' lo stesso turno (gli altri suoi turni del giorno sono indicati;
    // la compatibilita' degli orari la controlla il server)
    const delGiorno = colleghi.filter(c => giorno(c) === giorno(t));
    this.dati.colleghi.filter(c => !haTurno(c.idAspNetUsers, giorno(t), t.idPar_Turno)).forEach(c => buttons.push({
      text: this.transloco.translate('attendance.pages.mieiTurni.giveTo', { name: c.nominativo })
            + (delGiorno.some(x => x.idAspNetUsers === c.idAspNetUsers)
               ? ' (' + delGiorno.filter(x => x.idAspNetUsers === c.idAspNetUsers).map(x => x.codice).join(', ') + ')' : ''),
      icon: 'arrow-forward-outline',
      handler: () => { this.invia(t, null, c.idAspNetUsers); }
    }));
    if (buttons.length === 0) {
      const toast = await this.toastCtrl.create({ message: this.transloco.translate('attendance.pages.mieiTurni.noCandidates'), duration: 4000, color: 'warning', position: 'top' });
      await toast.present();
      return;
    }
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

import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ActionSheetController, NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../ClientServer-Service/Assets/ast.service';
import * as F from '../../../ClientServer-Service/Assets/Models/ast-flotta-model';
import { AstFormatService, astNumero, astOggi, astPerInputData } from '../../../Utility/Assets/ast-format.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

export type AstVehicleItemKind = 'assegnazione' | 'scadenza' | 'costo' | 'multa';

@Component({
  selector: 'app-ast-vehicle-edit-page',
  templateUrl: './ast-vehicle-edit-page.component.html',
  styleUrls: ['./ast-vehicle-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Veicolo (amministratori, AST-FLO): dati del veicolo e, salvato il veicolo, assegnazioni (con fringe benefit), scadenze
 (nello scadenzario comune), costi e multe. Il + del fab apre il menu per scegliere cosa aggiungere.
*/
export class AstVehicleEditPageComponent extends BasePageConfirmCancelComponent<F.AstVeicoloModel> {

  public id = 0;
  public dati: F.AstVeicoloGetOutModel | null = null;
  public sezione: AstVehicleItemKind = 'assegnazione';
  public readonly sezioni: AstVehicleItemKind[] = ['assegnazione', 'scadenza', 'costo', 'multa'];
  public readonly alimentazioni = [0, 1, 2, 3, 4, 5, 6];
  public readonly possessi = [0, 1, 2];

  public readonly colAssegnazione: NvxListColumn<F.AstVeicoloAssegnazioneModel>[];
  public readonly colScadenza: NvxListColumn<F.AstVeicoloScadenzaModel>[];
  public readonly colCosto: NvxListColumn<F.AstVeicoloCostoModel>[];
  public readonly colMulta: NvxListColumn<F.AstMultaModel>[];
  public readonly actAssegnazione: NvxListAction<F.AstVeicoloAssegnazioneModel>[];
  public readonly actScadenza: NvxListAction<F.AstVeicoloScadenzaModel>[];
  public readonly actCosto: NvxListAction<F.AstVeicoloCostoModel>[];
  public readonly actMulta: NvxListAction<F.AstMultaModel>[];
  public readonly scadenzaEseguita = (x: F.AstVeicoloScadenzaModel) => !!x.dataEsecuzione;
  public readonly scadenzaScaduta = (x: F.AstVeicoloScadenzaModel) => !x.dataEsecuzione && (x.dataScadenza ?? '') < astOggi();

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              public fabMenuService: FabMenuService,
              private actionSheetCtrl: ActionSheetController,
              private collectionDialogService: CollectionDialogService,
              private service: AstService,
              public fmt: AstFormatService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    const t = (k: string) => this.transloco.translate(k);

    this.colAssegnazione = [
      { key: 'dipendente', title: t('ast.fields.dipendente'), mobile: 'title' },
      { key: 'dataInizio', title: t('ast.fields.dal'), value: x => this.fmt.data(x.dataInizio), sortValue: x => x.dataInizio },
      { key: 'dataFine', title: t('ast.fields.al'), value: x => this.fmt.data(x.dataFine), sortValue: x => x.dataFine ?? '9999' },
      { key: 'fringeAnnuo', title: t('ast.fields.fringeAnnuo'), align: 'end', mobile: 'note',
        value: x => x.usoPromiscuo ? this.fmt.importo(x.fringeAnnuo) + ' (' + x.percentualeFringe + '%)' : t('ast.pages.vehicleEdit.noFringe') },
      { key: 'fringeAnnoCorrente', title: t('ast.fields.fringeAnno'), align: 'end', mobile: 'hidden', value: x => this.fmt.importo(x.fringeAnnoCorrente) },
    ];
    this.colScadenza = [
      { key: 'dataScadenza', title: t('ast.fields.data'), mobile: 'note', value: x => this.fmt.data(x.dataScadenza), sortValue: x => x.dataScadenza },
      { key: 'tipo', title: t('ast.fields.tipo'), mobile: 'title', value: x => x.tipo === 4 && x.descrizione ? x.descrizione : t('ast.enums.tipoScadenza.' + x.tipo) },
      { key: 'km', title: t('ast.fields.km'), align: 'end', mobile: 'hidden' },
      { key: 'dataEsecuzione', title: t('ast.fields.eseguita'), value: x => this.fmt.data(x.dataEsecuzione), sortValue: x => x.dataEsecuzione ?? '' },
    ];
    this.colCosto = [
      { key: 'data', title: t('ast.fields.data'), mobile: 'note', value: x => this.fmt.data(x.data), sortValue: x => x.data },
      { key: 'tipo', title: t('ast.fields.tipo'), mobile: 'title', value: x => t('ast.enums.tipoCosto.' + x.tipo) },
      { key: 'descrizione', title: t('ast.fields.descrizione') },
      { key: 'importo', title: t('ast.fields.importo'), align: 'end', value: x => this.fmt.importo(x.importo), sortValue: x => x.importo },
    ];
    this.colMulta = [
      { key: 'dataInfrazione', title: t('ast.fields.data'), mobile: 'note', value: x => this.fmt.dataOra(x.dataInfrazione), sortValue: x => x.dataInfrazione },
      { key: 'conducente', title: t('ast.fields.conducente'), mobile: 'title' },
      { key: 'importo', title: t('ast.fields.importo'), align: 'end', value: x => this.fmt.importo(x.importo), sortValue: x => x.importo },
      { key: 'stato', title: t('ast.fields.stato'), mobile: 'subtitle', value: x => t('ast.enums.statoMulta.' + x.stato) },
    ];

    const modifica = <T>(kind: AstVehicleItemKind) => {
      const b = userInterfaceService.Btn_Modifica;
      b.event = (x: T) => this.apri(kind, x);
      return b;
    };
    const cancella = <T extends { id: number }>(kind: AstVehicleItemKind) => {
      const b = userInterfaceService.Btn_Cancella;
      b.event = (x: T) => this.elimina(kind, x);
      return b;
    };
    this.actAssegnazione = [{ button: cancella('assegnazione') }, { button: modifica('assegnazione') }];
    this.actScadenza = [{ button: cancella('scadenza') }, { button: modifica('scadenza') }];
    this.actCosto = [{ button: cancella('costo') }, { button: modifica('costo') }];
    const bNotifica = userInterfaceService.Btn_Invia;
    bNotifica.event = (x: F.AstMultaModel) => this.notificaMulta(x);
    this.actMulta = [{ button: cancella('multa') }, { button: bNotifica, visible: x => x.stato === F.AstStatoMulta.DaNotificare && !!x.idDip_Anagrafica }, { button: modifica('multa') }];
  }

  get Title(): string { return this.transloco.translate(this.id ? 'ast.pages.vehicleEdit.title' : 'ast.pages.vehicleEdit.titleNew'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      targa: [null, [Validators.required, Validators.maxLength(15)]],
      marca: [null, [Validators.maxLength(50)]],
      modello: [null, [Validators.maxLength(50)]],
      alimentazione: [0],
      dataImmatricolazione: [null],
      possesso: [0],
      fornitore: [null, [Validators.maxLength(100)]],
      numeroContratto: [null, [Validators.maxLength(50)]],
      scadenzaContratto: [null],
      kmInclusi: [null],
      canoneMensile: [null],
      idAz_Sedi: [null],
      uso: [0],
      costoKmAci: [0, [Validators.min(0)]],
      emissioniCO2: [null],
      kmAttuali: [null],
      stato: [0],
      note: [null, [Validators.maxLength(2000)]],
    });
  }

  override ionViewWillEnter() {
    if (!this.id) this.id = history.state?.id ?? 0;
    super.ionViewWillEnter();
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  LoadData = (): Observable<F.AstVeicoloModel> => {
    const request = new GenericRequest<F.AstIdInModel>(F.AstIdInModel);
    request.data.id = this.id;
    return this.service.VeicoloGet(request).pipe(map(res => {
      this.dati = res.success ? res.data : null;
      this.aggiornaFab();
      const v = this.dati?.veicolo ?? new F.AstVeicoloModel();
      return { ...v, dataImmatricolazione: astPerInputData(v.dataImmatricolazione), scadenzaContratto: astPerInputData(v.scadenzaContratto) };
    }), catchError(() => of(new F.AstVeicoloModel())));
  };

  SaveData = (editModel: F.AstVeicoloModel): Observable<boolean> => {
    const request = new GenericRequest<F.AstVeicoloPutInModel>(F.AstVeicoloPutInModel);
    request.data.veicolo = {
      ...editModel, id: this.id,
      dataImmatricolazione: editModel.dataImmatricolazione || null, scadenzaContratto: editModel.scadenzaContratto || null,
      kmInclusi: astNumero(editModel.kmInclusi), canoneMensile: astNumero(editModel.canoneMensile), costoKmAci: astNumero(editModel.costoKmAci) ?? 0,
      emissioniCO2: astNumero(editModel.emissioniCO2), kmAttuali: astNumero(editModel.kmAttuali),
    };
    return this.service.VeicoloPut(request).pipe(map(res => {
      if (res.success && res.data && !this.id) {
        // veicolo nuovo: si resta sulla pagina per aggiungere assegnazioni, scadenze, costi e multe
        this.id = res.data.veicolo.id;
        this.nuovoSalvato = true;
      }
      return res.success;
    }), catchError(() => of(false)));
  };

  private nuovoSalvato = false;

  override AfterSave(saved: boolean): void {
    if (!saved) return;
    if (this.nuovoSalvato) {
      this.nuovoSalvato = false;
      super.ionViewWillEnter();
      return;
    }
    this.navCtrl.back();
  }

  private aggiornaFab() {
    this.fabMenuService.fabMenuItem = this.id ? [
      new FabMenuItem(this.transloco.translate('ast.pages.vehicleEdit.add'), 'add-circle-outline', () => this.menuAggiungi()),
    ] : [];
  }

  private async menuAggiungi() {
    const t = (k: string) => this.transloco.translate(k);
    const pool = this.dati?.veicolo.uso === F.AstUsoVeicolo.Pool;
    const sheet = await this.actionSheetCtrl.create({
      header: t('ast.pages.vehicleEdit.add'),
      buttons: [
        ...(pool ? [] : [{ text: t('ast.pages.vehicleEdit.addAssegnazione'), icon: 'person-add-outline', handler: () => { this.apri('assegnazione', null); } }]),
        { text: t('ast.pages.vehicleEdit.addScadenza'), icon: 'calendar-outline', handler: () => { this.apri('scadenza', null); } },
        { text: t('ast.pages.vehicleEdit.addCosto'), icon: 'cash-outline', handler: () => { this.apri('costo', null); } },
        { text: t('ast.pages.vehicleEdit.addMulta'), icon: 'alert-circle-outline', handler: () => { this.apri('multa', null); } },
        { text: t('common.buttons.cancel'), role: 'cancel' },
      ]
    });
    await sheet.present();
  }

  apri(kind: AstVehicleItemKind, item: any) {
    if (!this.dati) return;
    this.sezione = kind;
    const v = this.dati.veicolo;
    this.navCtrl.navigateForward('/astvehicleitemedit', {
      state: {
        kind, item: item ? { ...item } : null,
        veicolo: { id: v.id, targa: v.targa, emissioniCO2: v.emissioniCO2 },
        dipendenti: this.dati.dipendente,
      }
    });
  }

  private async elimina(kind: AstVehicleItemKind, x: { id: number }) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('ast.pages.vehicleEdit.confirmDelete')))
      return;
    const request = new GenericRequest<F.AstIdInModel>(F.AstIdInModel);
    request.data.id = x.id;
    const call = kind === 'assegnazione' ? this.service.AssegnazioneDelete(request)
               : kind === 'scadenza' ? this.service.VeicoloScadenzaDelete(request)
               : kind === 'costo' ? this.service.VeicoloCostoDelete(request)
               : this.service.MultaDelete(request);
    call.subscribe(res => { if (res.success) this.ricarica(); });
  }

  private async notificaMulta(x: F.AstMultaModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('ast.pages.vehicleEdit.confirmNotify', { nome: x.conducente ?? '' })))
      return;
    const request = new GenericRequest<F.AstIdInModel>(F.AstIdInModel);
    request.data.id = x.id;
    this.service.MultaNotifica(request).subscribe(res => { if (res.success) this.ricarica(); });
  }

  // ricarica le liste senza toccare il form (eventuali modifiche non salvate restano)
  private ricarica() {
    const request = new GenericRequest<F.AstIdInModel>(F.AstIdInModel);
    request.data.id = this.id;
    this.service.VeicoloGet(request).subscribe(res => { if (res.success) this.dati = res.data; });
  }
}

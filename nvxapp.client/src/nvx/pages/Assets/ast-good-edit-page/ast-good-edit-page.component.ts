import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../ClientServer-Service/Assets/ast.service';
import * as F from '../../../ClientServer-Service/Assets/Models/ast-flotta-model';
import * as B from '../../../ClientServer-Service/Assets/Models/ast-beni-model';
import { AstFormatService, astNumero, astPerInputData } from '../../../Utility/Assets/ast-format.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-ast-good-edit-page',
  templateUrl: './ast-good-edit-page.component.html',
  styleUrls: ['./ast-good-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Bene (amministratori, AST-BENI.1-2): dati del bene e storico delle consegne con lo stato dei verbali (prese visione).
 Con il bene disponibile il + del fab registra la consegna a un dipendente; dalla riga aperta si registra la restituzione.
*/
export class AstGoodEditPageComponent extends BasePageConfirmCancelComponent<B.AstBeneModel> {

  public id = 0;
  public dati: B.AstBeneGetOutModel | null = null;
  public readonly stati = [0, 2, 3];
  public readonly colAssegnazione: NvxListColumn<B.AstBeneAssegnazioneModel>[];
  public readonly actAssegnazione: NvxListAction<B.AstBeneAssegnazioneModel>[];
  public readonly chiusa = (x: B.AstBeneAssegnazioneModel) => !!x.dataRestituzione;
  private nuovoSalvato = false;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              public fabMenuService: FabMenuService,
              private collectionDialogService: CollectionDialogService,
              private service: AstService,
              private fmt: AstFormatService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    const t = (k: string) => this.transloco.translate(k);
    this.colAssegnazione = [
      { key: 'dipendente', title: t('ast.fields.dipendente'), mobile: 'title' },
      { key: 'dataConsegna', title: t('ast.fields.dataConsegna'), value: x => this.fmt.data(x.dataConsegna), sortValue: x => x.dataConsegna },
      { key: 'verbaleConsegna', title: t('ast.fields.verbaleConsegna'), value: x => this.verbale(x.verbaleConsegna), mobile: 'note' },
      { key: 'dataRestituzione', title: t('ast.fields.dataRestituzione'), value: x => this.fmt.data(x.dataRestituzione), sortValue: x => x.dataRestituzione ?? '9999' },
      { key: 'verbaleRestituzione', title: t('ast.fields.verbaleRestituzione'), value: x => x.dataRestituzione ? this.verbale(x.verbaleRestituzione) : '', mobile: 'hidden' },
    ];
    const bRestituzione = userInterfaceService.Btn_Completa;
    bRestituzione.text = t('ast.pages.goodEdit.return');
    bRestituzione.event = (x: B.AstBeneAssegnazioneModel) => this.navCtrl.navigateForward('/astgooditemedit', {
      state: { kind: 'restituzione', item: { ...x }, bene: this.dati?.bene } });
    const bDel = userInterfaceService.Btn_Cancella;
    bDel.event = (x: B.AstBeneAssegnazioneModel) => this.eliminaAssegnazione(x);
    this.actAssegnazione = [{ button: bDel }, { button: bRestituzione, visible: x => !x.dataRestituzione }];
  }

  get Title(): string { return this.transloco.translate(this.id ? 'ast.pages.goodEdit.title' : 'ast.pages.goodEdit.titleNew'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      codiceInventario: [null, [Validators.required, Validators.maxLength(30)]],
      idAst_Categoria: [null, [Validators.required]],
      descrizione: [null, [Validators.required, Validators.maxLength(200)]],
      marcaModello: [null, [Validators.maxLength(100)]],
      numeroSerie: [null, [Validators.maxLength(100)]],
      dataAcquisto: [null],
      valore: [null],
      scadenzaGaranzia: [null],
      idAz_Sedi: [null],
      stato: [0],
      note: [null, [Validators.maxLength(2000)]],
    });
  }

  get assegnato(): boolean { return this.dati?.bene.stato === B.AstStatoBene.Assegnato; }

  override ionViewWillEnter() {
    if (!this.id) this.id = history.state?.id ?? 0;
    super.ionViewWillEnter();
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  LoadData = (): Observable<B.AstBeneModel> => {
    const request = new GenericRequest<F.AstIdInModel>(F.AstIdInModel);
    request.data.id = this.id;
    return this.service.BeneGet(request).pipe(map(res => {
      this.dati = res.success ? res.data : null;
      this.aggiornaFab();
      const b = this.dati?.bene ?? new B.AstBeneModel();
      if (!b.id && this.dati?.categoria.length) b.idAst_Categoria = this.dati.categoria.find(c => c.attivo)?.id ?? 0;
      return { ...b, idAst_Categoria: b.idAst_Categoria || null as any, dataAcquisto: astPerInputData(b.dataAcquisto), scadenzaGaranzia: astPerInputData(b.scadenzaGaranzia) };
    }), catchError(() => of(new B.AstBeneModel())));
  };

  SaveData = (m: B.AstBeneModel): Observable<boolean> => {
    const request = new GenericRequest<B.AstBenePutInModel>(B.AstBenePutInModel);
    request.data.bene = { ...m, id: this.id, dataAcquisto: m.dataAcquisto || null, scadenzaGaranzia: m.scadenzaGaranzia || null, valore: astNumero(m.valore) };
    return this.service.BenePut(request).pipe(map(res => {
      if (res.success && res.data && !this.id) {
        // bene nuovo: si resta sulla pagina per registrare la consegna
        this.id = res.data.bene.id;
        this.nuovoSalvato = true;
      }
      return res.success;
    }), catchError(() => of(false)));
  };

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
    const disponibile = !!this.id && this.dati?.bene.stato === B.AstStatoBene.Disponibile;
    this.fabMenuService.fabMenuItem = disponibile ? [
      new FabMenuItem(this.transloco.translate('ast.pages.goodEdit.deliver'), 'add-circle-outline', () =>
        this.navCtrl.navigateForward('/astgooditemedit', { state: { kind: 'consegna', item: null, bene: this.dati?.bene, dipendenti: this.dati?.dipendente } })),
    ] : [];
  }

  verbale(v: B.AstVerbaleModel): string {
    if (!v.richiesto) return this.transloco.translate('ast.pages.goodEdit.reportNone');
    return v.dataConferma ? this.transloco.translate('ast.pages.goodEdit.reportConfirmed', { data: this.fmt.data(v.dataConferma) })
                          : this.transloco.translate('ast.pages.goodEdit.reportPending');
  }

  private async eliminaAssegnazione(x: B.AstBeneAssegnazioneModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('ast.pages.goodEdit.confirmDeleteAssignment')))
      return;
    const request = new GenericRequest<F.AstIdInModel>(F.AstIdInModel);
    request.data.id = x.id;
    this.service.BeneAssegnazioneDelete(request).subscribe(res => { if (res.success) super.ionViewWillEnter(); });
  }
}

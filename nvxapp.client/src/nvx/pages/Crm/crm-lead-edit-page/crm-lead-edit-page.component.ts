import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ModalController, AlertController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, from, map, of, switchMap } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CrmService } from '../../../ClientServer-Service/Crm/crm.service';
import * as M from '../../../ClientServer-Service/Crm/Models/crm-model';
import { CrmSetupService, crmAdesso, crmData, crmDataOra, crmImporto, crmPerInputData, crmPerInputDataOra } from '../../../Utility/Crm/crm-setup.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { CrmLeadConvertDialogComponent } from '../../../shared/components/Crm/crm-lead-convert-dialog/crm-lead-convert-dialog.component';

@Component({
  selector: 'app-crm-lead-edit-page',
  templateUrl: './crm-lead-edit-page.component.html',
  styleUrls: ['./crm-lead-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Lead: dati, attivita' e note; "Converti" lo trasforma in cliente (+ contatto, + opportunita')
export class CrmLeadEditPageComponent extends BasePageConfirmCancelComponent<M.CrmLeadModel> {

  public sezione = 'dati';
  public scheda: M.CrmLeadGetOutModel | null = null;
  public readonly stati = [0, 1, 2, 4];
  private id = 0;
  public readonly colAttivita: NvxListColumn<M.CrmAttivitaListModel>[];
  public readonly azAttivita: NvxListAction<M.CrmAttivitaListModel>[];
  private btnConverti: ButtonItem;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: CrmService,
              public setupService: CrmSetupService,
              private modalCtrl: ModalController,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    const t = (k: string) => this.transloco.translate(k);
    this.colAttivita = [
      { key: 'inizio', title: t('crm.fields.quando'), mobile: 'note', value: x => crmDataOra(x.inizio), sortValue: x => x.inizio },
      { key: 'oggetto', title: t('crm.fields.oggetto'), mobile: 'title', value: x => x.tipo + ': ' + x.oggetto },
      { key: 'stato', title: t('crm.fields.stato'), mobile: 'subtitle', value: x => t('crm.enums.statoAttivita.' + x.stato) },
    ];
    const b = userInterfaceService.Btn_Apri; b.event = (x: M.CrmAttivitaListModel) => this.navCtrl.navigateForward('/crmactivityedit', { state: { id: x.id } });
    this.azAttivita = [{ button: b }];
    this.btnConverti = userInterfaceService.Btn_Esegui;
    this.btnConverti.text = t('crm.pages.leadEdit.convert');
    this.btnConverti.image = 'swap-horizontal-outline';
    this.btnConverti.event = () => this.converti();
  }

  get Title(): string { return this.transloco.translate('crm.pages.leadEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      cognome: [null, [Validators.maxLength(100)]], nome: [null, [Validators.maxLength(100)]], azienda: [null, [Validators.maxLength(100)]],
      ruolo: [null, [Validators.maxLength(100)]], email: [null, [Validators.email, Validators.maxLength(100)]], telefono: [null, [Validators.maxLength(30)]],
      idCrm_Par_Fonte: [null], stato: [0], idResponsabile: [null], note: [null, [Validators.maxLength(2000)]], consensoMarketing: [false],
    });
  }

  LoadData = (): Observable<M.CrmLeadModel | null> => {
    this.id = this.id || history.state?.id || 0;
    return from(this.setupService.load()).pipe(switchMap(setup => {
      if (!this.id) {
        this.scheda = null;
        this.aggiornaPulsanti(null);
        const nuovo = new M.CrmLeadModel();
        if (setup && !setup.permessi.powerAdmin && !setup.permessi.admin)
          nuovo.idResponsabile = setup.permessi.idUtente;
        return of(nuovo);
      }
      return this.carica();
    }));
  };

  private carica(): Observable<M.CrmLeadModel | null> {
    const request = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    request.data.id = this.id;
    return this.service.LeadGet(request).pipe(map(res => {
      this.scheda = res.success ? res.data : null;
      this.aggiornaPulsanti(this.scheda?.lead ?? null);
      return this.scheda?.lead ?? null;
    }), catchError(() => of(null)));
  }

  ricarica() { this.carica().subscribe(); }

  // "Converti" solo per un lead salvato e non ancora convertito
  private aggiornaPulsanti(lead: M.CrmLeadModel | null) {
    const base = this.buttonbar.filter(b => b !== this.btnConverti);
    this.buttonbar = lead && lead.id && lead.stato !== M.CrmStatoLead.Convertito ? [this.btnConverti, ...base] : base;
  }

  get convertito(): boolean { return this.scheda?.lead.stato === M.CrmStatoLead.Convertito; }

  SaveData = (editModel: M.CrmLeadModel): Observable<boolean> => {
    const request = new GenericRequest<M.CrmLeadPutInModel>(M.CrmLeadPutInModel);
    request.data.lead = { ...editModel, id: this.id };
    return this.service.LeadPut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };

  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }

  async converti() {
    const lead = this.scheda?.lead;
    if (!lead) return;
    const clienti = await new Promise<M.CrmClienteListModel[]>(resolve => {
      const r = new GenericRequest<M.CrmClienteListInModel>(M.CrmClienteListInModel);
      this.service.ClienteList(r).subscribe({ next: res => resolve(res.success ? res.data.clienti.filter(c => c.attivo) : []), error: () => resolve([]) });
    });
    const modal = await this.modalCtrl.create({
      component: CrmLeadConvertDialogComponent,
      componentProps: { idLead: lead.id, titolo: lead.azienda || [lead.cognome, lead.nome].filter(v => !!v).join(' '), clienti }
    });
    await modal.present();
    const { data, role } = await modal.onDidDismiss();
    if (role === 'confirm' && data)
      this.navCtrl.navigateForward('/crmcustomeredit', { state: { id: data.idAz_Cliente } });
  }

  nuovaAttivita() {
    this.navCtrl.navigateForward('/crmactivityedit', { state: { id: 0, preset: { idCrm_Lead: this.id } } });
  }
}

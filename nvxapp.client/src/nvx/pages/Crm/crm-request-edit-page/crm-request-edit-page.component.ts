import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, from, map, of, switchMap } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CrmService } from '../../../ClientServer-Service/Crm/crm.service';
import * as M from '../../../ClientServer-Service/Crm/Models/crm-model';
import { CrmSetupService, crmAdesso, crmDataOra, crmPerInputDataOra } from '../../../Utility/Crm/crm-setup.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-crm-request-edit-page',
  templateUrl: './crm-request-edit-page.component.html',
  styleUrls: ['./crm-request-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Richiesta di assistenza di un cliente (CRM-ASS): dati, stato, assegnatario (notificato dal server), tempi e soluzione;
 sezione Messaggi con le note interne e gli eventi registrati dal server (apertura, assegnazione, cambi di stato).
 Una richiesta nuova resta aperta dopo il salvataggio (per aggiungere le note); il commerciale si assegna solo a se'.
*/
export class CrmRequestEditPageComponent extends BasePageConfirmCancelComponent<M.CrmRichiestaModel> {

  private id = 0;
  private nuova = false;
  public sezione = 'dati';
  public scheda: M.CrmRichiestaGetOutModel | null = null;
  public clienti: M.CrmClienteListModel[] = [];
  public contatti: M.CrmContattoModel[] = [];
  public opportunitaCliente: M.CrmOpportunitaListModel[] = [];
  public nota = '';
  public readonly stati = [0, 1, 2, 3, 4, 5];
  public readonly priorita = [0, 1, 2, 3];
  public readonly canali = [0, 1, 2, 3, 4];
  public readonly dataOra = crmDataOra;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: CrmService,
              public setupService: CrmSetupService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService,
              private route: ActivatedRoute) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string {
    return this.transloco.translate('crm.pages.requestEdit.title') + (this.scheda ? ' ' + this.scheda.richiesta.numeroCompleto : '');
  }

  get EditForm(): FormGroup {
    const f = this.fb.group({
      idAz_Cliente: [null, [Validators.required]], idAz_ClienteContatto: [null], idCrm_Opportunita: [null],
      oggetto: [null, [Validators.required, Validators.maxLength(200)]], descrizione: [null, [Validators.maxLength(4000)]],
      canale: [0], priorita: [1], stato: [0], idAssegnatario: [null], dataApertura: [null, [Validators.required]],
      soluzione: [null, [Validators.maxLength(2000)]],
    });
    f.get('idAz_Cliente')!.valueChanges.subscribe(id => this.caricaCliente(id));
    return f;
  }

  LoadData = (): Observable<M.CrmRichiestaModel | null> => {
    // id dallo stato della navigazione o dall'indirizzo (link della notifica)
    this.id = this.id || history.state?.id || Number(this.route.snapshot.queryParamMap.get('id')) || 0;
    const preset = history.state?.preset ?? {};
    return from(this.setupService.load()).pipe(switchMap(() => {
      const r = new GenericRequest<M.CrmClienteListInModel>(M.CrmClienteListInModel);
      this.service.ClienteList(r).subscribe(res => this.clienti = res.success ? res.data.clienti : []);
      if (!this.id) {
        this.scheda = null;
        return of(Object.assign(new M.CrmRichiestaModel(), { dataApertura: crmAdesso() }, preset));
      }
      return this.carica();
    }));
  };

  private carica(): Observable<M.CrmRichiestaModel | null> {
    const request = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    request.data.id = this.id;
    return this.service.RichiestaGet(request).pipe(map(res => {
      this.scheda = res.success ? res.data : null;
      if (!this.scheda) return null;
      this.contatti = this.scheda.contatti;
      this.opportunitaCliente = this.scheda.opportunitaCliente;
      const r = this.scheda.richiesta;
      return { ...r, dataApertura: crmPerInputDataOra(r.dataApertura)! };
    }), catchError(() => of(null)));
  }

  get amministratore(): boolean { return !!(this.setupService.setup?.permessi.powerAdmin || this.setupService.setup?.permessi.admin); }

  // il commerciale sceglie solo se stesso (o nessuno) come assegnatario; resta visibile quello gia' assegnato
  get assegnatari(): M.CrmUtenteModel[] {
    const utenti = this.setupService.setup?.utenti ?? [];
    if (this.amministratore) return utenti;
    const me = this.setupService.setup?.permessi.idUtente;
    const attuale = this._editForm?.get('idAssegnatario')?.value;
    return utenti.filter(u => u.id === me || u.id === attuale);
  }

  get clientiScelta(): M.CrmClienteListModel[] {
    const scelto = this._editForm?.get('idAz_Cliente')?.value;
    return this.clienti.filter(c => (c.attivo && c.tipo !== 1) || c.id === scelto);
  }

  private caricaCliente(idCliente: number | null) {
    if (!idCliente) { this.contatti = []; this.opportunitaCliente = []; return; }
    const r = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    r.data.id = idCliente;
    this.service.ClienteGet(r).subscribe(res => {
      this.contatti = res.success ? res.data.contatti : [];
      this.opportunitaCliente = res.success ? res.data.opportunita : [];
    });
  }

  ore(v: number | null | undefined): string {
    return v != null ? this.transloco.translate('crm.pages.requestEdit.hours', { ore: v }) : '-';
  }

  SaveData = (editModel: M.CrmRichiestaModel): Observable<boolean> => {
    const request = new GenericRequest<M.CrmRichiestaPutInModel>(M.CrmRichiestaPutInModel);
    request.data.richiesta = { ...editModel, id: this.id, idAssegnatario: editModel.idAssegnatario || null };
    return this.service.RichiestaPut(request).pipe(map(res => {
      if (res.success) {
        this.nuova = !this.id;
        this.id = res.data.richiesta.id;
      }
      return res.success;
    }), catchError(() => of(false)));
  };

  // una richiesta nuova resta aperta (note interne), una esistente torna all'elenco
  override AfterSave(saved: boolean): void {
    if (!saved)
      return;
    if (this.nuova)
      this.ionViewWillEnter();
    else
      this.navCtrl.back();
  }

  aggiungiNota() {
    if (!this.nota.trim() || !this.id)
      return;
    const request = new GenericRequest<M.CrmRichiestaMessaggioPutInModel>(M.CrmRichiestaMessaggioPutInModel);
    request.data = { idCrm_Richiesta: this.id, testo: this.nota.trim() };
    this.service.RichiestaMessaggioPut(request).subscribe(res => {
      if (res.success && this.scheda) {
        this.scheda.messaggi = res.data.messaggi;
        this.nota = '';
      }
    });
  }

  async elimina() {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('crm.pages.requestEdit.confirmDelete')))
      return;
    const r = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    r.data.id = this.id;
    this.service.RichiestaDelete(r).subscribe(res => { if (res.success) this.navCtrl.back(); });
  }
}

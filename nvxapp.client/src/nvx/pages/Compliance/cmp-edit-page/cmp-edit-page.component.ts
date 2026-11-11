import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CmpService } from '../../../ClientServer-Service/Compliance/cmp.service';
import * as M from '../../../ClientServer-Service/Compliance/Models/cmp-model';
import { CMP_SEZIONI, CmpCampo, CmpSezione } from '../../../Utility/Compliance/cmp-sezioni';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

/*
 Modifica di un elemento di Compliance (ANALISI_COMPLIANCE_APPLICATIVO.md): la sezione (stato di navigazione "sezione") dice quali
 campi mostrare (cmp-sezioni.ts). Stato: { sezione, item, idCompany } dalla pagina elenco (idCompany solo lato studio).
 Le tabelle di scelta (dipendenti, mansioni, DPI, sedi) arrivano da Cmp_Lookup; i dati sanitari delle visite si vedono e si
 scrivono solo se l'utente e' abilitato (il server li ignora negli altri casi). Cancella per gli elementi gia' salvati.
*/
@Component({
  selector: 'app-cmp-edit-page',
  templateUrl: './cmp-edit-page.component.html',
  styleUrls: ['./cmp-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CmpEditPageComponent extends BasePageConfirmCancelComponent<any> {

  public sezione: CmpSezione = CMP_SEZIONI['formazione'];
  public campi: CmpCampo[] = [];
  public lookup: M.CmpLookupOutModel | null = null;
  private idCompany: number | null = null;
  private readonly btnDelete: ButtonItem;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: CmpService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = () => this.cancella();
  }

  get Title(): string {
    return this.transloco.translate('cmp.sections.' + this.sezione.key) + (this._editModel?.id ? '' : ' - ' + this.transloco.translate('cmp.pages.edit.new'));
  }

  // il form si costruisce dai campi della sezione (letta dallo stato di navigazione prima di LoadData)
  get EditForm(): FormGroup {
    const state = history.state ?? {};
    this.sezione = CMP_SEZIONI[state.sezione] ?? CMP_SEZIONI['formazione'];
    this.idCompany = state.idCompany ?? null;
    this.campi = this.sezione.campi;
    const controlli: Record<string, any> = {};
    for (const c of this.campi) {
      const v: ValidatorFn[] = [];
      if (c.required && c.tipo !== 'toggle') v.push(Validators.required);
      if (c.max && (c.tipo === 'text' || c.tipo === 'textarea')) v.push(Validators.maxLength(c.max));
      if (c.min !== undefined && c.tipo === 'number') v.push(Validators.min(c.min));
      controlli[c.key] = [c.tipo === 'toggle' ? false : null, v];
    }
    return this.fb.group(controlli);
  }

  LoadData = (): Observable<any> => {
    const state = history.state ?? {};
    const item = { ...(state.item ?? this.sezione.nuovo?.() ?? { id: 0 }) };
    // nei campi data solo il giorno (yyyy-MM-dd), nei campi data e ora fino ai minuti
    for (const c of this.campi) {
      if (c.tipo === 'date' && item[c.key]) item[c.key] = String(item[c.key]).substring(0, 10);
      if (c.tipo === 'datetime' && item[c.key]) item[c.key] = String(item[c.key]).substring(0, 16);
    }
    this.buttonbar = item.id ? [this.btnConfirm, this.btnDelete, this.btnCancel] : [this.btnConfirm, this.btnCancel];

    if (!this.campi.some(c => c.lookup) && this.sezione.key !== 'visite')
      return of(item);
    const request = new GenericRequest<M.CmpListInModel>(M.CmpListInModel);
    request.data.idCompany = this.idCompany;
    return this.service.Lookup(request).pipe(
      map(res => {
        this.lookup = res.success && res.data ? res.data : null;
        // dati sanitari: solo per gli abilitati
        this.campi = this.sezione.campi.filter(c => !c.sanitario || !!this.lookup?.permessi.sanitario);
        return item;
      }),
      catchError(() => of(item)));
  };

  /** voci di una select da tabella: le non attive solo se gia' scelte */
  voci(c: CmpCampo): M.CmpVoceModel[] {
    const elenco: M.CmpVoceModel[] = (this.lookup as any)?.[c.lookup!] ?? [];
    const scelto = this._editForm?.get(c.key)?.value;
    return elenco.filter(v => v.attivo || v.id === scelto);
  }

  SaveData = (editModel: any): Observable<boolean> => {
    const item: any = { ...editModel };
    for (const c of this.sezione.campi) {
      const v = item[c.key];
      if (c.tipo === 'number')
        item[c.key] = v === null || v === undefined || v === '' ? null : Number(v);
      else if (c.tipo === 'date' || c.tipo === 'datetime')
        item[c.key] = v ? (c.tipo === 'datetime' && String(v).length === 16 ? v + ':00' : v) : null;
      else if (c.tipo === 'select' && c.lookup && (v === '' || v === undefined))
        item[c.key] = null;
      else if ((c.tipo === 'text' || c.tipo === 'textarea') && typeof v === 'string' && v.trim() === '')
        item[c.key] = null;
    }
    const request = new GenericRequest<M.CmpPutInModel>(M.CmpPutInModel);
    request.data.item = item;
    request.data.idCompany = this.idCompany;
    return this.service.Put(this.sezione.api, this.sezione.server, request).pipe(map(res => res.success), catchError(() => of(false)));
  };

  // in caso di errore si resta sulla pagina
  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }

  private async cancella() {
    if (!this._editModel?.id || !await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('cmp.pages.list.confirmDelete')))
      return;
    const request = new GenericRequest<M.CmpDeleteInModel>(M.CmpDeleteInModel);
    request.data.id = this._editModel.id;
    request.data.idCompany = this.idCompany;
    this.service.Delete(this.sezione.api, this.sezione.server, request).subscribe(res => { if (res.success) this.navCtrl.back(); });
  }
}

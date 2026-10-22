import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { NavController, ToastController } from '@ionic/angular';
import { Observable, from, map, of, switchMap } from 'rxjs';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { CompanyMoveService } from '../../../ClientServer-Service/Infrastructure/CompanyMove/company-move.service';
import { CompanyMoveCompanyModel, CompanyMoveDealerModel, CompanyMoveFinancialAdvisorModel, CompanyMovePutInModel, CompanyMoveTreeInModel } from '../../../ClientServer-Service/Infrastructure/CompanyMove/Models/company-move-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { TranslocoService } from '@jsverse/transloco';


/*
 Pagina "Sposta azienda" (SuperUser): cambia lo studio (FinancialAdvisor) a cui appartiene un'azienda.
 Riquadro "Azienda": centro -> studio -> azienda per trovarla; scegliendo un livello si filtrano quelli sotto,
 scegliendo direttamente un figlio si impostano i padri. Riquadro "Destinazione": centro -> studio.
 La gerarchia completa arriva con una sola chiamata; i filtri sono fatti qui.
 Campi del form: origineIdDealer / origineIdFinancialAdvisor / idCompany (azienda da spostare),
 destinazioneIdDealer / idFinancialAdvisor (dove spostarla); idCompany e idFinancialAdvisor sono quelli inviati.
 Dopo lo spostamento si resta sulla pagina (con l'azienda gia' nella nuova posizione) per poterne spostare altre.
*/
@Component({
  selector: 'app-company-move-page',
  templateUrl: './company-move-page.component.html',
  styleUrls: ['./company-move-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompanyMovePageComponent extends BasePageConfirmCancelComponent<CompanyMovePutInModel> {

  public dealers: CompanyMoveDealerModel[] = [];
  public financialAdvisors: CompanyMoveFinancialAdvisorModel[] = [];
  public companies: CompanyMoveCompanyModel[] = [];
  public caricamento = false;

  // elenchi filtrati per i combo (ricalcolati solo quando cambia una scelta)
  public origineStudi: CompanyMoveFinancialAdvisorModel[] = [];
  public origineAziende: CompanyMoveCompanyModel[] = [];
  public destinazioneStudi: CompanyMoveFinancialAdvisorModel[] = [];

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private toastCtrl: ToastController,
              private companyMoveService: CompanyMoveService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {

    super(navCtrl, userInterfaceService, fb);
    this.btnConfirm.disabled = true;
  }

  get Title(): string { return this.transloco.translate('pages.companyMove.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      origineIdDealer: [null],
      origineIdFinancialAdvisor: [null],
      idCompany: [null, [Validators.required]],
      destinazioneIdDealer: [null],
      idFinancialAdvisor: [null, [Validators.required]],
    }, { validators: this.destinazioneDiversa });
  }

  /** la destinazione non puo' essere lo studio in cui l'azienda e' gia' */
  private destinazioneDiversa = (group: AbstractControl): ValidationErrors | null => {
    const company = this.azienda(group.get('idCompany')?.value);
    return company && company.idFinancialAdvisor === group.get('idFinancialAdvisor')?.value ? { stessoStudio: true } : null;
  };

  LoadData = (): Observable<CompanyMovePutInModel> => {
    this.caricamento = true;
    return this.companyMoveService.CompanyMoveTree(new GenericRequest<CompanyMoveTreeInModel>(CompanyMoveTreeInModel)).pipe(map(res => {
      if (res.success && res.data) {
        this.dealers = res.data.dealers;
        this.financialAdvisors = res.data.financialAdvisors;
        this.companies = res.data.companies;
      }
      this.caricamento = false;
      this.aggiorna();
      // rientrando restano le scelte gia' fatte
      return Object.assign(new CompanyMovePutInModel(), this._editForm.value);
    }));
  };

  // con un errore il messaggio del server compare da solo (http-background-error-dialog) e le scelte restano
  SaveData = (editModel: CompanyMovePutInModel): Observable<boolean> => {
    const company = this.aziendaScelta;
    if (!company || editModel.idFinancialAdvisor == null || this.stessoStudio)
      return of(false);

    const conferma = this.collectionDialogService.ConfirmCancelDialog(
      this.transloco.translate('pages.companyMove.confirmMove', {
        company: company.descrizione,
        from: this.percorso(company.idFinancialAdvisor),
        to: this.percorso(editModel.idFinancialAdvisor)
      }));

    return from(conferma).pipe(switchMap(ok => {
      if (!ok)
        return of(false);

      const request = new GenericRequest<CompanyMovePutInModel>(CompanyMovePutInModel);
      request.data.idCompany = company.idCompany;
      request.data.idFinancialAdvisor = editModel.idFinancialAdvisor;
      return this.companyMoveService.CompanyMovePut(request).pipe(map(res => {
        if (!res.success || !res.data?.company)
          return false;
        company.idFinancialAdvisor = res.data.company.idFinancialAdvisor;
        return true;
      }));
    }));
  };

  // spostata: si resta sulla pagina per spostarne altre, con l'azienda gia' nella nuova posizione
  override AfterSave(saved: boolean): void {
    const company = this.aziendaScelta;
    if (!saved || !company)
      return;
    this.onOrigineAzienda(company.idCompany);
    this._editForm.patchValue({ destinazioneIdDealer: null, idFinancialAdvisor: null });
    this.aggiorna();
    this.toastCtrl.create({ message: this.transloco.translate('pages.companyMove.moved', { company: company.descrizione }), duration: 3000, position: 'bottom' })
      .then(t => t.present());
  }

  private v(campo: string): number | null {
    return this._editForm.get(campo)?.value ?? null;
  }

  // ---------- azienda da spostare ----------

  public onOrigineDealer(id: number | null): void {
    const valori: any = { origineIdDealer: id };
    if (this.studio(this.v('origineIdFinancialAdvisor'))?.idDealer !== id && id != null)
      valori.origineIdFinancialAdvisor = null;
    if (id != null && this.dealerDiAzienda(this.v('idCompany')) !== id)
      valori.idCompany = null;
    this._editForm.patchValue(valori);
    this.aggiorna();
  }

  public onOrigineStudio(id: number | null): void {
    const valori: any = { origineIdFinancialAdvisor: id };
    if (id != null) {
      valori.origineIdDealer = this.studio(id)?.idDealer ?? this.v('origineIdDealer');
      if (this.azienda(this.v('idCompany'))?.idFinancialAdvisor !== id)
        valori.idCompany = null;
    }
    this._editForm.patchValue(valori);
    this.aggiorna();
  }

  public onOrigineAzienda(id: number | null): void {
    const valori: any = { idCompany: id };
    const company = this.azienda(id);
    if (company) {
      // la posizione attuale dell'azienda
      valori.origineIdFinancialAdvisor = company.idFinancialAdvisor;
      valori.origineIdDealer = this.studio(company.idFinancialAdvisor)?.idDealer ?? null;
    }
    this._editForm.patchValue(valori);
    this.aggiorna();
  }

  // ---------- destinazione ----------

  public onDestinazioneDealer(id: number | null): void {
    const valori: any = { destinazioneIdDealer: id };
    if (id != null && this.studio(this.v('idFinancialAdvisor'))?.idDealer !== id)
      valori.idFinancialAdvisor = null;
    this._editForm.patchValue(valori);
    this.aggiorna();
  }

  public onDestinazioneStudio(id: number | null): void {
    const valori: any = { idFinancialAdvisor: id };
    if (id != null)
      valori.destinazioneIdDealer = this.studio(id)?.idDealer ?? this.v('destinazioneIdDealer');
    this._editForm.patchValue(valori);
    this.aggiorna();
  }

  // ---------- testi e controlli ----------

  public get aziendaScelta(): CompanyMoveCompanyModel | undefined {
    return this.azienda(this.v('idCompany'));
  }

  public get idDestinazione(): number | null {
    return this.v('idFinancialAdvisor');
  }

  /** la destinazione e' lo studio in cui l'azienda e' gia' */
  public get stessoStudio(): boolean {
    return this._editForm.hasError('stessoStudio');
  }

  public percorso(idFinancialAdvisor: number | null | undefined): string {
    const fa = this.studio(idFinancialAdvisor);
    if (!fa) return '';
    const dealer = this.dealers.find(d => d.idDealer === fa.idDealer);
    return `${dealer?.descrizione ?? '?'} › ${fa.descrizione}`;
  }

  private aggiorna(): void {
    const origineIdDealer = this.v('origineIdDealer');
    const origineIdFinancialAdvisor = this.v('origineIdFinancialAdvisor');
    const destinazioneIdDealer = this.v('destinazioneIdDealer');
    this.origineStudi = this.financialAdvisors.filter(f => origineIdDealer == null || f.idDealer === origineIdDealer);
    this.origineAziende = this.companies.filter(c =>
      origineIdFinancialAdvisor != null ? c.idFinancialAdvisor === origineIdFinancialAdvisor
        : origineIdDealer != null ? this.studio(c.idFinancialAdvisor)?.idDealer === origineIdDealer
          : true);
    this.destinazioneStudi = this.financialAdvisors.filter(f => destinazioneIdDealer == null || f.idDealer === destinazioneIdDealer);
    // la posizione dell'azienda puo' essere cambiata: si ricontrolla la destinazione
    this._editForm.updateValueAndValidity();
  }

  private studio(id: number | null | undefined): CompanyMoveFinancialAdvisorModel | undefined {
    return id == null ? undefined : this.financialAdvisors.find(f => f.idFinancialAdvisor === id);
  }

  private azienda(id: number | null | undefined): CompanyMoveCompanyModel | undefined {
    return id == null ? undefined : this.companies.find(c => c.idCompany === id);
  }

  private dealerDiAzienda(idCompany: number | null): number | undefined {
    const company = this.azienda(idCompany);
    return company ? this.studio(company.idFinancialAdvisor)?.idDealer : undefined;
  }
}

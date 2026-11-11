import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { LicenseService } from '../../../ClientServer-Service/Infrastructure/License/license.service';
import {
  AMBITI_LICENZA, AmbitoLicenza, LicenseCatalogGetInModel, LicenseCatalogModel, LicenseCatalogPutInModel, LicenseDealerModel, LicensePlanModel,
  TipoSoggettoLicenza
} from '../../../ClientServer-Service/Infrastructure/License/Models/license-model';
import { LivelloLicenza } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

// listino con i tre piani in campi separati (il form non gestisce l'array dei piani)
export class LicenseCatalogEditModel extends LicenseCatalogModel {
  public free: LicensePlanModel = new LicensePlanModel();
  public standard: LicensePlanModel = new LicensePlanModel();
  public pro: LicensePlanModel = new LicensePlanModel();
}

/*
 Listino delle licenze (ANALISI_INFRASTRUCTURE_LICENZE.md §3.1, §5, §5.1): tipo di soggetto, funzione, descrizione e i tre piani
 Free (prezzo 0), Standard e Pro con prezzi mensile/annuale e utenti massimi (vuoto = illimitati).
 Il SuperUser sceglie il proprietario di un nuovo listino (globale o di un centro); il centro crea solo i propri.
 Listino non modificabile dall'utente (es. globale visto da un centro): sola lettura.
*/
@Component({
  selector: 'app-license-catalog-edit-page',
  templateUrl: './license-catalog-edit-page.component.html',
  styleUrls: ['./license-catalog-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class LicenseCatalogEditPageComponent extends BasePageConfirmCancelComponent<LicenseCatalogEditModel> {

  public readonly ambiti = AMBITI_LICENZA;
  public readonly tipiSoggetto = [TipoSoggettoLicenza.Company, TipoSoggettoLicenza.FinancialAdvisor];
  public readonly livelli: { key: 'free' | 'standard' | 'pro', livello: LivelloLicenza }[] = [
    { key: 'free', livello: LivelloLicenza.Free },
    { key: 'standard', livello: LivelloLicenza.Standard },
    { key: 'pro', livello: LivelloLicenza.Pro },
  ];
  public dealer: LicenseDealerModel[] = [];
  public isSuperUser = false;
  public isNew = true;
  public canEdit = true;
  public abbonamenti = 0;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private licenseService: LicenseService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('pages.licenseCatalogEdit.title'); }

  private planGroup(free: boolean): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(20)]],
      descrizione: [null, [Validators.maxLength(100)]],
      prezzoMensile: [{ value: 0, disabled: free }, [Validators.required, Validators.min(0)]],
      prezzoAnnuale: [{ value: 0, disabled: free }, [Validators.required, Validators.min(0)]],
      // limiti, 0 o vuoto = illimitati: utenti (Presenze), clienti (CRM), spazio in MB (Documentale)
      maxUtenti: [null, [Validators.min(0)]],
      maxClienti: [null, [Validators.min(0)]],
      maxSpazioMB: [null, [Validators.min(0)]],
      attivo: [{ value: true, disabled: free }],
    });
  }

  /*
   Campi che impediscono il salvataggio (il pulsante Conferma resta grigio finche' il form non e' valido):
   elencati sopra i pulsanti, cosi' si capisce sempre cosa correggere.
  */
  get campiNonValidi(): string[] {
    if (!this._editForm || this._editForm.valid || this._editForm.disabled)
      return [];
    const t = (k: string) => this.transloco.translate(k);
    const out: string[] = [];
    if (this._editForm.get('descrizione')?.invalid)
      out.push(t('pages.licenseCatalogEdit.description'));
    for (const l of this.livelli) {
      const livello = t('pages.license.level' + l.livello);
      const campi: [string, string][] = [['codice', 'pages.licenseCatalogEdit.planCode'], ['descrizione', 'pages.licenseCatalogEdit.planDescription'],
                                         ['prezzoMensile', 'pages.licenseCatalogEdit.monthlyPrice'], ['prezzoAnnuale', 'pages.licenseCatalogEdit.yearlyPrice'],
                                         ['maxUtenti', 'pages.licenseCatalogEdit.maxUsers'], ['maxClienti', 'pages.licenseCatalogEdit.maxClients'],
                                         ['maxSpazioMB', 'pages.licenseCatalogEdit.maxStorage']];
      for (const [nome, chiave] of campi)
        if (this._editForm.get(l.key + '.' + nome)?.invalid)
          out.push(`${t(chiave)} (${livello})`);
    }
    return out;
  }

  get EditForm(): FormGroup {
    return this.fb.group({
      idDealer: [null],
      tipoSoggetto: [TipoSoggettoLicenza.Company, [Validators.required]],
      ambito: [AMBITI_LICENZA[0], [Validators.required]],
      descrizione: [null, [Validators.required, Validators.maxLength(100)]],
      disponibileSuCompany: [false],
      attivo: [true],
      free: this.planGroup(true),
      standard: this.planGroup(false),
      pro: this.planGroup(false),
    });
  }

  // limiti mostrati per la funzione del listino: Presenze utenti, CRM clienti, Documentale spazio, Cumulativo tutti (§5.1)
  usaLimite(funzione: AmbitoLicenza): boolean {
    const ambito = Number(this._editForm?.get('ambito')?.value);
    return ambito === funzione || ambito === AmbitoLicenza.Cumulativo;
  }
  public readonly Ambito = AmbitoLicenza;

  get isFinancialAdvisor(): boolean {
    return Number(this._editForm?.get('tipoSoggetto')?.value) === TipoSoggettoLicenza.FinancialAdvisor;
  }

  LoadData = (): Observable<LicenseCatalogEditModel | null> => {
    const request = new GenericRequest<LicenseCatalogGetInModel>(LicenseCatalogGetInModel);
    request.data.id = history.state?.id ?? 0;
    return this.licenseService.LicenseCatalogGet(request).pipe(
      map(res => {
        if (!res.success || !res.data)
          return null;
        this.isSuperUser = res.data.isSuperUser;
        this.dealer = res.data.dealer ?? [];
        const listino = res.data.listino;
        this.isNew = !listino.id;
        this.canEdit = listino.canEdit;
        this.abbonamenti = listino.abbonamenti;

        const model = Object.assign(new LicenseCatalogEditModel(), listino);
        for (const l of this.livelli)
          model[l.key] = { ...(listino.piano.find(p => p.livello === l.livello) ?? Object.assign(new LicensePlanModel(), { livello: l.livello })) };

        // proprietario: si sceglie solo creando (SuperUser); sola lettura se non modificabile
        setTimeout(() => {
          if (!this.isNew || !this.isSuperUser)
            this._editForm.get('idDealer')?.disable();
          if (!this.canEdit)
            this._editForm.disable();
        });
        return model;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );
  };

  SaveData = (editModel: LicenseCatalogEditModel): Observable<boolean> => {
    const numero = (v: any) => (v === null || v === undefined || v === '') ? null : Number(v);
    const piano = this.livelli.map(l => {
      const p = editModel[l.key];
      const free = l.livello === LivelloLicenza.Free;
      return Object.assign(new LicensePlanModel(), {
        id: p.id ?? 0,
        livello: l.livello,
        codice: (p.codice ?? '').trim(),
        descrizione: p.descrizione ? p.descrizione.trim() : null,
        ordine: l.livello * 10,
        prezzoMensile: free ? 0 : (numero(p.prezzoMensile) ?? 0),
        prezzoAnnuale: free ? 0 : (numero(p.prezzoAnnuale) ?? 0),
        // 0 o vuoto = illimitati
        maxUtenti: (numero(p.maxUtenti) ?? 0) > 0 ? numero(p.maxUtenti) : null,
        maxClienti: (numero(p.maxClienti) ?? 0) > 0 ? numero(p.maxClienti) : null,
        maxSpazioMB: (numero(p.maxSpazioMB) ?? 0) > 0 ? numero(p.maxSpazioMB) : null,
        attivo: free ? true : !!p.attivo,
      });
    });

    const request = new GenericRequest<LicenseCatalogPutInModel>(LicenseCatalogPutInModel);
    request.data.listino = Object.assign(new LicenseCatalogModel(), {
      id: editModel.id ?? 0,
      idDealer: this.isNew && this.isSuperUser ? numero(this._editForm.get('idDealer')?.value) : editModel.idDealer,
      tipoSoggetto: Number(editModel.tipoSoggetto),
      ambito: Number(editModel.ambito),
      descrizione: (editModel.descrizione ?? '').trim(),
      valuta: 'EUR',
      disponibileSuCompany: Number(editModel.tipoSoggetto) === TipoSoggettoLicenza.FinancialAdvisor && !!editModel.disponibileSuCompany,
      attivo: !!editModel.attivo,
      piano
    });
    return this.licenseService.LicenseCatalogPut(request).pipe(
      map(res => res.success),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  // in caso di errore (es. listino gia' esistente) si resta sulla pagina
  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }
}

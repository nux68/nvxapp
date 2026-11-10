import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { LicenseService } from '../../../ClientServer-Service/Infrastructure/License/license.service';
import {
  AmbitoLicenza, LicenseOfferModel, LicensePlanModel, LicenseSubscribeInModel, LicenseSubscriptionModel, MetodoPagamento, PeriodicitaLicenza
} from '../../../ClientServer-Service/Infrastructure/License/Models/license-model';

/*
 Sottoscrizione (o cambio piano) di una licenza (ANALISI_LICENZE.md §6, §7): piano, periodicita', metodo di pagamento
 e rinnovo automatico. Il prezzo lo ricalcola sempre il server dal listino. I piani gratuiti non passano dal pagamento.
 Con un pagamento ospitato dal provider il server restituisce la pagina su cui portare l'utente.
 Arriva dalla pagina Gestione licenze con l'offerta (listino risolto) nello state della navigazione.
*/
@Component({
  selector: 'app-license-subscribe-page',
  templateUrl: './license-subscribe-page.component.html',
  styleUrls: ['./license-subscribe-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class LicenseSubscribePageComponent extends BasePageConfirmCancelComponent<LicenseSubscribeInModel> {

  public offerta: LicenseOfferModel | null = null;
  public metodi: MetodoPagamento[] = [];
  public debugPagamenti = false;
  public attuale: LicenseSubscriptionModel | null = null;
  public readonly periodicita = [PeriodicitaLicenza.Mensile, PeriodicitaLicenza.Annuale];
  private messaggio: string | null = null;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private licenseService: LicenseService,
              private userNavigationService: UserNavigationService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    this.btnConfirm.text = this.transloco.translate('pages.licenseSubscribe.confirm');
  }

  get Title(): string { return this.transloco.translate('pages.licenseSubscribe.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      livello: [null, [Validators.required]],
      periodicita: [PeriodicitaLicenza.Mensile, [Validators.required]],
      metodo: [MetodoPagamento.Nessuno],
      rinnovoAutomatico: [false],
    });
  }

  LoadData = (): Observable<LicenseSubscribeInModel | null> => {
    const state = history.state ?? {};
    this.offerta = state.offerta ?? null;
    this.metodi = state.metodo ?? [];
    this.debugPagamenti = !!state.debugPagamenti;
    this.attuale = state.attuale ?? null;
    if (!this.offerta || this.offerta.piano.length === 0)
      return of(null);

    const model = new LicenseSubscribeInModel();
    model.ambito = this.offerta.ambito;
    // piano proposto: il primo diverso da quello attuale
    model.livello = (this.offerta.piano.find(p => p.livello !== this.attuale?.livello) ?? this.offerta.piano[0]).livello;
    model.periodicita = this.attuale?.periodicita ?? PeriodicitaLicenza.Mensile;
    model.metodo = this.metodi[0] ?? MetodoPagamento.Nessuno;
    return of(model);
  };

  get piano(): LicensePlanModel | undefined {
    const livello = Number(this._editForm?.get('livello')?.value);
    return this.offerta?.piano.find(p => p.livello === livello);
  }

  get prezzo(): number {
    const p = this.piano;
    if (!p)
      return 0;
    return Number(this._editForm.get('periodicita')?.value) === PeriodicitaLicenza.Annuale ? p.prezzoAnnuale : p.prezzoMensile;
  }

  // limiti del piano per la funzione: Presenze utenti, CRM clienti, Documentale spazio, Cumulativo tutti (§5.1)
  limitiPiano(p: LicensePlanModel): string {
    const t = (k: string, par?: any) => this.transloco.translate(k, par);
    const ambito = this.offerta?.ambito;
    const tutti = ambito === AmbitoLicenza.Cumulativo;
    const out: string[] = [];
    if (tutti || ambito === AmbitoLicenza.AttendanceTracking)
      out.push(p.maxUtenti == null ? t('pages.licenseSubscribe.unlimitedUsers') : t('pages.licenseSubscribe.users', { max: p.maxUtenti }));
    if (tutti || ambito === AmbitoLicenza.Crm)
      out.push(p.maxClienti == null ? t('pages.licenseSubscribe.unlimitedClients') : t('pages.licenseSubscribe.clients', { max: p.maxClienti }));
    if (tutti || ambito === AmbitoLicenza.DocumentManagement)
      out.push(p.maxSpazioMB == null ? t('pages.licenseSubscribe.unlimitedStorage')
        : t('pages.licenseSubscribe.storage', { max: p.maxSpazioMB >= 1024 ? (p.maxSpazioMB / 1024).toLocaleString(undefined, { maximumFractionDigits: 1 }) + ' GB' : p.maxSpazioMB + ' MB' }));
    return out.join(', ');
  }

  // a pagamento senza metodi disponibili non si puo' confermare
  get pagamentoNonDisponibile(): boolean {
    return this.prezzo > 0 && this.metodi.length === 0;
  }

  SaveData = (editModel: LicenseSubscribeInModel): Observable<boolean> => {
    if (this.pagamentoNonDisponibile)
      return of(false);
    const request = new GenericRequest<LicenseSubscribeInModel>(LicenseSubscribeInModel);
    request.data = {
      ambito: this.offerta!.ambito,
      livello: Number(editModel.livello),
      periodicita: Number(editModel.periodicita),
      metodo: this.prezzo > 0 ? Number(editModel.metodo) : MetodoPagamento.Nessuno,
      rinnovoAutomatico: !!editModel.rinnovoAutomatico,
      returnUrl: window.location.origin + '/licensemanagement'
    };
    return this.licenseService.LicenseSubscribe(request).pipe(
      map(res => {
        if (res.success && res.data?.redirectUrl) {
          // pagamento ospitato dal provider: l'esito arriva al ritorno
          window.location.href = res.data.redirectUrl;
          return false;
        }
        if (res.success)
          this.messaggio = res.data?.messages?.[0]?.text ?? null;
        return res.success;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  // dopo la sottoscrizione si torna alla gestione licenze con l'esito; in caso di errore si resta sulla pagina
  override AfterSave(saved: boolean): void {
    if (!saved)
      return;
    this.userNavigationService.RefreshLicense();
    this.navCtrl.navigateBack('/licensemanagement', { state: { messaggio: this.messaggio } });
  }
}

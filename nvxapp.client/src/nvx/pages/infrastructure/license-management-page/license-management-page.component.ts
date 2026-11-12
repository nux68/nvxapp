import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { LicenseService } from '../../../ClientServer-Service/Infrastructure/License/license.service';
import {
  AmbitoLicenza, FonteCopertura, LicenseAutoRenewPutInModel, LicenseCancelInModel, LicenseFunctionStatusModel, LicenseOfferModel, LicenseRenewInModel,
  LicenseStatusGetInModel, LicenseStatusGetOutModel, LicenseSubscriptionModel, StatoAbbonamento, TipoSoggettoLicenza
} from '../../../ClientServer-Service/Infrastructure/License/Models/license-model';
import { StatoLicenza } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

/*
 Gestione licenze dell'azienda o dello studio (ANALISI_INFRASTRUCTURE_LICENZE.md §4.3, §6, §10):
    - stato per funzione (Presenze, CRM, Documentale): livello, da dove viene (propria o dello studio), scadenza, limite utenti
    - licenze sottoscritte: rinnovo, rinnovo automatico e disdetta (solo PowerAdmin); la disdetta toglie subito la funzione dal menu
    - sottoscrizione o cambio piano dal listino risolto (l'azienda coperta dallo studio vede solo i piani superiori)
    - storico dei pagamenti; per lo studio, le aziende coperte dalle sue licenze
 Pagina di sola consultazione con azioni: i form stanno nella pagina di sottoscrizione.
*/
@Component({
  selector: 'app-license-management-page',
  templateUrl: './license-management-page.component.html',
  styleUrls: ['./license-management-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class LicenseManagementPageComponent {

  public title: string;
  public status: LicenseStatusGetOutModel | null = null;
  public messaggio: string | null = null;
  public readonly Fonte = FonteCopertura;
  public readonly Stato = StatoLicenza;
  public readonly StatoAbbonamento = StatoAbbonamento;

  constructor(private navCtrl: NavController,
              private licenseService: LicenseService,
              private collectionDialogService: CollectionDialogService,
              private userNavigationService: UserNavigationService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('pages.licenseManagement.title');
  }

  ionViewWillEnter() {
    // esito della sottoscrizione appena fatta (dalla pagina di sottoscrizione)
    this.messaggio = history.state?.messaggio ?? null;
    this.load();
  }

  private load() {
    this.licenseService.LicenseStatusGet(new GenericRequest<LicenseStatusGetInModel>(LicenseStatusGetInModel)).subscribe(res => {
      this.status = res.success && res.data ? res.data : null;
    });
  }

  get isStudio(): boolean {
    return this.status?.tipoSoggetto === TipoSoggettoLicenza.FinancialAdvisor;
  }

  get cumulativo(): AmbitoLicenza {
    return AmbitoLicenza.Cumulativo;
  }

  abbonamentiDi(ambito: AmbitoLicenza): LicenseSubscriptionModel[] {
    return (this.status?.abbonamento ?? []).filter(x => x.ambito === ambito);
  }

  offertaDi(ambito: AmbitoLicenza): LicenseOfferModel | undefined {
    return (this.status?.offerta ?? []).find(x => x.ambito === ambito);
  }

  // stato della funzione da mostrare: livello e scadenza, oppure non attiva / scaduta
  descrizioneStato(f: LicenseFunctionStatusModel): string {
    if (f.livello == null)
      return this.transloco.translate(f.stato === StatoLicenza.Expired ? 'pages.licenseManagement.expired' : 'pages.licenseManagement.notActive');
    const livello = f.descrizionePiano || this.transloco.translate('pages.license.level' + f.livello);
    const fonte = f.fonte === FonteCopertura.Studio
      ? this.transloco.translate('pages.licenseManagement.coveredByStudio', { studio: f.studio ?? '' })
      : '';
    return [livello, fonte].filter(x => x).join(' – ');
  }

  coloreStato(f: LicenseFunctionStatusModel): string {
    if (f.livello == null)
      return f.stato === StatoLicenza.Expired ? 'danger' : 'medium';
    return f.stato === StatoLicenza.Grace ? 'warning' : 'success';
  }

  sottoscrivi(ambito: AmbitoLicenza) {
    const offerta = this.offertaDi(ambito);
    if (!offerta || !this.status)
      return;
    this.navCtrl.navigateForward('/licensesubscribe', {
      state: {
        offerta,
        metodo: this.status.metodo,
        debugPagamenti: this.status.debugPagamenti,
        attuale: this.abbonamentiDi(ambito).find(x => x.stato === StatoAbbonamento.Attivo) ?? null
      }
    });
  }

  async rinnova(a: LicenseSubscriptionModel) {
    const message = this.transloco.translate('pages.licenseManagement.confirmRenew', {
      piano: a.descrizionePiano ?? '',
      prezzo: a.prezzo.toFixed(2),
      valuta: a.valuta
    });
    if (!await this.collectionDialogService.ConfirmCancelDialog(message))
      return;
    const request = new GenericRequest<LicenseRenewInModel>(LicenseRenewInModel);
    request.data.idAbbonamento = a.id;
    request.data.returnUrl = window.location.href;
    this.licenseService.LicenseRenew(request).subscribe(res => {
      if (res.success && res.data?.redirectUrl) {
        window.location.href = res.data.redirectUrl;
        return;
      }
      if (res.success) {
        this.messaggio = res.data?.messages?.[0]?.text ?? null;
        this.userNavigationService.RefreshLicense();
      }
      this.load();
    });
  }

  rinnovoAutomatico(a: LicenseSubscriptionModel, valore: boolean) {
    const request = new GenericRequest<LicenseAutoRenewPutInModel>(LicenseAutoRenewPutInModel);
    request.data.idAbbonamento = a.id;
    request.data.rinnovoAutomatico = valore;
    this.licenseService.LicenseAutoRenewPut(request).subscribe(res => {
      if (res.success && res.data)
        a.rinnovoAutomatico = res.data.abbonamento.rinnovoAutomatico;
      else
        this.load();
    });
  }

  // disdetta immediata (ANALISI_INFRASTRUCTURE_LICENZE.md §15.8, §15.9): la funzione esce dal menu, nessun rimborso;
  // per l'azienda, con il sistema licenze acceso, i dati delle funzioni rimaste senza copertura vengono eliminati
  async disdici(a: LicenseSubscriptionModel) {
    const t = (k: string, p?: any) => this.transloco.translate(k, p);
    const message = [
      t('pages.licenseManagement.confirmCancel', {
        piano: a.descrizionePiano || t('pages.license.level' + a.livello),
        funzione: t('pages.license.function' + a.ambito)
      }),
      a.prezzo > 0 && a.stato === StatoAbbonamento.Attivo
        ? t('pages.licenseManagement.confirmCancelNoRefund', { data: formatDate(a.dataScadenza, 'dd/MM/yyyy', 'en-US') })
        : '',
      a.disponibileSuCompany ? t('pages.licenseManagement.confirmCancelShared') : '',
      !this.isStudio && this.status?.attive ? t('pages.licenseManagement.confirmCancelDeleteData') : ''
    ].filter(x => x).join(' ');
    if (!await this.collectionDialogService.ConfirmCancelDialog(message))
      return;
    const request = new GenericRequest<LicenseCancelInModel>(LicenseCancelInModel);
    request.data.idAbbonamento = a.id;
    this.licenseService.LicenseCancel(request).subscribe(res => {
      if (res.success) {
        // disdetta ed eventuali dati eliminati
        this.messaggio = (res.data?.messages ?? []).map(m => m.text).filter(x => x).join(' ') || null;
        // rilegge applicativi e licenza: le voci di menu della funzione disdetta spariscono
        this.userNavigationService.RefreshLicense();
      }
      this.load();
    });
  }

  disdicibile(a: LicenseSubscriptionModel): boolean {
    return !!this.status?.canManage && a.stato !== StatoAbbonamento.Annullato;
  }

  rinnovabile(a: LicenseSubscriptionModel): boolean {
    return !!this.status?.canManage && (a.stato === StatoAbbonamento.Attivo || a.stato === StatoAbbonamento.Scaduto);
  }

  utenti(): string {
    if (!this.status)
      return '';
    return this.status.limiteUtenti == null
      ? this.transloco.translate('pages.licenseManagement.usersUnlimited', { attivi: this.status.utentiAttivi })
      : this.transloco.translate('pages.licenseManagement.usersOfLimit', { attivi: this.status.utentiAttivi, limite: this.status.limiteUtenti });
  }

  // limite della funzione con l'uso attuale (ANALISI_INFRASTRUCTURE_LICENZE.md §5.1): Presenze utenti, CRM clienti attivi, Documentale spazio
  limite(f: LicenseFunctionStatusModel): string {
    const t = (k: string, p?: any) => this.transloco.translate(k, p);
    switch (f.ambito) {
      case AmbitoLicenza.AttendanceTracking:
        return f.maxUtenti == null
          ? t('pages.licenseManagement.usersUnlimitedOf', { attivi: this.status?.utentiAttivi ?? 0 })
          : t('pages.licenseManagement.usersOf', { attivi: this.status?.utentiAttivi ?? 0, max: f.maxUtenti });
      case AmbitoLicenza.Crm:
        return f.maxClienti == null
          ? t('pages.licenseManagement.clientsUnlimitedOf', { attivi: f.clientiAttivi ?? 0 })
          : t('pages.licenseManagement.clientsOf', { attivi: f.clientiAttivi ?? 0, max: f.maxClienti });
      case AmbitoLicenza.DocumentManagement:
        return f.maxSpazioMB == null
          ? t('pages.licenseManagement.storageUnlimitedOf', { usato: this.mb(f.spazioUsatoMB ?? 0) })
          : t('pages.licenseManagement.storageOf', { usato: this.mb(f.spazioUsatoMB ?? 0), max: this.mb(f.maxSpazioMB) });
      default:
        return '';
    }
  }

  // MB in forma leggibile (oltre 1024 MB in GB)
  mb(valore: number): string {
    return valore >= 1024 ? (valore / 1024).toLocaleString(undefined, { maximumFractionDigits: 1 }) + ' GB' : valore + ' MB';
  }

  funzioniSuperiori(ambiti: number[]): string {
    return ambiti.map(a => this.transloco.translate('pages.license.function' + a)).join(', ');
  }
}

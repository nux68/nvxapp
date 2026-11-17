import { Component, ChangeDetectionStrategy } from '@angular/core';
import { Subscription } from 'rxjs';
import { formatDate } from '@angular/common';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { EsitoProvider, ProviderCheckoutService } from '../../../Utility/infrastructure/provider-checkout.service';
import { LicenseService } from '../../../ClientServer-Service/Infrastructure/License/license.service';
import {
  AmbitoLicenza, FonteCopertura, LicenseAutoRenewPutInModel, LicenseCancelInModel, LicenseFunctionStatusModel, LicenseOfferModel, LicensePaymentConfirmInModel, LicenseRenewInModel,
  LicenseStatusGetInModel, LicenseStatusGetOutModel, LicenseSubscriptionModel, MetodoPagamento, StatoAbbonamento, StatoPagamento, TipoSoggettoLicenza
} from '../../../ClientServer-Service/Infrastructure/License/Models/license-model';
import { StatoLicenza } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

/*
 Gestione licenze dell'azienda o dello studio (ANALISI_INFRASTRUCTURE_LICENZE.md §4.3, §6, §10):
    - stato per funzione (Presenze, CRM, Documentale): livello, da dove viene (propria o dello studio), scadenza, limite utenti
    - licenze sottoscritte: rinnovo, rinnovo automatico e disdetta (solo PowerAdmin); la disdetta toglie subito la funzione dal menu
    - sottoscrizione o cambio piano dal listino risolto (l'azienda coperta dallo studio vede solo i piani superiori)
    - storico dei pagamenti; per lo studio, le aziende coperte dalle sue licenze
    - pagamento ospitato dal provider (PayPal, §7.4) in una finestra a parte (ProviderCheckoutService): l'esito arriva dalla
      pagina di ritorno e la pagina lo conferma (LicensePaymentConfirm); "Verifica pagamento" per i pagamenti rimasti in attesa
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
  public messaggioColore = 'success';
  // pagamento aperto nella finestra del provider: abbonamento e pagina del provider (per riaprirla se la finestra e' stata bloccata)
  public attesa: { idAbbonamento: number; redirectUrl: string | null } | null = null;
  private esitiSub: Subscription | null = null;
  public readonly Fonte = FonteCopertura;
  public readonly Stato = StatoLicenza;
  public readonly StatoAbbonamento = StatoAbbonamento;

  constructor(private navCtrl: NavController,
              private licenseService: LicenseService,
              private collectionDialogService: CollectionDialogService,
              private userNavigationService: UserNavigationService,
              private checkout: ProviderCheckoutService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('pages.licenseManagement.title');
  }

  ionViewWillEnter() {
    // esito della sottoscrizione appena fatta (dalla pagina di sottoscrizione)
    this.messaggio = history.state?.messaggio ?? null;
    this.messaggioColore = history.state?.attesaPagamento ? 'warning' : 'success';
    this.attesa = history.state?.attesaPagamento ?? null;
    this.esitiSub?.unsubscribe();
    this.esitiSub = this.checkout.esiti().subscribe(e => this.esitoProvider(e));
    this.load();
  }

  ionViewWillLeave() {
    this.esitiSub?.unsubscribe();
    this.esitiSub = null;
  }

  // esito dalla pagina di ritorno del provider (finestra di pagamento)
  private esitoProvider(e: EsitoProvider) {
    const request = new GenericRequest<LicensePaymentConfirmInModel>(LicensePaymentConfirmInModel);
    request.data.riferimento = e.token;
    request.data.annullato = e.esito !== 'ok';
    this.conferma(request);
  }

  // "Verifica pagamento": ultimo pagamento in attesa dell'abbonamento (finestra chiusa, bloccata o esito perso)
  verificaPagamento(idAbbonamento: number) {
    const request = new GenericRequest<LicensePaymentConfirmInModel>(LicensePaymentConfirmInModel);
    request.data.idAbbonamento = idAbbonamento;
    this.conferma(request);
  }

  private conferma(request: GenericRequest<LicensePaymentConfirmInModel>) {
    this.licenseService.LicensePaymentConfirm(request).subscribe({
      next: res => {
        if (res.success && res.data) {
          const stato = res.data.stato;
          this.messaggio = res.data.messages?.map(m => m.text).join(' ') || null;
          this.messaggioColore = stato === StatoPagamento.Completato || stato === StatoPagamento.Debug ? 'success'
            : stato === StatoPagamento.Fallito && !request.data.annullato ? 'danger' : 'warning';
          // ancora in attesa: il pannello resta (si puo' riaprire la pagina del provider o verificare di nuovo)
          if (stato !== StatoPagamento.InAttesa && stato !== StatoPagamento.Creato)
            this.attesa = null;
          this.userNavigationService.RefreshLicense();
        }
        this.load();
      },
      error: () => this.load()
    });
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
        metodoSimulato: this.status.metodoSimulato ?? [],
        metodoSandbox: this.status.metodoSandbox ?? [],
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
    request.data.returnUrl = window.location.origin + '/assets/pagamento-ritorno.html';
    // pagamento dal provider: la finestra si apre adesso, ancora dentro il clic di conferma (dopo la chiamata verrebbe bloccata)
    const metodo = a.metodo || MetodoPagamento.CartaCredito;
    const esterno = a.prezzo > 0 && !(this.status?.metodoSimulato ?? []).includes(metodo);
    if (esterno)
      this.checkout.apri();
    this.licenseService.LicenseRenew(request).subscribe(res => {
      if (res.success && res.data?.redirectUrl) {
        this.inAttesa(a.id, res.data.redirectUrl);
        return;
      }
      if (esterno)
        this.checkout.chiudi();
      if (res.success) {
        this.messaggio = res.data?.messages?.[0]?.text ?? null;
        this.userNavigationService.RefreshLicense();
      }
      this.load();
    });
  }

  // pagamento avviato sulla pagina del provider: la finestra (se il browser l'ha aperta) va su PayPal, qui si aspetta l'esito
  private inAttesa(idAbbonamento: number, redirectUrl: string) {
    const aperta = this.checkout.vai(redirectUrl);
    this.attesa = { idAbbonamento, redirectUrl };
    this.messaggio = this.transloco.translate(aperta ? 'pages.licenseManagement.paymentWaiting' : 'pages.licenseManagement.popupBlocked');
    this.messaggioColore = 'warning';
    this.load();
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

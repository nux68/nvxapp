import { Component, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { NavController } from '@ionic/angular';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';
import { LicenseService } from '../../../ClientServer-Service/Infrastructure/License/license.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import {
  AmbitoLicenza, LicensePlanModel, LicensePublicCatalogModel, LicensePublicPriceListInModel, TipoSoggettoLicenza
} from '../../../ClientServer-Service/Infrastructure/License/Models/license-model';
import { LivelloLicenza } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

/*
 Showcase: piani e prezzi con le domande frequenti.
 Piani, prezzi e limiti vengono dai listini globali del SuperUser (License/LicensePublicPriceList, senza login):
 cambiando i listini cambia la pagina. I testi (titolo, compreso in tutti i piani, nota, domande) e le funzioni
 di cui mostrare il listino sono in ShowcaseService.Contenuti.pricing, ridefinibili dai rami.
  - Aziende / Studi: listini per tipo di soggetto (il selettore compare solo se ci sono entrambi)
  - Mensile / Annuale: prezzo al mese, con l'annuale l'importo annuo diviso 12 e il risparmio
  - con piu' funzioni (es. applicativo completo) si sceglie la funzione
*/
@Component({
  selector: 'app-showcase-pricing-page',
  templateUrl: './showcase-pricing-page.component.html',
  styleUrls: ['./showcase-pricing-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ShowcasePricingPageComponent implements OnInit {

  public readonly title: string;
  public readonly Tipo = TipoSoggettoLicenza;

  /** listini delle funzioni della pagina; null = in caricamento */
  public listini: LicensePublicCatalogModel[] | null = null;
  public ambito: AmbitoLicenza = AmbitoLicenza.Cumulativo;
  public tipo: TipoSoggettoLicenza = TipoSoggettoLicenza.Company;
  public annuale = false;

  constructor(public showcaseService: ShowcaseService,
              private licenseService: LicenseService,
              private navCtrl: NavController,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('showcase.menu.pricing');
  }

  public get c(): ShowcaseContenuti['pricing'] {
    return this.showcaseService.Contenuti.pricing;
  }

  ngOnInit(): void {
    const request = new GenericRequest<LicensePublicPriceListInModel>(LicensePublicPriceListInModel);
    request.data.ambito = this.c.ambiti ?? [];
    this.licenseService.LicensePublicPriceList(request).subscribe({
      next: res => this.imposta(res.success ? res.data?.listino ?? [] : []),
      error: () => this.imposta([])
    });
  }

  private imposta(listini: LicensePublicCatalogModel[]): void {
    this.listini = listini;
    this.ambito = this.ambiti[0] ?? AmbitoLicenza.Cumulativo;
    this.tipo = this.tipi.includes(TipoSoggettoLicenza.Company) ? TipoSoggettoLicenza.Company : (this.tipi[0] ?? TipoSoggettoLicenza.Company);
  }

  // ---------------------------------------------------------------- selettori

  /** funzioni con un listino, nell'ordine dei contenuti della pagina */
  public get ambiti(): AmbitoLicenza[] {
    const conListino = new Set((this.listini ?? []).map(l => l.ambito));
    const ordine = this.c.ambiti?.length ? this.c.ambiti : [...conListino];
    return ordine.filter(a => conListino.has(a));
  }

  /** tipi di soggetto con un listino per la funzione scelta (prima le aziende) */
  public get tipi(): TipoSoggettoLicenza[] {
    return [TipoSoggettoLicenza.Company, TipoSoggettoLicenza.FinancialAdvisor]
      .filter(t => (this.listini ?? []).some(l => l.ambito === this.ambito && l.tipoSoggetto === t));
  }

  public scegliAmbito(ambito: AmbitoLicenza): void {
    this.ambito = ambito;
    if (!this.tipi.includes(this.tipo))
      this.tipo = this.tipi[0] ?? TipoSoggettoLicenza.Company;
  }

  public get listino(): LicensePublicCatalogModel | undefined {
    return (this.listini ?? []).find(l => l.ambito === this.ambito && l.tipoSoggetto === this.tipo);
  }

  public nomeFunzione(ambito: AmbitoLicenza): string {
    return this.transloco.translate('pages.license.function' + ambito);
  }

  // ---------------------------------------------------------------- schede

  public nome(p: LicensePlanModel): string {
    return p.descrizione || p.codice;
  }

  /** piano consigliato: Standard */
  public isConsigliato(p: LicensePlanModel): boolean {
    return p.livello === LivelloLicenza.Standard;
  }

  public prezzoPeriodo(p: LicensePlanModel): number {
    return this.annuale ? p.prezzoAnnuale : p.prezzoMensile;
  }

  /** prezzo mostrato in grande: al mese (con l'annuale, l'importo annuo diviso per 12) */
  public prezzoAlMese(p: LicensePlanModel): number {
    return this.annuale ? p.prezzoAnnuale / 12 : p.prezzoMensile;
  }

  /** risparmio dell'annuale rispetto a 12 mensilita' (percentuale intera; 0 se non c'e') */
  public risparmio(p: LicensePlanModel): number {
    if (!p.prezzoMensile)
      return 0;
    return Math.max(0, Math.round((1 - p.prezzoAnnuale / (p.prezzoMensile * 12)) * 100));
  }

  public get risparmioMassimo(): number {
    return Math.max(0, ...(this.listino?.piano ?? []).map(p => this.risparmio(p)));
  }

  public euro(valore: number): string {
    return valore.toLocaleString(this.transloco.getActiveLang(), { style: 'currency', currency: this.listino?.valuta || 'EUR', minimumFractionDigits: 2 });
  }

  /** funzioni comprese: quella del listino, oppure tutte con il Cumulativo */
  public get funzioniIncluse(): string {
    if (this.ambito === AmbitoLicenza.Cumulativo)
      return [AmbitoLicenza.AttendanceTracking, AmbitoLicenza.Crm, AmbitoLicenza.DocumentManagement].map(a => this.nomeFunzione(a)).join(', ');
    return this.nomeFunzione(this.ambito);
  }

  /** limiti del piano per la funzione: Presenze utenti, CRM clienti, Documentale spazio, Cumulativo tutti */
  public limiti(p: LicensePlanModel): string[] {
    const t = (k: string, par?: Record<string, unknown>) => this.transloco.translate('pages.licenseSubscribe.' + k, par);
    const tutti = this.ambito === AmbitoLicenza.Cumulativo;
    const out: string[] = [];
    if (tutti || this.ambito === AmbitoLicenza.AttendanceTracking)
      out.push(p.maxUtenti == null ? t('unlimitedUsers') : t('users', { max: p.maxUtenti }));
    if (tutti || this.ambito === AmbitoLicenza.Crm)
      out.push(p.maxClienti == null ? t('unlimitedClients') : t('clients', { max: p.maxClienti }));
    if (tutti || this.ambito === AmbitoLicenza.DocumentManagement)
      out.push(p.maxSpazioMB == null ? t('unlimitedStorage') : t('storage', { max: this.spazio(p.maxSpazioMB) }));
    return out;
  }

  private spazio(mb: number): string {
    return mb >= 1024 ? (mb / 1024).toLocaleString(this.transloco.getActiveLang(), { maximumFractionDigits: 1 }) + ' GB' : mb + ' MB';
  }

  // ---------------------------------------------------------------- pulsanti

  /** registrazione dell'azienda o dello studio del listino scelto (si parte dal Free e si sceglie il piano dopo il login) */
  public cta(p: LicensePlanModel): string {
    if (this.prezzoPeriodo(p) === 0)
      return this.transloco.translate('showcase.pricingPage.start');
    return this.transloco.translate('pages.licenseSubscribe.choose', { piano: this.nome(p) });
  }

  /** registrazione con il tipo gia' scelto: listino degli studi -> Studio, altrimenti Azienda */
  public scegli(): void {
    this.navCtrl.navigateRoot('/register', { queryParams: { tipo: this.tipo === TipoSoggettoLicenza.FinancialAdvisor ? 'studio' : 'azienda' } });
  }

  public vai(url: string): void {
    this.navCtrl.navigateRoot(url);
  }
}

import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserManualGenerateInModel, UserManualMenuSection, UserManualService } from '../../../ClientServer-Service/Infrastructure/UserManual/user-manual.service';
import { ApplicationType } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { MainMenuItem, MainMenuService, MenuType } from '../../../Utility/infrastructure/main-menu.service';
import { SITE_NAME } from '../../../Utility/infrastructure/site-name';

/*
 Manuale utente in PDF (codice nell'url: /usermanual/<codice>). Il pulsante avvia il job sul server; quando il PDF e'
 pronto compare, come per gli altri export, nella barra dei job in alto con il pulsante per scaricarlo.
 Il manuale si compone dal menu che l'utente vede adesso (ruoli, applicativi attivi, voci nascoste dal "Menu utenti"):
 la pagina manda al server le sezioni e le voci del menu che riguardano il manuale (stesse regole del menu laterale).
 Titolo e descrizione: chiavi pages.userManual.manuals.<codice> (ogni ramo aggiunge le sue nelle proprie traduzioni).
*/
@Component({
  selector: 'app-user-manual-page',
  templateUrl: './user-manual-page.component.html',
  styleUrls: ['./user-manual-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class UserManualPageComponent {

  public code = '';
  public inPreparazione = false;
  public avviato = false;

  // applicativo del manuale (null = funzioni base, cioe' le voci di Infrastructure)
  private static readonly APPLICATIVO: Record<string, ApplicationType | null> = {
    infrastructure: null,
    attendance: ApplicationType.AttendanceTracking,
    crm: ApplicationType.Crm,
    documents: ApplicationType.DocumentManagement,
  };

  constructor(private route: ActivatedRoute,
              private service: UserManualService,
              private authService: AuthService,
              private mainMenuService: MainMenuService,
              private transloco: TranslocoService) { }

  get title(): string { return this.testo('title'); }
  get description(): string { return this.testo('description'); }

  // testo del manuale con ripiego su quello generico
  private testo(campo: string): string {
    const chiave = `pages.userManual.manuals.${this.code}.${campo}`;
    const t = this.transloco.translate(chiave);
    return t !== chiave ? t : this.transloco.translate('pages.userManual.' + campo);
  }

  ionViewWillEnter() {
    this.code = this.route.snapshot.paramMap.get('code') ?? '';
    this.avviato = false;
  }

  scarica() {
    if (!this.code || this.inPreparazione)
      return;
    const request = new GenericRequest<UserManualGenerateInModel>(UserManualGenerateInModel);
    request.data = { code: this.code, siteName: SITE_NAME, menu: this.menuDelManuale() };
    this.inPreparazione = true;
    this.service.Generate(request).subscribe({
      next: res => { this.inPreparazione = false; this.avviato = !!res.success; },
      error: () => this.inPreparazione = false
    });
  }

  // ---------------------------------------------------------------- menu dell'utente per il manuale

  // elenchi del menu dei ruoli dell'utente, filtrati come nel menu laterale (app.component)
  private elenchi(): MainMenuItem[][] {
    const a = this.authService;
    const ruoli = a.RoleNames;
    const elenchi: MainMenuItem[][] = [];
    const conRuoli = (items: MainMenuItem[], lista: 'user' | 'company', propri: string[]) =>
      items.filter(i => (!i.roles || i.roles.some(r => propri.includes(r))) && this.mainMenuService.VoceVisibile(i, lista, ruoli));

    elenchi.push(conRuoli(this.mainMenuService.Pages4User, 'user', ruoli));
    if (a.IsSuperUser) elenchi.push(this.mainMenuService.Pages4SuperUser);
    if (a.IsInGroupAdmin) elenchi.push(this.mainMenuService.Pages4Admin.filter(i => i.component !== 'PowerAdminPageComponent' || a.IsPowerAdmin));
    if (a.IsInGroupDealerAdmin) elenchi.push(this.mainMenuService.Pages4DealerAdmin.filter(i => i.component !== 'DealerPowerAdminPageComponent' || a.IsDealerPowerAdmin));
    if (a.IsInGroupFinancialAdvisorAdmin) elenchi.push(this.mainMenuService.Pages4FinancialAdvisorAdmin.filter(i => i.component !== 'FinancialAdvisorPowerAdminPageComponent' || a.IsFinancialAdvisorPowerAdmin));
    if (a.IsInGroupCompanyAdmin) {
      const propri = [...(a.IsUser ? ['User'] : []), ...(a.IsCompanyAdmin ? ['CompanyAdmin'] : []), ...(a.IsCompanyPowerAdmin ? ['CompanyPowerAdmin'] : [])];
      elenchi.push(conRuoli(this.mainMenuService.Pages4CompanyAdmin, 'company', propri)
        .filter(i => i.component !== 'CompanyPowerAdminPageComponent' || a.IsCompanyPowerAdmin));
    }
    elenchi.push(this.mainMenuService.Pages4Support.filter(i => !i.roles || i.roles.some(r => ruoli.includes(r))));
    return elenchi;
  }

  // sezioni (intestazioni del menu, tradotte) con le voci dell'applicativo del manuale, nell'ordine del menu
  private menuDelManuale(): UserManualMenuSection[] {
    const app = UserManualPageComponent.APPLICATIVO[this.code] ?? null;
    const sezioni: UserManualMenuSection[] = [];
    for (const elenco of this.elenchi()) {
      const intestazioni = elenco.filter(i => i.menuType === MenuType.MenuHeader);
      const primo = [...intestazioni].sort((x, y) => x.group - y.group)[0];
      const voci = elenco.filter(i => i.menuType === MenuType.MenuItem && (i.application ?? null) === app)
                         .sort((x, y) => x.zorder - y.zorder);
      for (const voce of voci) {
        const intestazione = intestazioni.find(h => h.group === voce.group) ?? primo;
        const titoloSezione = intestazione ? this.transloco.translate(intestazione.title) : this.title;
        let sezione = sezioni.find(s => s.titolo === titoloSezione);
        if (!sezione) {
          sezione = { titolo: titoloSezione, voci: [] };
          sezioni.push(sezione);
        }
        if (!sezione.voci.some(v => v.url === voce.url))
          sezione.voci.push({ titolo: this.transloco.translate(voce.title), url: voce.url });
      }
    }
    return sezioni;
  }
}

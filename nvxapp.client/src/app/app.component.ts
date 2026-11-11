import { combineLatest } from 'rxjs';
import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { AccountService } from '../nvx/ClientServer-Service/Infrastructure/Account/account.service';
import { AuthService } from '../nvx/Utility/infrastructure/auth.service';
import { UserNavigationService } from '../nvx/Utility/infrastructure/user-navigation.service';
import { SignalrService } from '../nvx/Utility/infrastructure/signalr.service';
import { environment } from '../environments/environment';
import { MainMenuItem, MainMenuService, MenuType } from '../nvx/Utility/infrastructure/main-menu.service';
import { MainMenuInfrastructureService } from '../nvx/Utility/infrastructure/main-menu-infrastructure.service';
import { LongJobNotifierService } from '../nvx/Utility/infrastructure/long-job-notifier.service';
import { JobNotifierService } from '../nvx/Utility/infrastructure/job-notifier.service';
import { TicketNotifierService } from '../nvx/Utility/infrastructure/ticket-notifier.service';
import { UserNotifierService } from '../nvx/Utility/infrastructure/user-notifier.service';
import { ThemeService } from '../nvx/Utility/infrastructure/theme.service';
import { ShowcaseService } from '../nvx/Utility/infrastructure/showcase.service';
import { SITE_NAME } from '../nvx/Utility/infrastructure/site-name';
import { ApplicationType, LivelloLicenza } from '../nvx/ClientServer-Service/Infrastructure/Account/Models/user-load-model';


@Component({
  selector: 'app-root',
  templateUrl: 'app.component.html',
  styleUrls: ['app.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false,
})
export class AppComponent implements OnInit {

  public appPages4SuperUser: MainMenuItem[] = [];
  public appPages4Admin: MainMenuItem[] = [];
  public appPages4DealerAdmin: MainMenuItem[] = [];
  public appPages4FinancialAdvisorAdmin: MainMenuItem[] = [];
  public appPages4CompanyAdmin: MainMenuItem[] = [];
  public appPages4User: MainMenuItem[] = [];

  public groupedCompanyAdminMenu: GroupMainMenuItem[] = [];
  public groupedFinancialAdvisorAdminMenu: GroupMainMenuItem[] = [];
  public groupedDealerAdminMenu: GroupMainMenuItem[] = [];
  public groupedAdminMenu: GroupMainMenuItem[] = [];
  public groupedSuperUserMenu: GroupMainMenuItem[] = [];
  public groupedUserMenu: GroupMainMenuItem[] = [];
  // sezione "Assistenza", in fondo al menu per tutti gli utenti collegati
  public groupedSupportMenu: GroupMainMenuItem[] = [];
  // Menu dopo il login: Accesso, poi una sezione espandibile "Amministrazione" con le voci comuni di
  // Infrastructure e una per ogni applicativo attivo (titolo = nome dell'applicativo) con le sue voci.
  // Dentro ogni sezione le sottosezioni (gruppi dei ruoli) si aprono e si chiudono una alla volta, come le sezioni. In fondo "Assistenza".
  public sezioniAccesso: SezioneMenu[] = [];
  public sezioniApplicativi: SezioneApplicativo[] = [];
  // menu dello showcase, mostrato al posto degli altri prima del login
  public groupedShowcaseMenu: GroupMainMenuItem[] = [];
  // sezione dello showcase aperta all'inizio: la prima, oppure quella del login ("Area riservata")
  // se entrando nel sito si va al login (AuthService.IngressoAlLogin, vedi paginaIngresso)
  public showcaseMenuAperto = '';
  // sezione aperta del menu: una stringa, non un array (ion-accordion-group non e' "multiple": con un array
  // la sezione non si apriva quando il valore cambiava dopo il login)
  public selectedMenu: string = this.mainMenuInfrastructureService.GetMenuHeaderMenu(this.mainMenuService.Pages4User)?.title ?? '';
  // sottosezione aperta di ogni sezione di applicativo (chiave della sezione -> titolo della sottosezione)
  public sottosezioniAperte: Record<string, string> = {};
  private timerApertura: ReturnType<typeof setTimeout> | undefined;
  


  

  private lastAuthStatus: boolean = false;

  public appPages = [


    { menuType: MenuType.MenuItem, zorder:0 , title: 'Inbox', url: '/folder/inbox', icon: 'mail' },
    { title: 'Outbox', url: '/folder/outbox', icon: 'paper-plane' },
    { title: 'Favorites', url: '/folder/favorites', icon: 'heart' },
    { title: 'Archived', url: '/folder/archived', icon: 'archive' },
    { title: 'Trash', url: '/folder/trash', icon: 'trash' },
    { title: 'Spam', url: '/folder/spam', icon: 'warning' },
  ];
  public labels = ['Family', 'Friends', 'Notes', 'Work', 'Travel', 'Reminders'];
  constructor(public authService: AuthService,
              public userNavigationService: UserNavigationService,
              public signalrService: SignalrService,
              private mainMenuService: MainMenuService,
              private mainMenuInfrastructureService: MainMenuInfrastructureService,
              private longJobNotifierService: LongJobNotifierService,
              private jobNotifierService: JobNotifierService,
              public ticketNotifierService: TicketNotifierService,
              private userNotifierService: UserNotifierService,
              private themeService: ThemeService, // applica il tema chiaro/scuro salvato fin dall'avvio
              public showcaseService: ShowcaseService,
              titleService: Title
              )
  {

    // titolo della scheda del browser: nome del sito del ramo
    titleService.setTitle(SITE_NAME);

    this.loadPages();
    this.groupedShowcaseMenu = this.groupMenu(this.showcaseService.Pages);
    const sezioneLogin = this.authService.IngressoAlLogin
      ? this.groupedShowcaseMenu.find(g => g.menuItem.some(p => p.url === '/login'))
      : undefined;
    this.showcaseMenuAperto = (sezioneLogin ?? this.groupedShowcaseMenu[0])?.menuHeader.title ?? '';

  }

  // voci di menu per ruolo; quelle degli applicativi solo se attivi (MainMenuService)
  private loadPages(): void {
    this.appPages4SuperUser = this.mainMenuService.Pages4SuperUser;
    this.appPages4Admin = this.mainMenuService.Pages4Admin;
    this.appPages4DealerAdmin = this.mainMenuService.Pages4DealerAdmin;
    this.appPages4FinancialAdvisorAdmin = this.mainMenuService.Pages4FinancialAdvisorAdmin;
    this.appPages4CompanyAdmin = this.mainMenuService.Pages4CompanyAdmin;
    this.appPages4User = this.mainMenuService.Pages4User;
  }

  ngOnInit() {

    if (environment.signalR.useSignalR) {
       // collego gli eventi solo dopo la connessione che avviene dopo il login
       this.signalrService.IsConnect$.subscribe(res => {

        if (res == true) {
          // 🔹 Sottoscrizione agli eventi della chat
          this.signalrService.on('ReceiveMessage').subscribe((msg: string) => {
            console.log('🔄 Dati ricevuti:', msg);
          });

          // 🔹 Esempio: Sottoscrizione a un altro evento (es. aggiornamento dati)
          this.signalrService.on('UpdateData').subscribe((data) => {
            console.log('🔄 Dati aggiornati:', data);
          });

          this.signalrService.on('LongJobProgress').subscribe((data) => {
            console.log('🔄 ReceiveJob Progress:', data);

            this.longJobNotifierService.addOrUpdateJob(data);

          });

          this.signalrService.on('JobNotifierAsync').subscribe((data) => {
            console.log('?? JobNotifierAsync Executed:', data);

            this.jobNotifierService.jobNotifier(data);

          });

          // ticket di assistenza: avviso, contatore del menu e pagine aperte
          this.signalrService.on('TicketNotifier').subscribe((data) => {
            this.ticketNotifierService.ticketNotifier(data);
          });

          // avvisi a comparsa degli applicativi (es. promemoria del CRM)
          this.signalrService.on('UserNotifier').subscribe((data) => {
            this.userNotifierService.userNotifier(data);
          });

        }

      });
    }

    // menu ricalcolati quando cambiano i ruoli o gli applicativi attivi dell'azienda
    combineLatest([this.authService.Roles$, this.authService.ActiveApplications$, this.authService.MenuRestrictions$]).subscribe(() => {
      this.loadPages();
      this.groupedCompanyAdminMenu = this.getGroupedCompanyAdminMenu();
      this.groupedFinancialAdvisorAdminMenu = this.getGroupedFinancialAdvisorAdmin();
      this.groupedDealerAdminMenu = this.getGroupedDealerAdmin();
      this.groupedAdminMenu = this.getGroupedAdmin();
      this.groupedSuperUserMenu = this.getGroupedSuperUser();
      this.groupedUserMenu = this.getGroupedUser();
      this.groupedSupportMenu = this.getGroupedSupport();
      this.componiMenu();

      // ticket in carico dell'utente (anche dopo un'impersonificazione)
      this.ticketNotifierService.refreshCount();

      // dopo il login (e dopo un'impersonificazione) si apre la prima sezione con voci, esclusa "Accesso".
      // Si imposta dopo che le sezioni nuove sono state disegnate: ion-accordion-group apre solo gli accordion
      // che esistono gia' quando il valore cambia (al login le sezioni arrivano insieme ai ruoli).
      // Vale anche per le sottosezioni: dopo un'impersonificazione la sezione "Amministrazione" resta, ma le sue
      // sottosezioni cambiano (es. da "SuperUser" a "Centro") e quella nuova non si aprirebbe.
      const sezione = this.primaSezioneAperta();
      const sottosezioni = Object.fromEntries(this.sezioniApplicativi.map(a => [a.chiave, a.aperta]));
      // Al login e all'impersonificazione ruoli e applicativi arrivano uno dopo l'altro: conta solo l'ultimo ricalcolo,
      // altrimenti si aprirebbe prima "Amministrazione" (ancora senza applicativi) e, a meta' animazione, non si richiuderebbe.
      clearTimeout(this.timerApertura);
      this.timerApertura = setTimeout(() => {
        this.selectedMenu = sezione;
        this.sottosezioniAperte = sottosezioni;
      });

      

      
      
    });

  }

  
  // voci dei ruoli dell'utente, nell'ordine del menu (utente, SuperUser, Admin, Centro, Studio, Azienda),
  // con la regola che decide quali voci mostrare
  private vociRuoli(): [MainMenuItem[], (component: string) => boolean][] {
    const ruoli: [MainMenuItem[], boolean, (component: string) => boolean][] = [
      [this.getMenuItem4User(), true, c => this.showPage4User(c)],
      [this.appPages4SuperUser, this.authService.IsSuperUser, c => this.showPage4SuperUser(c)],
      [this.appPages4Admin, this.authService.IsInGroupAdmin, c => this.showPage4Admin(c)],
      [this.appPages4DealerAdmin, this.authService.IsInGroupDealerAdmin, c => this.showPage4DealerAdmin(c)],
      [this.appPages4FinancialAdvisorAdmin, this.authService.IsInGroupFinancialAdvisorAdmin, c => this.showPage4FinancialAdvisorAdmin(c)],
      [this.getMenuItem4CompanyAdmin(), this.authService.IsInGroupCompanyAdmin, c => this.showPage4CompanyAdmin(c)],
    ];
    return ruoli.filter(([, visibili]) => visibili).map(([voci, , mostra]) => [voci, mostra]);
  }

  // un gruppo ha almeno una voce visibile con la regola della sua sezione
  private haVoci(gruppi: GroupMainMenuItem[], mostra: (component: string) => boolean): boolean {
    return gruppi.some(g => g.menuItem.some(p => p.menuType === MenuType.MenuItem && mostra(p.component)));
  }

  // Accesso in testa; poi "Amministrazione" (voci di Infrastructure) e una sezione per applicativo attivo
  private componiMenu(): void {
    const accesso = this.mainMenuInfrastructureService.GetMenuHeaderMenu(this.mainMenuService.Pages4User)?.title ?? '';
    const ruoli = this.vociRuoli();

    const [vociUtente, mostraUtente] = ruoli[0];
    this.sezioniAccesso = [{ gruppi: this.groupMenu(vociUtente).filter(g => g.menuHeader.title === accesso), mostra: mostraUtente }];

    // applicativi con voci, nell'ordine dell'enum (le voci lato studio contano per l'applicativo)
    const applicativi = [...new Set(ruoli.flatMap(([voci]) => voci.map(v => v.application)).filter((a): a is ApplicationType => a != null))]
      .sort((a, b) => a - b);
    const chiavi: (ApplicationType | null)[] = [null, ...applicativi];

    this.sezioniApplicativi = chiavi.map(app => {
      // gruppi dell'applicativo per ogni ruolo: intestazioni prese da tutto l'elenco del ruolo (un applicativo puo'
      // mettere le sue voci in un gruppo dichiarato da Infrastructure, es. "Generale")
      const sezioni: SezioneMenu[] = ruoli
        .map(([voci, mostra]) => ({
          gruppi: this.groupMenu(voci.filter(v => (v.application ?? null) === app), voci)
                      .filter(g => g.menuHeader.title !== accesso),
          mostra
        }))
        .filter(s => this.haVoci(s.gruppi, s.mostra));
      const primo = sezioni.flatMap(s => s.gruppi.filter(g => g.menuItem.some(p => p.menuType === MenuType.MenuItem && s.mostra(p.component))))[0];
      return {
        chiave: app == null ? 'admin' : 'app' + app,
        titolo: app == null ? 'menu.administration' : 'menu.application.' + app,
        sezioni,
        aperta: primo?.menuHeader.title ?? ''
      };
    }).filter(a => a.sezioni.length > 0);

    // "Assistenza" in fondo, come gli applicativi: dentro le sottosezioni "Assistenza" e "Configurazione"
    // (gruppi di Pages4Support, gia' filtrati per ruolo in getGroupedSupport)
    if (this.groupedSupportMenu.length > 0)
      this.sezioniApplicativi = [...this.sezioniApplicativi, {
        chiave: 'support',
        titolo: 'menu.support',
        sezioni: [{ gruppi: this.groupedSupportMenu, mostra: () => true }],
        aperta: this.groupedSupportMenu[0].menuHeader.title
      }];
  }

  // dopo il login si apre solo il primo applicativo disponibile (mai "Amministrazione"), con la sua prima sottosezione
  // aperta (SezioneApplicativo.aperta); senza applicativi (es. SuperUser o centro, anche impersonificati) si apre
  // "Amministrazione", altrimenti le loro voci (utenti, studi...) resterebbero nascoste in un menu tutto chiuso
  private primaSezioneAperta(): string {
    // "Assistenza" non si apre da sola
    const sezioni = this.sezioniApplicativi.filter(a => a.chiave !== 'support');
    return (sezioni.find(a => a.chiave !== 'admin') ?? sezioni[0])?.chiave ?? '';
  }

  // Sezioni aperte o chiuse a mano: i valori legati al menu le seguono, altrimenti dopo un'impersonificazione il
  // ricalcolo (spesso uguale al valore di prima, es. di nuovo "Presenze") non verrebbe riapplicato e resterebbe
  // aperta la sezione scelta a mano. L'evento ionChange risale dai gruppi interni: conta solo quello del gruppo stesso.
  public sezioneCambiata(ev: Event): void {
    if (ev.target === ev.currentTarget)
      this.selectedMenu = ((ev as CustomEvent).detail?.value as string) ?? '';
  }

  public sottosezioneCambiata(chiave: string, ev: Event): void {
    if (ev.target === ev.currentTarget)
      this.sottosezioniAperte = { ...this.sottosezioniAperte, [chiave]: ((ev as CustomEvent).detail?.value as string) ?? '' };
  }

  // somma dei contatori delle voci di una sezione di applicativo (es. richieste da approvare), visibile a sezione chiusa
  public appBadge(app: SezioneApplicativo): number {
    return app.sezioni.reduce((totale, s) => totale + s.gruppi.reduce((t, g) =>
      t + g.menuItem.filter(p => p.menuType === MenuType.MenuItem && s.mostra(p.component)).reduce((n, p) => n + this.menuBadge(p), 0), 0), 0);
  }

  // voce soggetta a un livello di licenza (MainMenuItem.minLicense) che l'azienda non ha: resta nel menu ma disattivata
  public voceDisattivata(p: MainMenuItem): boolean {
    return p.minLicense != null && p.application != null && !this.authService.hasLicenseAtLeast(p.application, p.minLicense);
  }

  // nome del livello di licenza richiesto dalla voce (es. "PRO"), mostrato accanto alla voce disattivata
  public licenzaRichiesta(p: MainMenuItem): string {
    return p.minLicense != null ? LivelloLicenza[p.minLicense].toUpperCase() : '';
  }

  // numero mostrato accanto alla voce di menu (ticket di assistenza in carico all'utente o badge della voce), 0 = nessuno
  public menuBadge(p: MainMenuItem): number {
    return p.component === 'TicketListPageComponent' ? this.ticketNotifierService.inCarico() : (p.badge?.() ?? 0);
  }

  public showPage4SuperUser(component: string): boolean {
    return true;
  }

  public showPage4Admin(component: string): boolean {

    switch (component) {
      case 'PowerAdminPageComponent':
        return this.authService.IsPowerAdmin;
        break;

      default:
        return this.authService.IsInGroupAdmin;
        break;
    }
  }

  public showPage4DealerAdmin(component: string): boolean {

    switch (component) {
      case 'DealerPowerAdminPageComponent':
        return this.authService.IsDealerPowerAdmin;
        break;

      default:
        return this.authService.IsInGroupDealerAdmin;
        break;
    }
  }
  
  public showPage4FinancialAdvisorAdmin(component: string): boolean {

    switch (component) {
      case 'FinancialAdvisorPowerAdminPageComponent':
        return this.authService.IsFinancialAdvisorPowerAdmin;
        break;

      default:
        return this.authService.IsInGroupFinancialAdvisorAdmin;
        break;
    }
  }

  public showPage4CompanyAdmin(component: string): boolean {

    switch (component) {
      case 'CompanyPowerAdminPageComponent':
        return this.authService.IsCompanyPowerAdmin;
        break;

      default:
        return this.authService.IsInGroupCompanyAdmin;
        break;
    }
  }

  public showPage4User(component: string): boolean {


    

    switch (component) {

      case 'DashboardPageComponent':
        return this.authService.Token != null;
        break;

      case 'LoginPageComponent':
        if (this.authService.Token != null)
          return false;
        else
          return true;
        break;

      case 'LogoutPageComponent':
        if(this.authService.Token==null)
          return false;
        else
          if (this.userNavigationService.UserCanGoBack)
            return false;
          else
            return true;
        break;

      case 'UserImpersonatePageComponent':
        if (this.userNavigationService.UserCanGoBack)
          return true;
        else
          return false;
        break;

      // licenze (listini; gestione per studio e azienda): a login fatto (i ruoli li filtra la voce di menu)
      case 'LicenseCatalogListPageComponent':
      case 'LicenseManagementPageComponent':
        return this.authService.Token != null;


      default:

        if (this.authService.IsUser)
          return true;
        else
          return false;
        break;
    }

    
  }

  public getMenuItem4User() {

    // ruoli dell'utente collegato: le voci della sezione Accesso con ruoli (es. 'User', listini delle licenze
    // per SuperUser e centro) restano solo a chi li ha; senza login nessun ruolo
    const currRole = this.authService.RoleNames;

    let retVal = this.appPages4User.filter(item =>
      (!item.roles || item.roles.some(role => currRole.includes(role))) &&
      this.mainMenuService.VoceVisibile(item, 'user', this.authService.RoleNames)
    );

    return retVal;

  }


  public getMenuItem4CompanyAdmin() {

    let currRole = [];

    if (this.authService.IsUser) {
      currRole.push("User");
    }
    if (this.authService.IsCompanyAdmin) {
      currRole.push("CompanyAdmin");
    }
    if (this.authService.IsCompanyPowerAdmin) {
      currRole.push("CompanyPowerAdmin");
    }

    let retVal =this.appPages4CompanyAdmin.filter(item =>
      (!item.roles || item.roles.some(role => currRole.includes(role))) &&
      this.mainMenuService.VoceVisibile(item, 'company', this.authService.RoleNames)
    );

    //non permette di impersonarsi a un user che ha anche ruoli admin
    if (this.authService.IsUser) {
      return retVal.filter(x => x.component != "UserCompanyListPageComponent");
    }

    return retVal;

    
  }

  public getMenuItems(menu: MainMenuItem[]): MainMenuItem[] {
    return menu
      .filter(x => x.menuType === MenuType.MenuItem)
      .sort((a, b) => a.zorder - b.zorder);
  }

  public getMenuAll(menu: MainMenuItem[]): MainMenuItem[] {
    return menu.slice().sort((a, b) => a.zorder - b.zorder);
  }

  public getMenuHeaderTitle(menuItems: MainMenuItem[]): string {
    const header = menuItems.find(item => item.menuType === MenuType.MenuHeader);
    return header ? header.title : '';
  }

  public getMenuNoteTitle(menuItems: MainMenuItem[], noteIndex: number = 0): string {
    const notes = menuItems.filter(item => item.menuType === MenuType.MenuNote).sort((a, b) => a.zorder - b.zorder);
    return notes[noteIndex] ? notes[noteIndex].title : '';
  }

  // intestazioni: da intestazioniDa (di solito tutto l'elenco del ruolo) se le voci sono solo una parte, es. quelle di un applicativo
  groupMenu(menuItems: MainMenuItem[], intestazioniDa: MainMenuItem[] = menuItems): GroupMainMenuItem[] {

    const groups: GroupMainMenuItem[] = [];
    const intestazioni = intestazioniDa.filter(item => item.menuType === MenuType.MenuHeader);
    menuItems = [...menuItems.filter(item => item.menuType !== MenuType.MenuHeader), ...intestazioni];

    // una voce il cui gruppo non ha intestazione (es. la sezione e' dichiarata da un applicativo non attivo
    // o non presente nel ramo) va nella prima sezione invece di sparire
    const gruppiConIntestazione = new Set(menuItems.filter(item => item.menuType === MenuType.MenuHeader).map(item => item.group));
    const primoGruppo = [...gruppiConIntestazione].sort((a, b) => a - b)[0];
    const gruppoDi = (item: MainMenuItem) => gruppiConIntestazione.has(item.group) ? item.group : primoGruppo;

    // 1. Estrai tutti i valori di 'group' in un array
    const allGroups = menuItems.map(item => item.group);
    // Risultato: [0, 0, 1, 1, 1]

    // 2. Usa un Set per ottenere solo i valori unici e riconvertilo in array
    //    sezioni nell'ordine di zorder della loro intestazione (a parita', nell'ordine in cui compaiono):
    //    ogni applicativo decide dove mettere le sue sezioni, anche rispetto a quelle di Infrastructure
    const zorderSezione = (group: number) => menuItems.find(item => item.group === group && item.menuType === MenuType.MenuHeader)?.zorder ?? 0;
    const uniqueGroups = [...new Set(allGroups)].sort((a, b) => zorderSezione(a) - zorderSezione(b));

    for (const groupNumber of uniqueGroups) {

      
      const header = menuItems.find(item => item.group === groupNumber && item.menuType === MenuType.MenuHeader);
      // voci nell'ordine di zorder: gli elenchi dei vari applicativi (es. Infrastructure e presenze) si mescolano nella stessa sezione
      const items = menuItems.filter(item => gruppoDi(item) === groupNumber && item.menuType === MenuType.MenuItem)
                             .sort((a, b) => a.zorder - b.zorder);

      // 5. Se esiste un header, crea il nuovo oggetto gruppo e aggiungilo all'array
      //    (una sezione rimasta senza voci, es. tutte nascoste dal CompanyPowerAdmin, non si mostra)
      if (header && items.length > 0) {
        const newGroup = new GroupMainMenuItem(header, items);
        groups.push(newGroup);
      }

    }


    return groups;

  }

  public getGroupedCompanyAdminMenu(): GroupMainMenuItem[] {
    const menuItems = this.getMenuItem4CompanyAdmin();
    return this.groupMenu(menuItems);
  }

  public getGroupedFinancialAdvisorAdmin(): GroupMainMenuItem[] {
    const menuItems = this.appPages4FinancialAdvisorAdmin;
    return this.groupMenu(menuItems);
  }

  public getGroupedDealerAdmin(): GroupMainMenuItem[] {
    const menuItems = this.appPages4DealerAdmin;
    return this.groupMenu(menuItems);
  }

  public getGroupedAdmin(): GroupMainMenuItem[] {
    const menuItems = this.appPages4Admin;
    return this.groupMenu(menuItems);
  }

  public getGroupedSuperUser(): GroupMainMenuItem[] {
    const menuItems = this.appPages4SuperUser;
    return this.groupMenu(menuItems);
  }

  public getGroupedUser(): GroupMainMenuItem[] {

    // ruoli dell'utente collegato (vedi getMenuItem4User)
    const currRole = this.authService.RoleNames;

    let retVal = this.appPages4User.filter(item =>
      (!item.roles || item.roles.some(role => currRole.includes(role))) &&
      this.mainMenuService.VoceVisibile(item, 'user', this.authService.RoleNames)
    );


    return this.groupMenu(retVal);
  }

  // sezione "Assistenza": voci senza ruoli per tutti gli utenti collegati, le altre per chi ha uno dei ruoli indicati
  // (il menu si ricalcola quando cambiano i ruoli: senza ruoli, cioe' senza login, la sezione non c'e')
  public getGroupedSupport(): GroupMainMenuItem[] {
    const ruoli = this.authService.RoleNames;
    if (ruoli.length === 0)
      return [];

    const retVal = this.mainMenuService.Pages4Support.filter(item => !item.roles || item.roles.some(role => ruoli.includes(role)));

    return this.groupMenu(retVal);
  }

}



// sezione espandibile del menu: "Amministrazione" (voci di Infrastructure) o un applicativo, con le sue sottosezioni
export interface SezioneApplicativo {
  chiave: string;          // valore dell'accordion: 'admin' o 'app<numero>'
  titolo: string;          // chiave di traduzione
  sezioni: SezioneMenu[];  // sottosezioni per ruolo
  aperta: string;          // sottosezione aperta all'inizio
}

// sezioni di un elenco di ruolo con la regola che decide quali voci mostrare
export interface SezioneMenu {
  gruppi: GroupMainMenuItem[];
  mostra: (component: string) => boolean;
}

export class GroupMainMenuItem {

  menuHeader: MainMenuItem;
  menuItem:   MainMenuItem[];
  

  constructor(menuHeader: MainMenuItem, menuItem: MainMenuItem[]) {
    this.menuHeader = menuHeader;
    this.menuItem = menuItem;
    
  }
}

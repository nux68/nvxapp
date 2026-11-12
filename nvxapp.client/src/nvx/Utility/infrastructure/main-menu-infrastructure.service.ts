import { Injectable } from '@angular/core';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { iMainMenuService, MainMenuItem, MenuType, WorkingMode } from './main-menu.service';
import { NotificationNotifierService, RUOLI_AZIENDA } from './notification-notifier.service';
import { RequestNotifierService } from './request-notifier.service';
import { APPLICATIVI_RICHIESTE } from './request-ui.model';

// ruoli che gestiscono (o leggono) documentazione e risposte AI: SuperUser, centro, studio, azienda (non il DEV)
const RUOLI_DOCUMENTAZIONE_AI = ['SuperUser', 'DealerPowerAdmin', 'DealerAdmin', 'FinancialAdvisorPowerAdmin', 'FinancialAdvisorAdmin',
                                 'CompanyPowerAdmin', 'CompanyAdmin'];

// licenze: chi vede lo stato delle licenze del proprio soggetto (sottoscrive solo il PowerAdmin) e chi vede i listini
const RUOLI_LICENZE_SOGGETTO = ['FinancialAdvisorPowerAdmin', 'FinancialAdvisorAdmin', 'CompanyPowerAdmin', 'CompanyAdmin'];
const RUOLI_LISTINI_LICENZE = ['SuperUser', 'DealerPowerAdmin', 'DealerAdmin'];

// riepiloghi periodici (INF-AI.3): amministratori dell'azienda e utenti dello studio
const RUOLI_RIEPILOGHI = ['CompanyPowerAdmin', 'CompanyAdmin', 'FinancialAdvisorPowerAdmin', 'FinancialAdvisorAdmin'];

// sezione dedicata di Pages4User (notifiche, scadenzario, calendario, prese visione): nel menu e' una sezione di primo
// livello come Accesso (app.component), non una sottosezione di Amministrazione
export const GRUPPO_SERVIZI = 9;

// applicativi soggetti a licenza: scadenzario, calendario e prese visione servono solo se l'azienda ne usa almeno uno
// (con il sistema licenze acceso AuthService conosce solo gli applicativi attivi e coperti da licenza)
const APPLICATIVI_MODULI = [ApplicationType.AttendanceTracking, ApplicationType.Crm, ApplicationType.DocumentManagement,
                            ApplicationType.HumanResources, ApplicationType.Compliance, ApplicationType.Assets];

@Injectable({
  providedIn: 'root'
})
export class MainMenuInfrastructureService implements iMainMenuService{

  // badge delle voci "Notifiche" (non lette) e "Prese visione" (da confermare)
  // badge delle voci delle richieste (da approvare / in attesa)
  constructor(private notificationNotifierService: NotificationNotifierService,
              private requestNotifierService: RequestNotifierService) { }

  public GetMenuHeaderMenu(MainMenuItem: MainMenuItem[], group: number = 0): MainMenuItem | undefined {

    let menu = MainMenuItem.find(x => x.menuType == MenuType.MenuHeader && x.group == group);

    if (menu == null)
        menu = MainMenuItem.find(x => x.menuType == MenuType.MenuHeader && x.group == 0);

    return menu;
  }



  public get Pages4SuperUser(): MainMenuItem[] {
    return [
      //{ menuType: MenuType.MenuHeader, zorder: 0, title: 'TITOLO MENU', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuHeader, zorder:   0, group: 0, title: 'menu.superUser', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem,   zorder: 100, group: 0, title: 'menu.superUserPage', component: "SuperUserPageComponent", url: '/superuser', icon: 'triangle' },
      { menuType: MenuType.MenuItem,   zorder: 200, group: 0, title: 'menu.dealers', component: "DealerListPageComponent", url: '/dealerlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem, zorder: 300, group: 0, title: 'menu.users', component: "UserListPageComponent", url: '/userlist', icon: 'people' },
      { menuType: MenuType.MenuItem, zorder: 400, group: 0, title: 'menu.companyMove', component: "CompanyMovePageComponent", url: '/companymove', icon: 'swap-horizontal' },
      { menuType: MenuType.MenuItem, zorder: 500, group: 0, title: 'menu.storageSettings', component: "StorageSettingsPageComponent", url: '/storagesettings', icon: 'server' },
      { menuType: MenuType.MenuItem, zorder: 600, group: 0, title: 'menu.companyStorage', component: "CompanyStorageListPageComponent", url: '/companystoragelist', icon: 'pie-chart' },
      // pagamenti delle licenze e ripartizione ai centri (ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md)
      { menuType: MenuType.MenuItem, zorder: 700, group: 0, title: 'menu.payments', component: "PaymentReportPageComponent", url: '/paymentreport', icon: 'wallet' }
    ];
  }

  public get Pages4Admin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 0, group: 0, title: 'menu.admin', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 100, group: 0, title: 'menu.powerAdmin', component: "PowerAdminPageComponent", url: '/poweradmin', icon: 'square' },
      { menuType: MenuType.MenuItem, zorder: 200, group: 0, title: 'menu.admin', component: "AdminPageComponent", url: '/admin', icon: 'square' },
      { menuType: MenuType.MenuItem, zorder: 300, group: 0, title: 'menu.users', component: "UserListPageComponent", url: '/userlist', icon: 'people' },
      { menuType: MenuType.MenuItem, zorder: 400, group: 0, title: 'menu.dealers', component: "DealerListPageComponent", url: '/dealerlist', icon: 'list-circle' }
    ];
  }

  public get Pages4DealerAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder: 0, group: 0, title: 'menu.dealer'            , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem, zorder: 100, group: 0, title: 'menu.dealerPowerAdmin', component: "DealerPowerAdminPageComponent", url: '/dealerpoweradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 200, group: 0, title: 'menu.dealerAdmin'      , component: "DealerAdminPageComponent", url: '/dealeradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem, zorder: 300, group: 0, title: 'menu.dealerUsers'     , component: "UserDealerListPageComponent", url: '/userdealerlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem, zorder: 400, group: 0, title: 'menu.financialAdvisors'             , component: "FinancialAdvisorListPageComponent", url: '/financialadvisorlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem, zorder: 500, group: 0, title: 'menu.companyStorage'                , component: "CompanyStorageListPageComponent", url: '/companystoragelist', icon: 'pie-chart' },
      // incassi delle licenze dei clienti del centro e quote liquidate dal SuperUser (ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md)
      { menuType: MenuType.MenuItem, zorder: 600, group: 0, title: 'menu.dealerPayments', roles: ['DealerPowerAdmin', 'DealerAdmin'], component: "PaymentDealerPageComponent", url: '/paymentdealer', icon: 'wallet' }
    ];
  }

  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder:   0, group: 0, title: 'menu.financialAdvisor'            , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem  , zorder: 100, group: 0, title: 'menu.financialAdvisorPowerAdmin', component: "FinancialAdvisorPowerAdminPageComponent", url: '/financialadvisorpoweradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem  , zorder: 200, group: 0, title: 'menu.financialAdvisorAdmin'      , component: "FinancialAdvisorAdminPageComponent", url: '/financialadvisoradmin', icon: 'ellipse' },
      { menuType: MenuType.MenuItem  , zorder: 300, group: 0, title: 'menu.financialAdvisorUsers'     , component: "UserFinancialAdvisorListPageComponent", url: '/userfinancialadvisorlist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem  , zorder: 400, group: 0, title: 'menu.companies'           , component: "CompanyListPageComponent", url: '/companylist', icon: 'list-circle' },
      { menuType: MenuType.MenuItem  , zorder: 500, group: 0, title: 'menu.companyStorage'     , component: "CompanyStorageListPageComponent", url: '/companystoragelist', icon: 'pie-chart' }
    ];
  }

  public get Pages4CompanyAdmin(): MainMenuItem[] {
    return [
      // "Generale" (gruppo 0): in Amministrazione non ha piu' voci (sparisce da sola), ma resta l'intestazione delle
      // voci di gruppo 0 dei rami applicativi (es. la sezione Generale delle presenze)
      { menuType: MenuType.MenuHeader, zorder:    0,  group: 0, title: 'menu.general'                                           , component: '', url: '', icon: '' },
      // Anagrafiche comuni (schema Common): visibili se l'azienda ha almeno un applicativo che usa i clienti
      { menuType: MenuType.MenuHeader, zorder: 5000,  group: 6, title: 'menu.registry'                                          , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem  , zorder: 5100,  group: 6, title: 'menu.customers'         , roles: ['CompanyPowerAdmin']  , component: "CustomerListPageComponent", url: '/customerlist', icon: 'business',
        anyApplication: [ApplicationType.AttendanceTracking, ApplicationType.Crm, ApplicationType.DocumentManagement] },
      { menuType: MenuType.MenuItem  , zorder: 5200,  group: 6, title: 'menu.users'             , roles: ['CompanyPowerAdmin']  , component: "UserCompanyListPageComponent", url: '/usercompanylist', icon: 'people' },
      // Parametri (gruppo 1): ultima sezione; l'intestazione e' anche qui (e prima di quella delle presenze, che resta per
      // i rami senza questa) perche' le preferenze ci sono senza presenze e anche per il CompanyAdmin
      { menuType: MenuType.MenuHeader, zorder: 900000, group: 1, title: 'menu.parameters', roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem  , zorder: 201000, group: 1, title: 'menu.preferences'       , roles: ['CompanyPowerAdmin']  , component: "CompanyPowerAdminPageComponent", url: '/companypoweradmin', icon: 'options' },
      { menuType: MenuType.MenuItem  , zorder: 201100, group: 1, title: 'menu.preferences'       , roles: ['CompanyAdmin']       , component: "CompanyAdminPageComponent", url: '/companyadmin', icon: 'options' },
      // dopo le voci delle presenze
      { menuType: MenuType.MenuItem  , zorder: 208000, group: 1, title: 'menu.menuRestriction'      , roles: ['CompanyPowerAdmin']  , component: "CompanyMenuRestrictionPageComponent", url: '/companymenurestriction', icon: 'options' }
    ];
  }

  public get Pages4User(): MainMenuItem[] {

    
    //const roles = ['SuperUser', 'PowerAdmin', 'Admin', 'DealerPowerAdmin', 'DealerAdmin', 'FinancialAdvisorPowerAdmin', 'FinancialAdvisorAdmin', 'CompanyPowerAdmin', 'CompanyAdmin', 'User'];

    return [
      { menuType: MenuType.MenuHeader, zorder: 0,   group: 0, title: 'menu.access', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem,   zorder: 100, group: 0, title: 'menu.impersonate', component: "UserImpersonatePageComponent", url: '/userimpersonate', icon: 'people-circle', nonConfigurabile: true },
      { menuType: MenuType.MenuItem,   zorder: 200, group: 0, title: 'menu.dashboard', component: "DashboardPageComponent", url: '/dashboard', icon: 'home', nonConfigurabile: true },
      { menuType: MenuType.MenuItem,   zorder: 300, group: 0, title: 'menu.login', component: "LoginPageComponent", url: '/login', icon: 'enter', nonConfigurabile: true },
      { menuType: MenuType.MenuItem,   zorder: 400, group: 0, title: 'menu.logout', component: "LogoutPageComponent", url: '/logout', icon: 'exit', nonConfigurabile: true },
      { menuType: MenuType.MenuItem,   zorder: 500, group: 0, title: 'menu.preferences', component: "UserPageComponent", url: '/user', icon: 'options', nonConfigurabile: true },
      // servizi comuni (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC) in una sezione dedicata del menu, subito dopo Accesso
      // (gruppo GRUPPO_SERVIZI, vedi app.component): notifiche per tutti gli utenti collegati;
      // scadenzario, calendario e prese visione per chi lavora in un'azienda (visibilita' e permessi li ricontrolla il server),
      // solo se l'azienda ha almeno un applicativo attivo e coperto da licenza: le scadenze e le prese visione arrivano dai moduli
      { menuType: MenuType.MenuHeader, zorder: 510, group: GRUPPO_SERVIZI, title: 'menu.services', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem,   zorder: 520, group: GRUPPO_SERVIZI, title: 'menu.notifications', component: "NotificationListPageComponent", url: '/notifications', icon: 'notifications', nonConfigurabile: true,
        badge: this.notificationNotifierService.nonLette },
      // richieste da approvare comuni (ANALISI_INFRASTRUCTURE_RICHIESTE.md): solo se l'azienda ha un applicativo che porta richieste
      { menuType: MenuType.MenuItem,   zorder: 524, group: GRUPPO_SERVIZI, title: 'menu.requestsAdmin', roles: ['CompanyPowerAdmin', 'CompanyAdmin'], component: "RequestListPageComponent", url: '/requestsadmin', icon: 'checkmark-done',
        badge: this.requestNotifierService.daApprovare, anyApplication: APPLICATIVI_RICHIESTE },
      // "Le mie richieste" non sta piu' in questa sezione: le proprie richieste si vedono nei moduli (Presenze: /attendancerequests,
      // Prenotazioni: /astbookings); /requests resta raggiungibile dai collegamenti (es. notifiche)
      { menuType: MenuType.MenuItem,   zorder: 530, group: GRUPPO_SERVIZI, title: 'menu.deadlines', roles: RUOLI_AZIENDA, component: "DeadlineListPageComponent", url: '/deadlines', icon: 'calendar-number',
        anyApplication: APPLICATIVI_MODULI },
      // calendario personale (INF-CAL.3): feed .ics da sottoscrivere in Google / Outlook / Apple
      { menuType: MenuType.MenuItem,   zorder: 535, group: GRUPPO_SERVIZI, title: 'menu.calendarFeed', roles: RUOLI_AZIENDA, component: "CalendarFeedPageComponent", url: '/calendarfeed', icon: 'calendar',
        anyApplication: APPLICATIVI_MODULI },
      { menuType: MenuType.MenuItem,   zorder: 540, group: GRUPPO_SERVIZI, title: 'menu.acknowledgements', roles: RUOLI_AZIENDA, component: "AcknowledgementListPageComponent", url: '/acknowledgements', icon: 'eye',
        badge: this.notificationNotifierService.daConfermare, anyApplication: APPLICATIVI_MODULI },
      // riepilogo AI periodico (INF-AI.3)
      { menuType: MenuType.MenuItem,   zorder: 545, group: 0, title: 'menu.digests', roles: RUOLI_RIEPILOGHI, component: "DigestListPageComponent", url: '/digests', icon: 'sparkles' },
      // licenze (ANALISI_INFRASTRUCTURE_LICENZE.md §10), dopo il login: listini per SuperUser e utenti del centro,
      // gestione delle licenze per studio e azienda
      { menuType: MenuType.MenuItem,   zorder: 600, group: 0, title: 'menu.licenseCatalogs', roles: RUOLI_LISTINI_LICENZE, component: "LicenseCatalogListPageComponent", url: '/licensecataloglist', icon: 'pricetag' },
      { menuType: MenuType.MenuItem,   zorder: 610, group: 0, title: 'menu.licenseManagement', roles: RUOLI_LICENZE_SOGGETTO, component: "LicenseManagementPageComponent", url: '/licensemanagement', icon: 'ribbon' }
    ];
  }

  /*
   Sezione "Assistenza": per tutti gli utenti collegati, in fondo al menu (dopo le sezioni dei ruoli).
   Ogni voce indica i ruoli che la vedono (nessun ruolo = tutti); permessi e livelli li ricontrolla il server.
  */
  public get Pages4Support(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder:   0, group: 0, title: 'menu.support', component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem,   zorder: 100, group: 0, title: 'menu.tickets', component: "TicketListPageComponent", url: '/ticketlist', icon: 'help-buoy' },
      { menuType: MenuType.MenuItem,   zorder: 200, group: 0, title: 'menu.ticketTypes', roles: ['SuperUser'], component: "TicketTypeListPageComponent", url: '/tickettypelist', icon: 'pricetags' },
      // manuali in PDF: quello delle funzioni base qui, quelli degli applicativi nei loro rami (Pages4Support), dopo questa voce
      { menuType: MenuType.MenuItem,   zorder: 900, group: 0, title: 'menu.manualInfrastructure', component: "UserManualPageComponent", url: '/usermanual/infrastructure', icon: 'book', nonConfigurabile: true },
      // manuale dei comandi vocali dell'assistente: dopo i manuali degli applicativi (910-930); il server lo compone dai
      // comandi presenti nel ramo, filtrati su ruoli e applicativi attivi (VoiceCommandsUserManualProvider)
      { menuType: MenuType.MenuItem,   zorder: 940, group: 0, title: 'menu.manualVoiceCommands', component: "UserManualPageComponent", url: '/usermanual/voicecommands', icon: 'mic', nonConfigurabile: true },
      // "Configurazione" dell'assistenza (gruppo 1, dopo la sezione Assistenza e i manuali), come quelle di CRM e documentale
      { menuType: MenuType.MenuHeader, zorder: 950, group: 1, title: 'menu.supportSettings', roles: RUOLI_DOCUMENTAZIONE_AI, component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem,   zorder: 960, group: 1, title: 'menu.aiDocuments', roles: RUOLI_DOCUMENTAZIONE_AI, component: "AiDocumentListPageComponent", url: '/aidocumentlist', icon: 'library' },
      { menuType: MenuType.MenuItem,   zorder: 970, group: 1, title: 'menu.ticketTypeAiSettings', roles: RUOLI_DOCUMENTAZIONE_AI, component: "TicketTypeAiSettingPageComponent", url: '/tickettypeaisetting', icon: 'sparkles' }
    ];
  }

  public get GetWorkingMode(): WorkingMode {    return WorkingMode.Infrastructure;  }

}

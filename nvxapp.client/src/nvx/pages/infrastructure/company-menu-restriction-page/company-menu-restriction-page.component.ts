import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { MainMenuService, VoceMenuConfigurabile } from '../../../Utility/infrastructure/main-menu.service';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { CompanyMenuRestrictionService } from '../../../ClientServer-Service/Infrastructure/CompanyMenuRestriction/company-menu-restriction.service';
import { CompanyMenuRestrictionListInModel, CompanyMenuRestrictionPutInModel } from '../../../ClientServer-Service/Infrastructure/CompanyMenuRestriction/Models/company-menu-restriction-model';
import { MenuRestrictionModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';


/*
 Pagina "Menu utenti" (CompanyPowerAdmin): quali voci di menu vedono gli utenti Admin e User dell'azienda.
 Le voci arrivano da MainMenuService (tutti gli applicativi attivi), quindi quelle aggiunte da un ramo
 applicativo compaiono da sole. Per ogni voce c'e' un interruttore solo per i ruoli a cui il codice la
 concede: si puo' solo togliere. Si salvano le esclusioni (voce + ruolo); le voci nuove restano visibili.
*/
@Component({
  selector: 'app-company-menu-restriction-page',
  templateUrl: './company-menu-restriction-page.component.html',
  styleUrls: ['./company-menu-restriction-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompanyMenuRestrictionPageComponent {

  /** chiave di traduzione del titolo (tradotto nel template) */
  public readonly title = 'pages.companyMenuRestriction.title';

  /** ruoli configurabili, con la chiave di traduzione dell'etichetta della colonna */
  public readonly ruoli = [
    { role: 'CompanyAdmin', label: 'pages.companyMenuRestriction.roleAdmin' },
    { role: 'User', label: 'pages.companyMenuRestriction.roleUser' },
  ];

  public sezioni: { titolo: string, voci: VoceMenuConfigurabile[] }[] = [];
  public caricamento = false;

  /** esclusioni correnti (chiave "url|ruolo"), anche quelle di voci oggi non presenti: si conservano */
  private escluse = new Set<string>();
  private esclusePerAltreVoci: MenuRestrictionModel[] = [];

  public buttonbar: ButtonItem[] = [];

  constructor(private mainMenuService: MainMenuService,
              private authService: AuthService,
              private navCtrl: NavController,
              private companyMenuRestrictionService: CompanyMenuRestrictionService,
              userInterfaceService: UserInterfaceService) {

    const btnConferma = userInterfaceService.Btn_Conferma;
    btnConferma.event = () => this.salva();
    const btnAnnulla = userInterfaceService.Btn_Annulla;
    btnAnnulla.event = () => this.navCtrl.back();
    this.buttonbar = [btnConferma, btnAnnulla];
  }

  ionViewWillEnter() {
    this.carica();
  }

  private chiave(url: string, role: string): string {
    return AuthService.normalizeMenuUrl(url) + '|' + role;
  }

  public visibile(voce: VoceMenuConfigurabile, role: string): boolean {
    return !this.escluse.has(this.chiave(voce.url, role));
  }

  public cambia(voce: VoceMenuConfigurabile, role: string, visibile: boolean): void {
    const k = this.chiave(voce.url, role);
    if (visibile)
      this.escluse.delete(k);
    else
      this.escluse.add(k);
  }

  /** voci raggruppate per sezione, nell'ordine del menu */
  private costruisciSezioni(): void {
    const voci = this.mainMenuService.VociMenuConfigurabili.sort((a, b) => a.zorder - b.zorder);
    const sezioni: { titolo: string, voci: VoceMenuConfigurabile[] }[] = [];
    for (const v of voci) {
      let s = sezioni.find(x => x.titolo === v.sezione);
      if (!s) {
        s = { titolo: v.sezione, voci: [] };
        sezioni.push(s);
      }
      s.voci.push(v);
    }
    this.sezioni = sezioni;
  }

  public carica(): void {
    this.costruisciSezioni();
    this.caricamento = true;
    const request = new GenericRequest<CompanyMenuRestrictionListInModel>(CompanyMenuRestrictionListInModel);
    this.companyMenuRestrictionService.CompanyMenuRestrictionList(request).subscribe({
      next: res => {
        if (res.success && res.data)
          this.applicaEsclusioni(res.data.companyMenuRestriction);
        this.caricamento = false;
      },
      error: () => { this.caricamento = false; }
    });
  }

  private applicaEsclusioni(esclusioni: MenuRestrictionModel[]): void {
    const configurabili = new Set(this.sezioni.flatMap(s => s.voci.flatMap(v => v.ruoli.map(r => this.chiave(v.url, r)))));
    this.escluse = new Set(esclusioni.map(x => this.chiave(x.url, x.role)).filter(k => configurabili.has(k)));
    // esclusioni di voci oggi assenti (es. applicativo disattivato): restano salvate cosi' come sono
    this.esclusePerAltreVoci = esclusioni.filter(x => !configurabili.has(this.chiave(x.url, x.role)));
  }

  public salva(): void {
    const request = new GenericRequest<CompanyMenuRestrictionPutInModel>(CompanyMenuRestrictionPutInModel);
    request.data.companyMenuRestriction = [
      ...[...this.escluse].map(k => { const [url, role] = k.split('|'); return { url, role }; }),
      ...this.esclusePerAltreVoci,
    ];
    // salvato si torna alla pagina precedente; con un errore si resta qui (le scelte non vanno perse)
    this.companyMenuRestrictionService.CompanyMenuRestrictionPut(request).subscribe(res => {
      if (res.success && res.data) {
        // la sessione corrente vede subito il nuovo menu (es. chi configura ha anche il ruolo User)
        this.authService.setMenuRestrictions(res.data.companyMenuRestriction);
        this.navCtrl.back();
      }
    });
  }
}

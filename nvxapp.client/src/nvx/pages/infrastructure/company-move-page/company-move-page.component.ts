import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ToastController } from '@ionic/angular';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { CompanyMoveService } from '../../../ClientServer-Service/Infrastructure/CompanyMove/company-move.service';
import { CompanyMoveCompanyModel, CompanyMoveDealerModel, CompanyMoveFinancialAdvisorModel, CompanyMovePutInModel, CompanyMoveTreeInModel } from '../../../ClientServer-Service/Infrastructure/CompanyMove/Models/company-move-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';


/** scelta centro -> studio (-> azienda) con i figli filtrati sul padre */
class Selezione {
  idDealer: number | null = null;
  idFinancialAdvisor: number | null = null;
  idCompany: number | null = null;
}

/*
 Pagina "Sposta azienda" (SuperUser): cambia lo studio (FinancialAdvisor) a cui appartiene un'azienda.
 Riquadro "Azienda": centro -> studio -> azienda per trovarla; scegliendo un livello si filtrano quelli sotto,
 scegliendo direttamente un figlio si impostano i padri. Riquadro "Destinazione": centro -> studio.
 La gerarchia completa arriva con una sola chiamata; i filtri sono fatti qui.
 Dopo lo spostamento si resta sulla pagina (con l'azienda gia' nella nuova posizione) per poterne spostare altre.
*/
@Component({
  selector: 'app-company-move-page',
  templateUrl: './company-move-page.component.html',
  styleUrls: ['./company-move-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompanyMovePageComponent {

  public readonly title = 'Sposta azienda';

  public dealers: CompanyMoveDealerModel[] = [];
  public financialAdvisors: CompanyMoveFinancialAdvisorModel[] = [];
  public companies: CompanyMoveCompanyModel[] = [];
  public caricamento = false;

  public readonly origine = new Selezione();
  public readonly destinazione = new Selezione();

  // elenchi filtrati per i combo (ricalcolati solo quando cambia una scelta)
  public origineStudi: CompanyMoveFinancialAdvisorModel[] = [];
  public origineAziende: CompanyMoveCompanyModel[] = [];
  public destinazioneStudi: CompanyMoveFinancialAdvisorModel[] = [];

  public buttonbar: ButtonItem[] = [];
  private btnConferma: ButtonItem;

  constructor(private navCtrl: NavController,
              private toastCtrl: ToastController,
              private companyMoveService: CompanyMoveService,
              private collectionDialogService: CollectionDialogService,
              userInterfaceService: UserInterfaceService) {

    this.btnConferma = userInterfaceService.Btn_Conferma;
    this.btnConferma.event = () => this.conferma();
    const btnAnnulla = userInterfaceService.Btn_Annulla;
    btnAnnulla.event = () => this.navCtrl.back();
    this.buttonbar = [this.btnConferma, btnAnnulla];
    this.aggiorna();
  }

  ionViewWillEnter() {
    this.carica();
  }

  public carica(): void {
    this.caricamento = true;
    this.companyMoveService.CompanyMoveTree(new GenericRequest<CompanyMoveTreeInModel>(CompanyMoveTreeInModel)).subscribe({
      next: res => {
        if (res.success && res.data) {
          this.dealers = res.data.dealers;
          this.financialAdvisors = res.data.financialAdvisors;
          this.companies = res.data.companies;
        }
        this.caricamento = false;
        this.aggiorna();
      },
      error: () => { this.caricamento = false; }
    });
  }

  // ---------- azienda da spostare ----------

  public onOrigineDealer(id: number | null): void {
    this.origine.idDealer = id;
    if (this.studio(this.origine.idFinancialAdvisor)?.idDealer !== id && id != null)
      this.origine.idFinancialAdvisor = null;
    if (id != null && this.dealerDiAzienda(this.origine.idCompany) !== id)
      this.origine.idCompany = null;
    this.aggiorna();
  }

  public onOrigineStudio(id: number | null): void {
    this.origine.idFinancialAdvisor = id;
    if (id != null) {
      this.origine.idDealer = this.studio(id)?.idDealer ?? this.origine.idDealer;
      if (this.azienda(this.origine.idCompany)?.idFinancialAdvisor !== id)
        this.origine.idCompany = null;
    }
    this.aggiorna();
  }

  public onOrigineAzienda(id: number | null): void {
    this.origine.idCompany = id;
    const company = this.azienda(id);
    if (company) {
      // la posizione attuale dell'azienda
      this.origine.idFinancialAdvisor = company.idFinancialAdvisor;
      this.origine.idDealer = this.studio(company.idFinancialAdvisor)?.idDealer ?? null;
    }
    this.aggiorna();
  }

  // ---------- destinazione ----------

  public onDestinazioneDealer(id: number | null): void {
    this.destinazione.idDealer = id;
    if (id != null && this.studio(this.destinazione.idFinancialAdvisor)?.idDealer !== id)
      this.destinazione.idFinancialAdvisor = null;
    this.aggiorna();
  }

  public onDestinazioneStudio(id: number | null): void {
    this.destinazione.idFinancialAdvisor = id;
    if (id != null)
      this.destinazione.idDealer = this.studio(id)?.idDealer ?? this.destinazione.idDealer;
    this.aggiorna();
  }

  // ---------- testi e controlli ----------

  public get aziendaScelta(): CompanyMoveCompanyModel | undefined {
    return this.azienda(this.origine.idCompany);
  }

  /** la destinazione e' lo studio in cui l'azienda e' gia' */
  public get stessoStudio(): boolean {
    const company = this.aziendaScelta;
    return !!company && company.idFinancialAdvisor === this.destinazione.idFinancialAdvisor;
  }

  public percorso(idFinancialAdvisor: number | null | undefined): string {
    const fa = this.studio(idFinancialAdvisor);
    if (!fa) return '';
    const dealer = this.dealers.find(d => d.idDealer === fa.idDealer);
    return `${dealer?.descrizione ?? '?'} › ${fa.descrizione}`;
  }

  private aggiorna(): void {
    this.origineStudi = this.financialAdvisors.filter(f => this.origine.idDealer == null || f.idDealer === this.origine.idDealer);
    this.origineAziende = this.companies.filter(c =>
      this.origine.idFinancialAdvisor != null ? c.idFinancialAdvisor === this.origine.idFinancialAdvisor
        : this.origine.idDealer != null ? this.studio(c.idFinancialAdvisor)?.idDealer === this.origine.idDealer
          : true);
    this.destinazioneStudi = this.financialAdvisors.filter(f => this.destinazione.idDealer == null || f.idDealer === this.destinazione.idDealer);

    this.btnConferma.disabled = !this.aziendaScelta || this.destinazione.idFinancialAdvisor == null || this.stessoStudio;
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

  // ---------- salvataggio ----------

  public async conferma(): Promise<void> {
    const company = this.aziendaScelta;
    const idDest = this.destinazione.idFinancialAdvisor;
    if (!company || idDest == null || this.stessoStudio)
      return;

    const ok = await this.collectionDialogService.ConfirmCancelDialog(
      `Spostare l'azienda "${company.descrizione}" da ${this.percorso(company.idFinancialAdvisor)} a ${this.percorso(idDest)}? ` +
      `Gli utenti del vecchio studio non vedranno piu' l'azienda.`);
    if (!ok)
      return;

    const request = new GenericRequest<CompanyMovePutInModel>(CompanyMovePutInModel);
    request.data.idCompany = company.idCompany;
    request.data.idFinancialAdvisor = idDest;
    // con un errore il messaggio del server compare da solo (http-background-error-dialog) e le scelte restano
    this.companyMoveService.CompanyMovePut(request).subscribe(res => {
      if (res.success && res.data?.company) {
        company.idFinancialAdvisor = res.data.company.idFinancialAdvisor;
        this.onOrigineAzienda(company.idCompany);   // l'azienda ora e' nella nuova posizione
        this.destinazione.idDealer = null;
        this.destinazione.idFinancialAdvisor = null;
        this.aggiorna();
        this.toastCtrl.create({ message: `Azienda "${company.descrizione}" spostata.`, duration: 3000, position: 'bottom' })
          .then(t => t.present());
      }
    });
  }
}

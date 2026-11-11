import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { NavController, ToastController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DigestGenerateInModel, DigestListInModel, DigestModel, DigestType } from '../../../ClientServer-Service/Infrastructure/Digest/Models/digest-model';
import { DigestService } from '../../../ClientServer-Service/Infrastructure/Digest/digest.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/*
 Riepiloghi periodici (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-AI.3): quelli dell'azienda generati per l'utente
 (amministratori) e quelli degli studi in cui lavora. Il dettaglio mostra il testo e i fatti da cui e' stato scritto.
 "Genera ora" (CompanyPowerAdmin nell'azienda corrente, FinancialAdvisorPowerAdmin nello studio corrente): stessa
 generazione di quella settimanale, con la notifica; la visibilita' e i permessi li ricontrolla il server.
*/
@Component({
  selector: 'app-digest-list-page',
  templateUrl: './digest-list-page.component.html',
  styleUrls: ['./digest-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DigestListPageComponent {

  public title: string;
  public searchText!: string;
  public digestList: DigestModel[] | null = null;
  public buttonbar: ButtonItem[] = [];
  /** generazione in corso (l'AI puo' richiedere qualche secondo) */
  public generando = false;

  public readonly columns: NvxListColumn<DigestModel>[];
  public readonly actions: NvxListAction<DigestModel>[];
  public readonly iconOf = (item: DigestModel) => item.tipo === DigestType.Studio ? 'briefcase-outline' : 'business-outline';
  public readonly sortByData = (a: DigestModel, b: DigestModel) => b.id - a.id;

  constructor(private navCtrl: NavController,
              private toastCtrl: ToastController,
              private userInterfaceService: UserInterfaceService,
              private digestService: DigestService,
              private languageService: LanguageService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.digestList.title');

    this.columns = [
      { key: 'al', title: this.transloco.translate('pages.digestList.columnPeriod'), width: '200px', mobile: 'note',
        value: d => this.formatData(d.dal) + ' - ' + this.formatData(d.al),
        sortValue: d => d.al ? new Date(d.al) : null },
      { key: 'soggetto', title: this.transloco.translate('pages.digestList.columnSubject'), mobile: 'title',
        value: d => (d.soggetto ?? '') + ' (' + this.transloco.translate('pages.digestList.type.' + d.tipo) + ')' },
      { key: 'testo', title: this.transloco.translate('pages.digestList.columnText'), mobile: 'subtitle' },
      { key: 'generatoConAI', title: this.transloco.translate('pages.digestList.columnSource'), width: '140px',
        value: d => this.transloco.translate(d.generatoConAI ? 'pages.digestList.sourceAi' : 'pages.digestList.sourcePlain') },
    ];

    const btnDettaglio = userInterfaceService.Btn_Dettaglio;
    btnDettaglio.event = (item: DigestModel) => this.navCtrl.navigateForward('/digestdetail', { state: { id: item.id } });
    this.actions = [{ button: btnDettaglio }];
  }

  ionViewWillEnter() {
    this.load();
  }

  load() {
    const request = new GenericRequest<DigestListInModel>(DigestListInModel);
    this.digestService.DigestList(request).subscribe(res => {
      this.digestList = res.success && res.data ? res.data.digest : [];
      this.buttonbar = res.success && res.data && res.data.puoGenerare ? [this.btnGeneraOra()] : [];
    });
  }

  private btnGeneraOra(): ButtonItem {
    const btn = this.userInterfaceService.Btn_GeneraOra;
    btn.disabled = this.generando;
    btn.event = () => this.genera();
    return btn;
  }

  private genera() {
    if (this.generando)
      return;
    this.generando = true;
    this.buttonbar = [this.btnGeneraOra()];
    const request = new GenericRequest<DigestGenerateInModel>(DigestGenerateInModel);
    this.digestService.DigestGenerate(request).subscribe({
      next: res => {
        this.generando = false;
        if (res.success && res.data) {
          const testo = res.data.messages?.map(m => m.text).filter(t => !!t).join(' ');
          if (testo)
            this.toast(testo, res.data.id.length > 0 ? 'success' : 'warning');
        }
        this.load();
      },
      error: () => {
        this.generando = false;
        this.load();
      }
    });
  }

  private async toast(message: string, color: string) {
    const toast = await this.toastCtrl.create({ message, duration: 3000, position: 'top', color });
    await toast.present();
  }

  private formatData(value: string | null): string {
    return value ? formatDate(value, 'shortDate', this.languageService.locale) : '';
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}

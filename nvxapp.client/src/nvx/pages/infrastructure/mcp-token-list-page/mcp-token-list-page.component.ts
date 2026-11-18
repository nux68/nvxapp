import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { McpStatusModel, McpTokenListInModel, McpTokenModel, McpTokenRevokeInModel } from '../../../ClientServer-Service/Infrastructure/Mcp/Models/mcp-access-model';
import { McpAccessService } from '../../../ClientServer-Service/Infrastructure/Mcp/mcp-access.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/** filtro della lista: solo i token attivi o anche quelli revocati e scaduti */
type McpTokenFilter = 'active' | 'all';

/*
 Accesso AI (ANALISI_INFRASTRUCTURE_MCP.md, sezione 7): token personali con cui un client AI (Claude Code, Claude Desktop...)
 si collega all'endpoint MCP e lavora per conto dell'utente.
   - l'elenco e' quello dei token creati da chi ha fatto il login; ogni token lavora come l'utente indicato (lo stesso,
     oppure quello impersonato quando il token e' stato creato) sulla sua azienda
   - il valore del token si vede solo alla creazione (pagina /aiaccesstoken); qui si riconosce dal nome e dal prefisso
   - "Revoca" lo spegne subito e non si puo' annullare
 In alto lo stato dell'accesso: endpoint presente nell'installazione e accesso AI attivo per l'azienda
 (lo attiva il CompanyPowerAdmin dalla pagina /aiaccesssetting).
*/
@Component({
  selector: 'app-mcp-token-list-page',
  templateUrl: './mcp-token-list-page.component.html',
  styleUrls: ['./mcp-token-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class McpTokenListPageComponent {

  public title: string;
  public searchText!: string;
  public tokenList: McpTokenModel[] | null = null;
  public status: McpStatusModel | null = null;
  public filtro: McpTokenFilter = 'active';
  public readonly filtri: McpTokenFilter[] = ['active', 'all'];

  public buttonbar: ButtonItem[] = [];
  public readonly columns: NvxListColumn<McpTokenModel>[];
  public readonly actions: NvxListAction<McpTokenModel>[];
  public readonly highlight = (item: McpTokenModel) => item.state === 'active';
  public readonly iconOf = (item: McpTokenModel) => item.state === 'active' ? 'key-outline' : 'close-circle-outline';
  public readonly sortByCreazione = (a: McpTokenModel, b: McpTokenModel) => (b.createdAt ?? '').localeCompare(a.createdAt ?? '');

  private readonly btnNuovo: ButtonItem;
  private readonly btnRegistro: ButtonItem;
  private readonly btnImpostazione: ButtonItem;

  constructor(private navCtrl: NavController,
              private userInterfaceService: UserInterfaceService,
              private mcpAccessService: McpAccessService,
              private collectionDialogService: CollectionDialogService,
              private languageService: LanguageService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.mcpTokenList.title');

    this.columns = [
      { key: 'name', title: this.transloco.translate('pages.mcpTokenList.columnName'), mobile: 'title' },
      { key: 'tokenPrefix', title: this.transloco.translate('pages.mcpTokenList.columnPrefix'), width: '120px', mobile: 'hidden',
        value: t => t.tokenPrefix + '…' },
      { key: 'userName', title: this.transloco.translate('pages.mcpTokenList.columnUser'),
        value: t => this.descrizioneUtente(t) },
      { key: 'expiresAt', title: this.transloco.translate('pages.mcpTokenList.columnExpires'), width: '120px',
        value: t => this.formatData(t.expiresAt, 'shortDate'),
        sortValue: t => t.expiresAt ? new Date(t.expiresAt) : null },
      { key: 'lastUsedAt', title: this.transloco.translate('pages.mcpTokenList.columnLastUsed'), width: '150px', mobile: 'note',
        value: t => t.lastUsedAt ? this.formatData(t.lastUsedAt, 'short') : this.transloco.translate('pages.mcpTokenList.never'),
        sortValue: t => t.lastUsedAt ? new Date(t.lastUsedAt) : null },
      { key: 'state', title: this.transloco.translate('pages.mcpTokenList.columnState'), width: '110px',
        value: t => this.transloco.translate('pages.mcpTokenList.state.' + (t.state || 'active')) },
    ];

    const btnRevoca = userInterfaceService.Btn_Revoca;
    btnRevoca.event = (item: McpTokenModel) => this.revoca(item);
    this.actions = [
      { button: btnRevoca, visible: t => t.state === 'active' },
    ];

    this.btnNuovo = userInterfaceService.Btn_Aggiungi;
    this.btnNuovo.text = this.transloco.translate('pages.mcpTokenList.newToken');
    this.btnNuovo.event = () => this.navCtrl.navigateForward('/aiaccesstoken');

    this.btnRegistro = userInterfaceService.Btn_Elenco;
    this.btnRegistro.text = this.transloco.translate('pages.mcpTokenList.callLog');
    this.btnRegistro.event = () => this.navCtrl.navigateForward('/aiaccesslog');

    this.btnImpostazione = userInterfaceService.Btn_Preferenze;
    this.btnImpostazione.text = this.transloco.translate('pages.mcpTokenList.companySetting');
    this.btnImpostazione.event = () => this.navCtrl.navigateForward('/aiaccesssetting');
  }

  ionViewWillEnter() {
    this.load();
  }

  load() {
    const request = new GenericRequest<McpTokenListInModel>(McpTokenListInModel);
    request.data.showAll = this.filtro === 'all';
    this.mcpAccessService.McpTokenList(request).subscribe(res => {
      this.tokenList = res.success && res.data ? res.data.mcpToken : [];
      this.status = res.success && res.data ? res.data.status : null;
      this.aggiornaBottoni();
    });
  }

  // senza l'endpoint nell'installazione un token nuovo non servirebbe; l'impostazione dell'azienda solo a chi puo' cambiarla
  private aggiornaBottoni() {
    const bottoni: ButtonItem[] = [];
    if (this.status?.serverEnabled)
      bottoni.push(this.btnNuovo);
    bottoni.push(this.btnRegistro);
    if (this.status?.canManageCompany)
      bottoni.push(this.btnImpostazione);
    this.buttonbar = bottoni;
  }

  filtroChanged(value: any) {
    this.filtro = (value ?? 'active') as McpTokenFilter;
    this.tokenList = null;
    this.load();
  }

  private async revoca(item: McpTokenModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('pages.mcpTokenList.confirmRevoke', { name: item.name })))
      return;
    const request = new GenericRequest<McpTokenRevokeInModel>(McpTokenRevokeInModel);
    request.data.id = item.id;
    this.mcpAccessService.McpTokenRevoke(request).subscribe(res => {
      if (res.success)
        this.load();
    });
  }

  // utente con cui lavora il token e azienda; segnala i token creati impersonando
  private descrizioneUtente(t: McpTokenModel): string {
    let testo = t.company ? `${t.userName} (${t.company})` : t.userName;
    if (t.impersonated)
      testo += ' · ' + this.transloco.translate('pages.mcpTokenList.impersonated');
    return testo;
  }

  /** stato dell'accesso da mostrare in alto: chiave di traduzione e colore della nota */
  public get avviso(): { key: string; color: string } | null {
    if (!this.status)
      return null;
    if (!this.status.serverEnabled)
      return { key: 'pages.mcpTokenList.serverDisabled', color: 'warning' };
    if (!this.status.hasCompany)
      return { key: 'pages.mcpTokenList.noCompany', color: 'medium' };
    if (!this.status.companyEnabled)
      return { key: this.status.canManageCompany ? 'pages.mcpTokenList.companyDisabledAdmin' : 'pages.mcpTokenList.companyDisabled', color: 'warning' };
    return { key: 'pages.mcpTokenList.companyEnabled.' + this.status.companyMaxLevel, color: 'success' };
  }

  private formatData(value: string | null, format: string): string {
    return value ? formatDate(value, format, this.languageService.locale) : '';
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}

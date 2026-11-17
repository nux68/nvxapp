import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { McpCallLogListInModel, McpCallLogModel } from '../../../ClientServer-Service/Infrastructure/Mcp/Models/mcp-access-model';
import { McpAccessService } from '../../../ClientServer-Service/Infrastructure/Mcp/mcp-access.service';
import { NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { LanguageService } from '../../../Utility/infrastructure/language.service';

/*
 Registro delle chiamate dei client AI (ANALISI_INFRASTRUCTURE_MCP.md, sezione 8.4), in sola lettura.
   - amministratori dell'azienda (CompanyPowerAdmin, CompanyAdmin): tutte le chiamate fatte sull'azienda
   - gli altri: solo le chiamate fatte con i propri token
 Ogni riga riporta chi ha creato il token e l'utente con cui ha lavorato il client AI (diversi quando il token
 e' stato creato impersonando), lo strumento, i parametri e l'esito.
*/
@Component({
  selector: 'app-mcp-call-log-page',
  templateUrl: './mcp-call-log-page.component.html',
  styleUrls: ['./mcp-call-log-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class McpCallLogPageComponent {

  public title: string;
  public searchText!: string;
  public logList: McpCallLogModel[] | null = null;
  public wholeCompany = false;
  public truncated = false;
  public giorni = 7;
  public readonly periodi: number[] = [1, 7, 30, 90];

  public readonly columns: NvxListColumn<McpCallLogModel>[];
  public readonly highlight = (item: McpCallLogModel) => !item.success;
  public readonly iconOf = (item: McpCallLogModel) => item.success ? 'checkmark-circle-outline' : 'alert-circle-outline';
  public readonly sortByData = (a: McpCallLogModel, b: McpCallLogModel) => (b.data ?? '').localeCompare(a.data ?? '') || b.id - a.id;

  constructor(private mcpAccessService: McpAccessService,
              private languageService: LanguageService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.mcpCallLog.title');

    this.columns = [
      { key: 'data', title: this.transloco.translate('pages.mcpCallLog.columnDate'), width: '150px', mobile: 'note',
        value: x => x.data ? formatDate(x.data, 'short', this.languageService.locale) : '',
        sortValue: x => x.data ? new Date(x.data) : null },
      { key: 'tool', title: this.transloco.translate('pages.mcpCallLog.columnTool'), mobile: 'title' },
      { key: 'userName', title: this.transloco.translate('pages.mcpCallLog.columnUser'),
        value: x => this.descrizioneUtente(x) },
      { key: 'tokenName', title: this.transloco.translate('pages.mcpCallLog.columnToken'), mobile: 'hidden' },
      { key: 'arguments', title: this.transloco.translate('pages.mcpCallLog.columnArguments'), mobile: 'hidden' },
      { key: 'message', title: this.transloco.translate('pages.mcpCallLog.columnResult'),
        value: x => this.esito(x) },
    ];
  }

  ionViewWillEnter() {
    this.load();
  }

  load() {
    const request = new GenericRequest<McpCallLogListInModel>(McpCallLogListInModel);
    request.data.days = this.giorni;
    this.mcpAccessService.McpCallLogList(request).subscribe(res => {
      this.logList = res.success && res.data ? res.data.mcpCallLog : [];
      this.wholeCompany = res.success && !!res.data?.wholeCompany;
      this.truncated = res.success && !!res.data?.truncated;
    });
  }

  periodoChanged(value: any) {
    this.giorni = Number(value) || 7;
    this.logList = null;
    this.load();
  }

  // utente con cui ha lavorato il client AI; se diverso, anche chi ha creato il token
  private descrizioneUtente(x: McpCallLogModel): string {
    if (x.userNameLogin && x.userNameLogin !== x.userName)
      return this.transloco.translate('pages.mcpCallLog.userVia', { user: x.userName, login: x.userNameLogin });
    return x.userName;
  }

  // esito: il messaggio dello strumento su una riga, preceduto da "Errore" se la chiamata non e' riuscita
  private esito(x: McpCallLogModel): string {
    const testo = (x.message ?? '').replace(/\s+/g, ' ').trim();
    const breve = testo.length > 160 ? testo.substring(0, 160) + '…' : testo;
    return x.success ? breve : this.transloco.translate('pages.mcpCallLog.failed', { message: breve });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}

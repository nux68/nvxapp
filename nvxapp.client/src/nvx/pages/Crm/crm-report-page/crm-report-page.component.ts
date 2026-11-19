import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ModalController, AlertController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, Subscription, catchError, from, map, of, switchMap } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CrmService } from '../../../ClientServer-Service/Crm/crm.service';
import * as M from '../../../ClientServer-Service/Crm/Models/crm-model';
import { CrmSetupService, crmAdesso, crmData, crmDataOra, crmImporto, crmPerInputData, crmPerInputDataOra } from '../../../Utility/Crm/crm-setup.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ApplicationType } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { DataChangedNotifierService } from '../../../Utility/infrastructure/data-changed-notifier.service';

@Component({
  selector: 'app-crm-report-page',
  templateUrl: './crm-report-page.component.html',
  styleUrls: ['./crm-report-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Report del CRM: pipeline per fase e responsabile, vinte/perse nel periodo, conversione dei lead, attivita' per utente
export class CrmReportPageComponent {

  public title: string;
  public dal: string;
  public al: string;
  public report: M.CrmReportOutModel | null = null;
  // dati modificati da un comando eseguito fuori dalla pagina (assistente AI via MCP): ANALISI_INFRASTRUCTURE_MCP.md, MCP-43
  private dataChangedSub: Subscription | null = null;

  constructor(private service: CrmService,
              public setupService: CrmSetupService,
              private transloco: TranslocoService,
              private dataChangedNotifierService: DataChangedNotifierService) {
    this.title = this.transloco.translate('crm.pages.report.title');
    const oggi = new Date();
    this.al = oggi.toISOString().substring(0, 10);
    this.dal = new Date(oggi.getFullYear(), 0, 1, 12).toISOString().substring(0, 10);
  }

  async ionViewWillEnter() {
    await this.setupService.load();
    this.load();
    // "attivita' per utente": ricarica con il periodo scelto
    this.dataChangedSub?.unsubscribe();
    this.dataChangedSub = this.dataChangedNotifierService.of(ApplicationType.Crm).subscribe(() => this.load());
  }

  ionViewWillLeave() {
    this.dataChangedSub?.unsubscribe();
    this.dataChangedSub = null;
  }

  load() {
    const request = new GenericRequest<M.CrmReportInModel>(M.CrmReportInModel);
    request.data = { dal: this.dal, al: this.al };
    this.service.Report(request).subscribe(res => this.report = res.success ? res.data : null);
  }

  importo(v: number): string { return crmImporto(v, this.setupService.setup?.cfg.valuta); }

  // larghezza della barra rispetto al massimo della tabella
  barra(righe: M.CrmReportRigaModel[], r: M.CrmReportRigaModel, campo: 'valore' | 'numero'): string {
    const max = Math.max(...righe.map(x => x[campo]), 1);
    return Math.round(r[campo] / max * 100) + '%';
  }

  get conversione(): number {
    return this.report && this.report.leadCreati > 0 ? Math.round(this.report.leadConvertiti / this.report.leadCreati * 100) : 0;
  }

  get tassoVinte(): number {
    const tot = (this.report?.vinte ?? 0) + (this.report?.perse ?? 0);
    return tot > 0 ? Math.round((this.report?.vinte ?? 0) / tot * 100) : 0;
  }
}

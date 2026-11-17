import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { McpCompanySettingGetInModel, McpCompanySettingModel, McpCompanySettingPutInModel, McpStatusModel } from '../../../ClientServer-Service/Infrastructure/Mcp/Models/mcp-access-model';
import { McpAccessService } from '../../../ClientServer-Service/Infrastructure/Mcp/mcp-access.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/*
 Accesso AI dell'azienda (ANALISI_INFRASTRUCTURE_MCP.md, sezioni 8.5 e 8.6), solo CompanyPowerAdmin.
   - spento finche' non viene attivato: con l'accesso attivo i dati dell'azienda che l'utente puo' vedere arrivano
     al fornitore dell'AI che l'utente ha collegato (nota sulla riservatezza nella pagina)
   - livello: quali strumenti sono pubblicati (1 sola lettura, 2 anche le modifiche personali), non oltre quello
     ammesso dall'installazione
 Lo spegnimento vale subito per tutti i token che lavorano sull'azienda, anche quelli creati impersonando.
*/
@Component({
  selector: 'app-mcp-company-setting-page',
  templateUrl: './mcp-company-setting-page.component.html',
  styleUrls: ['./mcp-company-setting-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class McpCompanySettingPageComponent extends BasePageConfirmCancelComponent<McpCompanySettingModel> {

  public status: McpStatusModel | null = null;
  /** livelli che si possono scegliere: da 1 a quello ammesso dall'installazione */
  public livelli: number[] = [1];

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private mcpAccessService: McpAccessService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('pages.mcpCompanySetting.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      enabled: [false],
      maxLevel: [1, [Validators.required]],
    });
  }

  LoadData = (): Observable<McpCompanySettingModel | null> =>
    this.mcpAccessService.McpCompanySettingGet(new GenericRequest<McpCompanySettingGetInModel>(McpCompanySettingGetInModel)).pipe(
      map(res => {
        if (!res.success || !res.data)
          return null;
        this.status = res.data.status;
        const massimo = Math.max(1, this.status?.serverMaxLevel ?? 1);
        this.livelli = Array.from({ length: massimo }, (_, i) => i + 1);
        const setting = res.data.mcpCompanySetting ?? new McpCompanySettingModel();
        setting.maxLevel = Math.min(Math.max(1, setting.maxLevel), massimo);
        return setting;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );

  SaveData = (editModel: McpCompanySettingModel): Observable<boolean> => {
    const request = new GenericRequest<McpCompanySettingPutInModel>(McpCompanySettingPutInModel);
    request.data.mcpCompanySetting = { enabled: !!editModel.enabled, maxLevel: Number(editModel.maxLevel) || 1 };
    return this.mcpAccessService.McpCompanySettingPut(request).pipe(
      map(res => res.success),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }
}

import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController, ToastController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { McpTokenCreateInModel, McpTokenCreateOutModel, McpTokenListInModel } from '../../../ClientServer-Service/Infrastructure/Mcp/Models/mcp-access-model';
import { McpAccessService } from '../../../ClientServer-Service/Infrastructure/Mcp/mcp-access.service';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/*
 Nuovo token personale per l'accesso AI (ANALISI_INFRASTRUCTURE_MCP.md, sezione 7).
   - form: nome (per riconoscerlo nell'elenco), durata in giorni e lingua dei testi restituiti al client AI
   - il token lavora come l'utente corrente: se si sta impersonando qualcuno, come l'utente impersonato (sezione 7.1)
   - dopo la creazione la pagina resta aperta e mostra il valore del token, UNA VOLTA SOLA, con l'indirizzo
     dell'endpoint e le istruzioni per collegare Claude Code e Claude Desktop; poi resta solo "Chiudi"
*/
@Component({
  selector: 'app-mcp-token-edit-page',
  templateUrl: './mcp-token-edit-page.component.html',
  styleUrls: ['./mcp-token-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class McpTokenEditPageComponent extends BasePageConfirmCancelComponent<McpTokenCreateInModel> {

  /** token appena creato: finche' e' null si vede il form */
  public created: McpTokenCreateOutModel | null = null;
  public tokenMaxDays = 365;
  public readonly lingue = ['it', 'en'];
  public readonly btnCopia: ButtonItem;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private mcpAccessService: McpAccessService,
              private languageService: LanguageService,
              private toastCtrl: ToastController,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    this.btnConfirm.text = this.transloco.translate('pages.mcpTokenEdit.create');
    this.btnCopia = userInterfaceService.Btn_Copia;
  }

  get Title(): string { return this.transloco.translate('pages.mcpTokenEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      days: [90, [Validators.required, Validators.min(1), Validators.max(365)]],
      language: ['it', [Validators.required]],
    });
  }

  // nessun dato da modificare: si legge solo la durata massima ammessa dall'installazione
  LoadData = (): Observable<McpTokenCreateInModel | null> => {
    this.created = null;
    const modello = new McpTokenCreateInModel();
    modello.language = this.languageService.current === 'en' ? 'en' : 'it';

    return this.mcpAccessService.McpTokenList(new GenericRequest<McpTokenListInModel>(McpTokenListInModel)).pipe(
      map(res => {
        if (res.success && res.data?.status) {
          this.tokenMaxDays = res.data.status.tokenMaxDays;
          modello.days = Math.min(modello.days, this.tokenMaxDays);
          const giorni = this._editForm.get('days');
          giorni?.setValidators([Validators.required, Validators.min(1), Validators.max(this.tokenMaxDays)]);
          giorni?.updateValueAndValidity();
        }
        return modello;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );
  };

  SaveData = (editModel: McpTokenCreateInModel): Observable<boolean> => {
    const request = new GenericRequest<McpTokenCreateInModel>(McpTokenCreateInModel);
    request.data.name = (editModel.name ?? '').trim();
    request.data.days = Number(editModel.days) || 0;
    request.data.language = editModel.language;
    return this.mcpAccessService.McpTokenCreate(request).pipe(
      map(res => {
        if (res.success && res.data?.token) {
          this.created = res.data;
          return true;
        }
        return false;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  // creato il token si resta sulla pagina per mostrarne il valore: resta solo "Chiudi"
  override AfterSave(saved: boolean): void {
    if (!saved)
      return;
    const chiudi = this.userInterfaceService.Btn_Chiudi;
    chiudi.event = () => this.navCtrl.back();
    this.buttonbar = [chiudi];
  }

  /** comando per collegare Claude Code */
  public get comandoClaudeCode(): string {
    if (!this.created)
      return '';
    return `claude mcp add --transport http nvxapp ${this.created.endpointUrl} --header "Authorization: Bearer ${this.created.token}"`;
  }

  /** voce da aggiungere a "mcpServers" nel file di configurazione di Claude Desktop */
  public get configClaudeDesktop(): string {
    if (!this.created)
      return '';
    return JSON.stringify({
      nvxapp: {
        command: 'npx',
        args: ['-y', 'mcp-remote', this.created.endpointUrl, '--header', `Authorization: Bearer ${this.created.token}`]
      }
    }, null, 2);
  }

  public async copia(testo: string) {
    let copiato = false;
    try {
      await navigator.clipboard.writeText(testo);
      copiato = true;
    } catch {
      // appunti non disponibili (es. pagina non sicura): l'utente seleziona il testo a mano
    }
    const toast = await this.toastCtrl.create({
      message: this.transloco.translate(copiato ? 'pages.mcpTokenEdit.copied' : 'pages.mcpTokenEdit.copyFailed'),
      duration: 2500,
      position: 'top',
      color: copiato ? 'success' : 'warning'
    });
    await toast.present();
  }

  // seleziona tutto il testo al clic, per copiarlo a mano
  seleziona(event: Event) {
    const input = event.target as HTMLInputElement | null;
    input?.select?.();
  }
}

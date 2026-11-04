import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ModalController, AlertController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, from, map, of, switchMap } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CrmService } from '../../../ClientServer-Service/Crm/crm.service';
import * as M from '../../../ClientServer-Service/Crm/Models/crm-model';
import { CrmSetupService, crmAdesso, crmData, crmDataOra, crmImporto, crmPerInputData, crmPerInputDataOra } from '../../../Utility/Crm/crm-setup.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-crm-reassign-page',
  templateUrl: './crm-reassign-page.component.html',
  styleUrls: ['./crm-reassign-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Riassegnazione in blocco dei dati di un commerciale (es. dimissioni): amministratori dell'azienda
export class CrmReassignPageComponent extends BasePageConfirmCancelComponent<M.CrmRiassegnaInModel> {

  public esito: M.CrmRiassegnaOutModel | null = null;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: CrmService,
              public setupService: CrmSetupService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    this.btnConfirm.text = this.transloco.translate('crm.pages.reassign.run');
  }

  get Title(): string { return this.transloco.translate('crm.pages.reassign.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({ idDa: [null, [Validators.required]], idA: [null], clienti: [true], opportunita: [true], lead: [true], attivitaDaFare: [true] });
  }

  LoadData = (): Observable<M.CrmRiassegnaInModel | null> => from(this.setupService.load()).pipe(map(() => new M.CrmRiassegnaInModel()));

  SaveData = (editModel: M.CrmRiassegnaInModel): Observable<boolean> => {
    const request = new GenericRequest<M.CrmRiassegnaInModel>(M.CrmRiassegnaInModel);
    request.data = { ...editModel };
    return this.service.Riassegna(request).pipe(map(res => { this.esito = res.success ? res.data : null; return res.success; }), catchError(() => of(false)));
  };

  // si resta sulla pagina per mostrare l'esito
  override AfterSave(saved: boolean): void { }
}

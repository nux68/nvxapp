import { Component, Input, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, of } from 'rxjs';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { DeletionService } from '../../../../ClientServer-Service/Infrastructure/Deletion/deletion.service';
import {
  DeletionExecuteInModel, DeletionExecuteOutModel, DeletionNodeType, DeletionPreviewInModel, DeletionPreviewOutModel, DeletionTargetModel
} from '../../../../ClientServer-Service/Infrastructure/Deletion/Models/deletion-model';

export class DeletionDialogModel {
  public confirmText: string = '';
}

/*
 Cancellazione fisica di un centro, studio, azienda o utente: anteprima calcolata dal server (cosa verra' eliminato,
 utenti che restano, avvisi) e conferma forte digitando il nome. Alla conferma esegue l'eliminazione e si chiude
 (ruolo 'confirm', con il riepilogo DeletionExecuteOutModel); la pagina ricarica la lista.
*/
@Component({
  selector: 'app-deletion-dialog',
  templateUrl: './deletion-dialog.component.html',
  styleUrls: ['./deletion-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DeletionDialogComponent extends BaseDialogConfirmCancelComponent<DeletionDialogModel> {

  @Input() target: DeletionTargetModel = new DeletionTargetModel();

  public preview: DeletionPreviewOutModel | null = null;
  public running = false;

  constructor(protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              private deletionService: DeletionService,
              private transloco: TranslocoService,
              private cdRef: ChangeDetectorRef) {
    super(userInterfaceService, fb, modalCtrl);
  }

  override ngOnInit() {
    super.ngOnInit();

    // pulsante di conferma rosso, attivo solo con il nome digitato correttamente
    const btnDelete = this.userInterfaceService.Btn_Cancella;
    this.btnConfirm.text = this.transloco.translate('components.deletionDialog.confirm');
    this.btnConfirm.image = btnDelete.image;
    this.btnConfirm.color = btnDelete.color;
    this.btnConfirm.disabled = true;
    this.btnConfirm.event = () => this.handleConfirm();

    const request = new GenericRequest<DeletionPreviewInModel>(DeletionPreviewInModel);
    request.data.target = this.target;
    this.deletionService.DeletionPreview(request).subscribe(res => {
      if (res.success && res.data) {
        this.preview = res.data;
        this._editForm.get('confirmText')?.updateValueAndValidity();
      } else {
        // errore (es. permessi): il messaggio lo mostra il client generico, il dialog si chiude
        this.modalCtrl.dismiss(null, 'cancel');
      }
      this.cdRef.markForCheck();
    });
  }

  get Title(): string {
    const titoli: Record<DeletionNodeType, string> = {
      [DeletionNodeType.Dealer]: 'components.deletionDialog.titleDealer',
      [DeletionNodeType.FinancialAdvisor]: 'components.deletionDialog.titleFinancialAdvisor',
      [DeletionNodeType.Company]: 'components.deletionDialog.titleCompany',
      [DeletionNodeType.UserDealer]: 'components.deletionDialog.titleUserLink',
      [DeletionNodeType.UserFinancialAdvisor]: 'components.deletionDialog.titleUserLink',
      [DeletionNodeType.UserCompany]: 'components.deletionDialog.titleUserLink',
      [DeletionNodeType.User]: 'components.deletionDialog.titleUser',
    };
    return this.transloco.translate(titoli[this.target.nodeType] ?? 'components.deletionDialog.titleCompany');
  }

  get EditForm(): FormGroup {
    return this.fb.group({
      confirmText: ['', [Validators.required, this.sameName]],
    });
  }

  // il nome digitato coincide con quello dell'anteprima (maiuscole e spazi esterni non contano)
  private sameName = (control: AbstractControl): ValidationErrors | null => {
    const nome = (this.preview?.descrizione ?? '').trim().toLowerCase();
    const digitato = String(control.value ?? '').trim().toLowerCase();
    return this.preview?.canDelete && nome.length > 0 && digitato === nome ? null : { sameName: true };
  };

  LoadData = (): Observable<DeletionDialogModel> => of(this._editModel ?? new DeletionDialogModel());

  SaveData = (editModel: DeletionDialogModel): Observable<DeletionDialogModel> => of(editModel);

  private handleConfirm() {
    if (!this._editForm.valid || !this.preview || this.running)
      return;

    this.running = true;
    this.btnConfirm.disabled = true;

    const request = new GenericRequest<DeletionExecuteInModel>(DeletionExecuteInModel);
    request.data.target = this.target;
    request.data.confirmText = this._editForm.value.confirmText;
    this.deletionService.DeletionExecute(request).subscribe({
      next: res => {
        this.running = false;
        if (res.success)
          this.modalCtrl.dismiss(res.data as DeletionExecuteOutModel, 'confirm');
        else
          this.btnConfirm.disabled = !this._editForm.valid;
        this.cdRef.markForCheck();
      },
      error: () => {
        this.running = false;
        this.btnConfirm.disabled = !this._editForm.valid;
        this.cdRef.markForCheck();
      }
    });
  }
}

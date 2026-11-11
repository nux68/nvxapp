import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DeadlineDeleteInModel, DeadlineGetInModel, DeadlineModel, DeadlinePutInModel, DeadlineUserModel } from '../../../ClientServer-Service/Infrastructure/Deadline/Models/deadline-model';
import { DeadlineService } from '../../../ClientServer-Service/Infrastructure/Deadline/deadline.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

/*
 Scadenza inserita a mano (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.3), per gli amministratori dell'azienda:
 descrizione, note, data, giorni di preavviso del promemoria, responsabile (utenti dell'azienda).
 Le scadenze registrate dai moduli non si modificano qui (le gestisce il modulo); quelle manuali si possono cancellare.
*/
@Component({
  selector: 'app-deadline-edit-page',
  templateUrl: './deadline-edit-page.component.html',
  styleUrls: ['./deadline-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DeadlineEditPageComponent extends BasePageConfirmCancelComponent<DeadlineModel> {

  /** utenti dell'azienda selezionabili come responsabili */
  public utenti: DeadlineUserModel[] = [];

  private readonly btnDelete: ButtonItem;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private deadlineService: DeadlineService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = () => this.cancella();
  }

  get Title(): string { return this.transloco.translate('pages.deadlineEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(200)]],
      note: [null, [Validators.maxLength(500)]],
      dataScadenza: [null, [Validators.required]],
      giorniPreavviso: [30, [Validators.required, Validators.min(0), Validators.max(365)]],
      idAspNetUsersResponsabile: [null],
    });
  }

  LoadData = (): Observable<DeadlineModel | null> => {
    const state = history.state;
    const request = new GenericRequest<DeadlineGetInModel>(DeadlineGetInModel);
    request.data.id = state && state.id ? state.id : 0;

    return this.deadlineService.DeadlineGet(request).pipe(
      map(res => {
        if (!res.success || !res.data)
          return null;
        this.utenti = res.data.utenti ?? [];
        const deadline = res.data.deadline;
        // nel campo data solo il giorno (yyyy-MM-dd)
        deadline.dataScadenza = deadline.dataScadenza ? deadline.dataScadenza.substring(0, 10) : '';
        // cancellazione solo delle scadenze manuali gia' salvate
        this.buttonbar = deadline.id > 0 && deadline.manuale
          ? [this.btnConfirm, this.btnDelete, this.btnCancel]
          : [this.btnConfirm, this.btnCancel];
        return deadline;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );
  };

  SaveData = (editModel: DeadlineModel): Observable<boolean> => {
    const request = new GenericRequest<DeadlinePutInModel>(DeadlinePutInModel);
    request.data.id = editModel.id;
    request.data.descrizione = editModel.descrizione;
    request.data.note = editModel.note || null;
    request.data.dataScadenza = editModel.dataScadenza;
    request.data.giorniPreavviso = Number(editModel.giorniPreavviso) || 0;
    request.data.idAspNetUsersResponsabile = editModel.idAspNetUsersResponsabile || null;

    return this.deadlineService.DeadlinePut(request).pipe(
      map(res => res.success),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  // in caso di errore si resta sulla pagina
  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }

  private async cancella() {
    if (!this._editModel || !await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('pages.deadlineEdit.confirmDelete')))
      return;

    const request = new GenericRequest<DeadlineDeleteInModel>(DeadlineDeleteInModel);
    request.data.id = this._editModel.id;
    this.deadlineService.DeadlineDelete(request).subscribe(res => {
      if (res.success)
        this.navCtrl.back();
    });
  }
}

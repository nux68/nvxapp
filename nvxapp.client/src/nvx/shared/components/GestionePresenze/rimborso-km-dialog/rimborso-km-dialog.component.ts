import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { ModalController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { Dip_RapportoAutovettureModel, autovetturaTesto } from '../../../../ClientServer-Service/GestionePresenze/Dip_RapportoAutovetture/Models/dip-rapporto-autovetture-model';

/** dati di una spesa di rimborso km: autovettura e km percorsi (l'importo e' calcolato) */
export class RimborsoKmDialogModel {
  idDip_RapportoAutovetture: number | null = null;
  km: number | null = null;
}

/*
 Rimborso chilometrico di una spesa della nota spese: si sceglie l'autovettura del proprio rapporto di lavoro
 e si indicano i km; l'importo (km x rimborso per km dell'autovettura) e' solo mostrato, lo ricalcola il server.
*/
@Component({
  selector: 'app-rimborso-km-dialog',
  templateUrl: './rimborso-km-dialog.component.html',
  styleUrls: ['./rimborso-km-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class RimborsoKmDialogComponent extends BaseDialogConfirmCancelComponent<RimborsoKmDialogModel> {

  @Input() rimborsoKm: RimborsoKmDialogModel;
  @Input() autovetture: Dip_RapportoAutovettureModel[] = [];

  public readonly autovetturaTesto = autovetturaTesto;

  constructor(protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              private transloco: TranslocoService) {
    super(userInterfaceService, fb, modalCtrl);
  }

  override ionViewWillEnter() {
    // una sola autovettura: gia' scelta
    if (!this.rimborsoKm.idDip_RapportoAutovetture && this.autovetture.length === 1)
      this.rimborsoKm.idDip_RapportoAutovetture = this.autovetture[0].id;
    super.ionViewWillEnter();
  }

  get Title(): string { return this.transloco.translate('attendance.components.rimborsoKmDialog.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      idDip_RapportoAutovetture: [null, [Validators.required]],
      km: [null, [Validators.required, Validators.min(0.01), Validators.max(999999)]],
    });
  }

  LoadData = (): Observable<RimborsoKmDialogModel | null> => {
    return of(this.rimborsoKm);
  };

  SaveData = (editModel: RimborsoKmDialogModel): Observable<RimborsoKmDialogModel> => {
    return of(this._editModel);
  };

  public get autovettura(): Dip_RapportoAutovettureModel | undefined {
    const id = this._editForm?.get('idDip_RapportoAutovetture')?.value;
    return this.autovetture.find(a => a.id === id);
  }

  /** importo calcolato come fa il server: km x rimborso per km, arrotondato al centesimo */
  public get importo(): number {
    const km = Number(this._editForm?.get('km')?.value) || 0;
    return RimborsoKmDialogComponent.calcolaImporto(km, this.autovettura?.rimborsoKM ?? 0);
  }

  public static calcolaImporto(km: number, rimborsoKM: number): number {
    return Math.round(km * rimborsoKM * 100) / 100;
  }

  public errore(campo: string): boolean {
    const ctrl = this._editForm.get(campo);
    return !!ctrl && ctrl.invalid && ctrl.touched;
  }

  public euro(valore: number): string {
    return '€ ' + valore.toFixed(2).replace('.', ',');
  }

  public tariffa(valore: number): string {
    return '€ ' + valore.toFixed(4).replace('.', ',');
  }
}

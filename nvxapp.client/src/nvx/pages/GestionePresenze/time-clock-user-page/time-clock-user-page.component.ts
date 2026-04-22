import { Component, OnInit, OnDestroy } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { DipGGTimbraturaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/dip-gg-timbratura.service';
import { Dip_GG_Timbratura_Stamp_InModel, Dip_GG_Timbratura_StampPrepare_InModel, Dip_GG_Timbratura_StampPrepare_OutModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NavController } from '@ionic/angular';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { catchError, map, Observable, of } from 'rxjs';
import { Par_AttivitaModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';

@Component({
  selector: 'app-time-clock-user-page',
  templateUrl: './time-clock-user-page.component.html',
  styleUrls: ['./time-clock-user-page.component.scss'],
  standalone: false
})
export class TimeClockUserPageComponent extends BasePageConfirmCancelComponent<Dip_GG_Timbratura_StampPrepare_OutModel> {

  public currentTime: string = '';
  public formattedDate: string = '';
  public lastAction: string = '';
  public startDateBtn: string | undefined = undefined;

  private timeInterval: any;
  // true quando l'utente ha modificato manualmente il picker → il timer NON sovrascrive
  private userHasEdited: boolean = false;
  public par_AttivitaList: Par_AttivitaModel[] = []


  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private dipGGTimbraturaService: DipGGTimbraturaService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return 'Terminale di timbratura'; }

  get EditForm(): FormGroup {
    return this.fb.group({
      currentDate: [null, [Validators.required]],
      idPar_Attivita: [null, [Validators.required]],
    });
  }

  LoadData = (): Observable<Dip_GG_Timbratura_StampPrepare_OutModel | null> => {
    const request = new GenericRequest<Dip_GG_Timbratura_StampPrepare_InModel>(Dip_GG_Timbratura_StampPrepare_InModel);
    return this.dipGGTimbraturaService.PrepareStamp(request).pipe(
      map(res => {

        this.par_AttivitaList = res.data.par_Attivita;

        return res.data;
      }),
      catchError(error => {
        console.error('Errore durante PrepareStamp:', error);
        return of(null);
      })
    );
  };

  SaveData = (editModel: Dip_GG_Timbratura_StampPrepare_OutModel): Observable<boolean> => {
    const stampDate = this.startDateBtn ? new Date(this.startDateBtn) : new Date();

    const request = new GenericRequest<Dip_GG_Timbratura_Stamp_InModel>(Dip_GG_Timbratura_Stamp_InModel);
    request.data.dateStamp = this.toIsoLocal(stampDate);
    request.data.excludeRicalc = false;

    return this.dipGGTimbraturaService.Stamp(request).pipe(
      map(() => {
        this.lastAction = `${this.formatTime(stampDate)} (${this.formatDate(stampDate)})`;
        return true;
      }),
      catchError(error => {
        console.error('Errore durante Stamp:', error);
        return of(false);
      })
    );
  };

  override ionViewWillEnter() {
    this.userHasEdited = false;
    this.startClock();
    super.ionViewWillEnter();
  }

  ionViewWillLeave() {
    this.stopClock();
  }

  private startClock() {
    const tick = () => {
      const now = new Date();

      if (!this.userHasEdited) {
        this.currentTime = this.formatTime(now);
        this.formattedDate = this.formatDate(now);
        this.startDateBtn = this.toIsoLocal(now);
        this._editForm?.patchValue({ currentDate: this.startDateBtn });
      }

      const msToNextMinute = (60 - now.getSeconds()) * 1000 - now.getMilliseconds();
      this.timeInterval = setTimeout(tick, msToNextMinute);
    };

    tick();
  }

  private stopClock() {
    if (this.timeInterval) {
      clearTimeout(this.timeInterval);
    }
  }

  onStartDatetimeChange(event: any) {
    const value = event?.detail?.value;
    if (value) {
      const selected = new Date(value);
      this.startDateBtn = this.toIsoLocal(selected);
      this.currentTime = this.formatTime(selected);
      this.formattedDate = this.formatDate(selected);
      this.userHasEdited = true;
    }
  }

  private toIsoLocal(date: Date): string {
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` +
           `T${pad(date.getHours())}:${pad(date.getMinutes())}:00`;
  }

  formatTime(date: Date): string {
    return `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
  }

  formatDate(date: Date): string {
    return `${date.getDate().toString().padStart(2, '0')}/${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getFullYear()}`;
  }
}

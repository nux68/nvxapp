import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { TranslocoService } from '@jsverse/transloco';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TimbraturaEvolutaService } from '../../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/timbratura-evoluta.service';
import {
  Az_SediTimbraturaModel, RegolaGeofence, TimbraturaEvoluta_ConfigGetInModel, TimbraturaEvoluta_ConfigPutInModel, TimbraturaEvoluta_SediInModel
} from '../../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/Models/timbratura-evoluta-model';

/*
 Timbratura della sede (AT-TIMB.1-3): QR della postazione, geofence (coordinate, raggio, regola), kiosk e limite delle
 timbrature differite (offline). Il "+" del fab apre i PIN e i badge dei dipendenti (postazione QR e kiosk: menu Terminali).
*/
@Component({
  selector: 'app-timbratura-sede-page',
  templateUrl: './timbratura-sede-page.component.html',
  styleUrls: ['./timbratura-sede-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TimbraturaSedePageComponent extends BasePageConfirmCancelComponent<Az_SediTimbraturaModel> {

  public sedi: Az_SediTimbraturaModel[] = [];
  public readonly regola = RegolaGeofence;
  public kioskRegistrato = false;

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private timbraturaEvolutaService: TimbraturaEvolutaService,
    public fabMenuService: FabMenuService,
    private transloco: TranslocoService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('attendance.pages.timbraturaSede.title'); }

  get EditForm(): FormGroup {
    const form = this.fb.group({
      idAz_Sedi: [null as number | null, [Validators.required]],
      latitudine: [null as number | null, [Validators.min(-90), Validators.max(90)]],
      longitudine: [null as number | null, [Validators.min(-180), Validators.max(180)]],
      raggioMetri: [200, [Validators.required, Validators.min(10)]],
      regolaGeofence: [RegolaGeofence.Nessuna],
      qrAttivo: [false],
      qrObbligatorio: [false],
      qrDurataSecondi: [30, [Validators.min(10), Validators.max(600)]],
      kioskAttivo: [false],
      differitaMaxOre: [24, [Validators.min(0)]],
      rigeneraSegretoQr: [false],
    });
    form.get('idAz_Sedi')!.valueChanges.subscribe(id => { if (id && id !== this._editModel?.idAz_Sedi) this.caricaSede(id); });
    return form;
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.menu.pinTimbratura'), 'keypad-outline', () => this.navCtrl.navigateForward('/timbraturapin')),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  LoadData = (): Observable<Az_SediTimbraturaModel | null> => {
    return this.timbraturaEvolutaService.Sedi(new GenericRequest<TimbraturaEvoluta_SediInModel>(TimbraturaEvoluta_SediInModel)).pipe(
      map(res => {
        this.sedi = res.data?.sedi ?? [];
        const sede = this.sedi.find(s => s.idAz_Sedi === history.state?.idAz_Sedi) ?? this.sedi[0] ?? null;
        this.kioskRegistrato = sede?.kioskRegistrato ?? false;
        return sede;
      }),
      catchError(() => of(null))
    );
  };

  private caricaSede(idAz_Sedi: number) {
    const request = new GenericRequest<TimbraturaEvoluta_ConfigGetInModel>(TimbraturaEvoluta_ConfigGetInModel);
    request.data.idAz_Sedi = idAz_Sedi;
    this.timbraturaEvolutaService.ConfigGet(request).subscribe(res => {
      if (res.data?.config) {
        this._editModel = res.data.config;
        this.kioskRegistrato = res.data.config.kioskRegistrato;
        this._editForm.patchValue(res.data.config, { emitEvent: false });
      }
    });
  }

  /** coordinate della sede dalla posizione del dispositivo */
  usaPosizione() {
    navigator.geolocation?.getCurrentPosition(p => {
      this._editForm.patchValue({ latitudine: Math.round(p.coords.latitude * 1e6) / 1e6, longitudine: Math.round(p.coords.longitude * 1e6) / 1e6 });
    }, () => { }, { enableHighAccuracy: true, timeout: 15000 });
  }

  SaveData = (editModel: Az_SediTimbraturaModel): Observable<boolean> => {
    const request = new GenericRequest<TimbraturaEvoluta_ConfigPutInModel>(TimbraturaEvoluta_ConfigPutInModel);
    const num = (v: any) => v === null || v === undefined || v === '' ? null : Number(v);
    request.data.config = { ...editModel, latitudine: num(editModel.latitudine), longitudine: num(editModel.longitudine),
      raggioMetri: Number(editModel.raggioMetri), qrDurataSecondi: Number(editModel.qrDurataSecondi), differitaMaxOre: Number(editModel.differitaMaxOre) };
    return this.timbraturaEvolutaService.ConfigPut(request).pipe(
      map(res => res.success),
      catchError(() => of(false))
    );
  };

  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }
}

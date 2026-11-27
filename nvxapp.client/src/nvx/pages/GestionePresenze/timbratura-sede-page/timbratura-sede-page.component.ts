import { Component, ChangeDetectionStrategy, ElementRef, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { TranslocoService } from '@jsverse/transloco';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
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

  /*
   Cartina della posizione della sede (Leaflet + OpenStreetMap): punto della sede (trascinabile), cerchio del raggio e
   indirizzo ricavato dalle coordinate (Nominatim di OpenStreetMap). Si apre solo con il tasto; la libreria si carica
   allora. L'indirizzo si chiede con fetch, non con HttpClient: l'interceptor aggiungerebbe il token dell'utente.
  */
  @ViewChild('mappaEl') mappaEl?: ElementRef<HTMLDivElement>;
  public mappaAperta = false;
  public indirizzo: string | null = null;
  public indirizzoInCorso = false;
  public indirizzoErrore = false;
  private leaflet: any = null;
  private mappa: any = null;
  private marker: any = null;
  private cerchio: any = null;
  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private timbraturaEvolutaService: TimbraturaEvolutaService,
    public fabMenuService: FabMenuService,
    private transloco: TranslocoService,
    private languageService: LanguageService
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
    // cartina aperta: punto e cerchio seguono coordinate e raggio
    form.get('latitudine')!.valueChanges.subscribe(() => this.aggiornaMappa());
    form.get('longitudine')!.valueChanges.subscribe(() => this.aggiornaMappa());
    form.get('raggioMetri')!.valueChanges.subscribe(() => this.aggiornaMappa());
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
    this.chiudiMappa();
  }

  /** coordinate valide nel form (per la cartina) */
  get coordinate(): [number, number] | null {
    const lat = Number(this._editForm?.get('latitudine')?.value);
    const lon = Number(this._editForm?.get('longitudine')?.value);
    const vuoto = (v: any) => v === null || v === undefined || v === '';
    if (vuoto(this._editForm?.get('latitudine')?.value) || vuoto(this._editForm?.get('longitudine')?.value)
        || isNaN(lat) || isNaN(lon) || lat < -90 || lat > 90 || lon < -180 || lon > 180)
      return null;
    return [lat, lon];
  }

  private get raggio(): number {
    const r = Number(this._editForm?.get('raggioMetri')?.value);
    return isNaN(r) || r < 10 ? 10 : r;
  }

  /** tasto "Mostra sulla mappa": cartina con il punto, il cerchio del raggio e l'indirizzo */
  async mostraMappa() {
    const c = this.coordinate;
    if (!c) return;
    this.mappaAperta = true;
    if (!this.leaflet)
      { const m: any = await import('leaflet'); this.leaflet = m.default ?? m; }
    // il contenitore compare con @if: si aspetta che sia nel DOM
    setTimeout(() => {
      this.creaMappa();
      this.cercaIndirizzo();
    });
  }

  chiudiMappa() {
    this.mappa?.remove();
    this.mappa = this.marker = this.cerchio = null;
    this.mappaAperta = false;
  }

  private creaMappa() {
    const L = this.leaflet;
    const c = this.coordinate;
    if (!L || !c || !this.mappaEl) return;
    this.mappa?.remove();
    this.mappa = L.map(this.mappaEl.nativeElement, { zoomControl: true }).setView(c, 16);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener">OpenStreetMap</a>'
    }).addTo(this.mappa);
    this.cerchio = L.circle(c, { radius: this.raggio, color: '#FF7300', weight: 2, fillOpacity: 0.12 }).addTo(this.mappa);
    // punto trascinabile (icona disegnata in CSS: niente immagini della libreria da copiare)
    const icona = L.divIcon({ className: 'nvx-mappa-punto', iconSize: [22, 22], iconAnchor: [11, 11] });
    this.marker = L.marker(c, { draggable: true, icon: icona, autoPan: true }).addTo(this.mappa);
    this.marker.on('dragend', () => {
      const p = this.marker.getLatLng();
      this._editForm.patchValue({ latitudine: Math.round(p.lat * 1e6) / 1e6, longitudine: Math.round(p.lng * 1e6) / 1e6 });
      this._editForm.markAsDirty();
      this.cercaIndirizzo();
    });
    this.mappa.fitBounds(this.cerchio.getBounds(), { padding: [20, 20], maxZoom: 18 });
  }

  private aggiornaMappa() {
    const c = this.coordinate;
    if (!this.mappa || !c) return;
    this.marker?.setLatLng(c);
    this.cerchio?.setLatLng(c);
    this.cerchio?.setRadius(this.raggio);
  }

  /** indirizzo dalle coordinate (OpenStreetMap Nominatim), nella lingua dell'interfaccia */
  private async cercaIndirizzo() {
    const c = this.coordinate;
    if (!c) return;
    this.indirizzoInCorso = true;
    this.indirizzoErrore = false;
    try {
      const url = `https://nominatim.openstreetmap.org/reverse?format=jsonv2&zoom=18&lat=${c[0]}&lon=${c[1]}&accept-language=${this.languageService.current}`;
      const res = await fetch(url, { headers: { 'Accept': 'application/json' } });
      const dati = res.ok ? await res.json() : null;
      this.indirizzo = dati?.display_name ?? null;
      this.indirizzoErrore = !this.indirizzo;
    } catch {
      this.indirizzo = null;
      this.indirizzoErrore = true;
    }
    this.indirizzoInCorso = false;
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
        this.chiudiMappa();
        this.indirizzo = null;
      }
    });
  }

  /** coordinate della sede dalla posizione del dispositivo */
  usaPosizione() {
    navigator.geolocation?.getCurrentPosition(p => {
      this._editForm.patchValue({ latitudine: Math.round(p.coords.latitude * 1e6) / 1e6, longitudine: Math.round(p.coords.longitude * 1e6) / 1e6 });
      if (this.mappaAperta) {
        this.aggiornaMappa();
        this.mappa?.setView(this.coordinate, this.mappa.getZoom());
        this.cercaIndirizzo();
      }
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

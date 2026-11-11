import { Component, ChangeDetectionStrategy, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TimbraturaEvolutaService } from '../../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/timbratura-evoluta.service';
import {
  Az_SediTimbraturaModel, Dip_GG_TimbraturaInfoModel, EsitoGeofence, TimbraturaEvoluta_SediInModel, TimbraturaEvoluta_StampInModel
} from '../../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/Models/timbratura-evoluta-model';
import { TimbraturaInCoda, TimbraturaOfflineService } from '../../../Utility/GestionePresenze/timbratura-offline.service';

/*
 Timbratura in sede (AT-TIMB.1/2/4): con il QR della postazione (aperto dalla fotocamera: /timbraturaqr?c=<codice>, oppure
 letto qui con BarcodeDetector dove il browser lo offre), con la posizione per il geofence e senza rete (coda locale).
 L'ora e' quella del server; senza rete quella del dispositivo, accettata entro il limite della sede.
*/
@Component({
  selector: 'app-timbratura-qr-page',
  templateUrl: './timbratura-qr-page.component.html',
  styleUrls: ['./timbratura-qr-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TimbraturaQrPageComponent {

  @ViewChild('video') video?: ElementRef<HTMLVideoElement>;

  public title: string;
  public codice: string | null = null;
  public sedi: Az_SediTimbraturaModel[] = [];
  public idAz_Sedi: number | null = null;
  public ultima: Dip_GG_TimbraturaInfoModel | null = null;
  public messaggio: string | null = null;
  public inCorso = false;
  public scansione = false;
  public readonly lettoreDisponibile = 'BarcodeDetector' in window;
  public coda: TimbraturaInCoda[] = [];
  private stream: MediaStream | null = null;

  constructor(private route: ActivatedRoute,
              private timbraturaEvolutaService: TimbraturaEvolutaService,
              private timbraturaOfflineService: TimbraturaOfflineService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.timbraturaQr.title');
    this.timbraturaOfflineService.coda$.subscribe(c => this.coda = c);
  }

  ionViewWillEnter() {
    this.codice = this.route.snapshot.queryParamMap.get('c');
    this.timbraturaOfflineService.inviaCoda();
    this.timbraturaEvolutaService.Sedi(new GenericRequest<TimbraturaEvoluta_SediInModel>(TimbraturaEvoluta_SediInModel)).subscribe(res => {
      this.sedi = res.data?.sedi ?? [];
      this.idAz_Sedi = res.data?.idAz_Sedi_Proposta ?? this.sedi[0]?.idAz_Sedi ?? null;
    });
  }

  ionViewWillLeave() {
    this.fermaScansione();
  }

  private posizione(): Promise<GeolocationPosition | null> {
    return new Promise(resolve => {
      if (!navigator.geolocation)
        return resolve(null);
      navigator.geolocation.getCurrentPosition(p => resolve(p), () => resolve(null), { enableHighAccuracy: true, timeout: 10000, maximumAge: 30000 });
    });
  }

  async timbra() {
    if (this.inCorso)
      return;
    this.inCorso = true;
    this.messaggio = null;
    const pos = await this.posizione();
    const request = new GenericRequest<TimbraturaEvoluta_StampInModel>(TimbraturaEvoluta_StampInModel);
    request.data.codice = this.codice;
    request.data.idAz_Sedi = this.codice ? null : this.idAz_Sedi;
    request.data.latitudine = pos?.coords.latitude ?? null;
    request.data.longitudine = pos?.coords.longitude ?? null;
    request.data.precisioneMetri = pos?.coords.accuracy ?? null;

    if (!navigator.onLine) {
      this.timbraturaOfflineService.accoda(request.data);
      this.messaggio = this.transloco.translate('attendance.pages.timbraturaQr.queued');
      this.inCorso = false;
      return;
    }
    try {
      const res = await firstValueFrom(this.timbraturaEvolutaService.Stamp(request));
      if (res.success) {
        this.ultima = res.data?.info ?? null;
        this.messaggio = res.messages?.map(m => m.text).join(' ') || this.transloco.translate('attendance.pages.timbraturaQr.done');
        this.codice = null;   // il codice vale una volta
      }
    }
    catch {
      // rete assente o server non raggiungibile: in coda con l'ora del dispositivo
      this.timbraturaOfflineService.accoda(request.data);
      this.messaggio = this.transloco.translate('attendance.pages.timbraturaQr.queued');
    }
    this.inCorso = false;
  }

  inviaOra() {
    this.timbraturaOfflineService.inviaCoda();
  }

  togli(item: TimbraturaInCoda) {
    this.timbraturaOfflineService.togli(item.id);
  }

  esitoTesto(e: EsitoGeofence): string {
    return this.transloco.translate('attendance.pages.timbraturaQr.geo' + e);
  }

  /** lettura del QR con la fotocamera (BarcodeDetector, Chrome/Edge su Android e desktop) */
  async scansiona() {
    try {
      this.stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' } });
      this.scansione = true;
      setTimeout(async () => {
        const v = this.video?.nativeElement;
        if (!v || !this.stream) return;
        v.srcObject = this.stream;
        await v.play();
        const detector = new (window as any).BarcodeDetector({ formats: ['qr_code'] });
        const ciclo = async () => {
          if (!this.scansione) return;
          try {
            const trovati = await detector.detect(v);
            if (trovati.length > 0) {
              const testo: string = trovati[0].rawValue ?? '';
              const m = /[?&]c=([^&]+)/.exec(testo);
              this.codice = m ? decodeURIComponent(m[1]) : testo;
              this.fermaScansione();
              this.timbra();
              return;
            }
          } catch { }
          setTimeout(ciclo, 300);
        };
        ciclo();
      });
    }
    catch {
      this.messaggio = this.transloco.translate('attendance.pages.timbraturaQr.noCamera');
    }
  }

  fermaScansione() {
    this.scansione = false;
    this.stream?.getTracks().forEach(t => t.stop());
    this.stream = null;
  }
}

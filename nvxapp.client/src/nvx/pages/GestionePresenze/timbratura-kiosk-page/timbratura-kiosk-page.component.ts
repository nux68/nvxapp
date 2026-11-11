import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TimbraturaEvolutaService } from '../../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/timbratura-evoluta.service';
import {
  Az_SediTimbraturaModel, TimbraturaEvoluta_KioskRegistraInModel, TimbraturaEvoluta_KioskStampInModel, TimbraturaEvoluta_SediInModel
} from '../../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/Models/timbratura-evoluta-model';

/*
 Kiosk (AT-TIMB.3): dispositivo condiviso in sede, collegato con un amministratore dell'azienda e registrato sulla sede
 (token salvato nel dispositivo). Il dipendente digita il PIN o passa il badge (lettore che scrive come una tastiera).
*/
@Component({
  selector: 'app-timbratura-kiosk-page',
  templateUrl: './timbratura-kiosk-page.component.html',
  styleUrls: ['./timbratura-kiosk-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TimbraturaKioskPageComponent {

  private static readonly Chiave = 'nvx.kiosk';
  public title: string;
  public sedi: Az_SediTimbraturaModel[] = [];
  public idAz_Sedi: number | null = null;
  public token: string | null = null;
  public pin = '';
  public badge = '';
  public esito: { ok: boolean, testo: string } | null = null;
  public ora = new Date();
  public readonly tasti = ['1', '2', '3', '4', '5', '6', '7', '8', '9', 'C', '0', 'OK'];
  private timer: any;
  private timerEsito: any;

  constructor(private timbraturaEvolutaService: TimbraturaEvolutaService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.timbraturaKiosk.title');
  }

  ionViewWillEnter() {
    try {
      const salvato = JSON.parse(localStorage.getItem(TimbraturaKioskPageComponent.Chiave) ?? 'null');
      this.idAz_Sedi = salvato?.idAz_Sedi ?? null;
      this.token = salvato?.token ?? null;
    } catch { }
    this.timbraturaEvolutaService.Sedi(new GenericRequest<TimbraturaEvoluta_SediInModel>(TimbraturaEvoluta_SediInModel)).subscribe(res => {
      this.sedi = (res.data?.sedi ?? []).filter(s => s.kioskAttivo);
      if (!this.sedi.some(s => s.idAz_Sedi === this.idAz_Sedi)) {
        this.idAz_Sedi = this.sedi[0]?.idAz_Sedi ?? null;
        this.token = null;
      }
    });
    this.timer = setInterval(() => { this.ora = new Date(); }, 1000);
  }

  ionViewWillLeave() {
    clearInterval(this.timer);
  }

  get sedeNome(): string { return this.sedi.find(s => s.idAz_Sedi === this.idAz_Sedi)?.sede ?? ''; }

  async registra() {
    if (!this.idAz_Sedi)
      return;
    const request = new GenericRequest<TimbraturaEvoluta_KioskRegistraInModel>(TimbraturaEvoluta_KioskRegistraInModel);
    request.data.idAz_Sedi = this.idAz_Sedi;
    const res = await firstValueFrom(this.timbraturaEvolutaService.KioskRegistra(request));
    if (res.success && res.data?.kioskToken) {
      this.token = res.data.kioskToken;
      try { localStorage.setItem(TimbraturaKioskPageComponent.Chiave, JSON.stringify({ idAz_Sedi: this.idAz_Sedi, token: this.token })); } catch { }
      document.documentElement.requestFullscreen?.().catch(() => { });
    }
  }

  tasto(t: string) {
    if (t === 'C') this.pin = '';
    else if (t === 'OK') this.invia();
    else if (this.pin.length < 8) this.pin += t;
  }

  async invia() {
    if (!this.idAz_Sedi || !this.token || (!this.pin && !this.badge))
      return;
    const request = new GenericRequest<TimbraturaEvoluta_KioskStampInModel>(TimbraturaEvoluta_KioskStampInModel);
    request.data.idAz_Sedi = this.idAz_Sedi;
    request.data.kioskToken = this.token;
    request.data.pin = this.pin || null;
    request.data.badge = this.badge || null;
    this.pin = '';
    this.badge = '';
    try {
      const res = await firstValueFrom(this.timbraturaEvolutaService.KioskStamp(request));
      this.esito = res.success
        ? { ok: true, testo: this.transloco.translate('attendance.pages.timbraturaKiosk.done', { name: res.data?.nominativo ?? '', time: (res.data?.ora ?? '').substring(11, 16) }) }
        : { ok: false, testo: res.messages?.map(m => m.text).join(' ') ?? '' };
    }
    catch {
      this.esito = { ok: false, testo: this.transloco.translate('attendance.pages.timbraturaKiosk.offline') };
    }
    clearTimeout(this.timerEsito);
    this.timerEsito = setTimeout(() => { this.esito = null; }, 5000);
  }

  schermoIntero() {
    document.documentElement.requestFullscreen?.().catch(() => { });
  }
}

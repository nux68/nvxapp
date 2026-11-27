import { Component, ChangeDetectionStrategy, HostListener } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TimbraturaEvolutaService } from '../../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/timbratura-evoluta.service';
import {
  Az_SediTimbraturaModel, TimbraturaEvoluta_KioskIdentificaInModel, TimbraturaEvoluta_KioskRegistraInModel, TimbraturaEvoluta_KioskStampInModel,
  TimbraturaEvoluta_SediInModel, TimbraturaOpzioneModel
} from '../../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/Models/timbratura-evoluta-model';
import { TerminaleService } from '../../../Utility/GestionePresenze/terminale.service';

/*
 Kiosk (AT-TIMB.3): dispositivo condiviso in sede, collegato con un amministratore dell'azienda e registrato sulla sede
 (token salvato nel dispositivo). Il dipendente digita il PIN o passa il badge (lettore che scrive come una tastiera).
 Le chiamate del kiosk non aprono il dialog degli errori: PIN/badge errati, blocco e altri esiti negativi solo nella
 segnalazione in rosso.
 Riconosciuto il dipendente (KioskIdentifica) si sceglie reparto - attivita' tra quelli della sede compatibili con lui
 (AT-TIMB.7): con una sola opzione non si chiede nulla, senza scelta entro 10 secondi si timbra quella di default.
 Tastiera fisica: le cifre come il tastierino, Invio = OK, Tab e Canc = C, Backspace toglie l'ultima cifra (non quando si
 scrive nel campo del badge, che ha il suo Invio). Nella scelta di reparto - attivita': frecce su e giu' spostano
 l'evidenziata (il conto alla rovescia riparte), Invio la conferma, Canc annulla.
 La registrazione e "Schermo intero" avviano il terminale (TerminaleService): menu e barra nascosti, uscita solo con la
 password di un CompanyPowerAdmin.
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
  private static readonly SecondiScelta = 10;
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
  private timerScelta: any;
  // dipendente riconosciuto in attesa della scelta di reparto - attivita' (PIN/badge ripetuti nel KioskStamp);
  // indiceDefault = riga evidenziata (all'inizio quella proposta dal server, poi spostata con le frecce)
  public scelta: { nominativo: string, opzioni: TimbraturaOpzioneModel[], indiceDefault: number, pin: string | null, badge: string | null } | null = null;
  public secondi = 0;

  constructor(private timbraturaEvolutaService: TimbraturaEvolutaService,
              private transloco: TranslocoService,
              public terminale: TerminaleService) {
    this.title = this.transloco.translate('attendance.pages.timbraturaKiosk.title');
  }

  ionViewWillEnter() {
    try {
      const salvato = JSON.parse(localStorage.getItem(TimbraturaKioskPageComponent.Chiave) ?? 'null');
      this.idAz_Sedi = salvato?.idAz_Sedi ?? null;
      this.token = salvato?.token ?? null;
    } catch { }
    this.timbraturaEvolutaService.Sedi(new GenericRequest<TimbraturaEvoluta_SediInModel>(TimbraturaEvoluta_SediInModel), true).subscribe(res => {
      this.sedi = (res.data?.sedi ?? []).filter(s => s.kioskAttivo);
      const sede = this.sedi.find(s => s.idAz_Sedi === this.idAz_Sedi);
      if (!sede) {
        this.idAz_Sedi = this.sedi[0]?.idAz_Sedi ?? null;
        this.dimenticaRegistrazione();
      }
      // registrazione annullata dalla pagina Timbratura della sede: si torna a "Registra il dispositivo"
      else if (this.token && !sede.kioskRegistrato)
        this.dimenticaRegistrazione();
    });
    this.timer = setInterval(() => { this.ora = new Date(); }, 1000);
  }

  ionViewWillLeave() {
    clearInterval(this.timer);
    this.annullaScelta();
    // pagina lasciata col terminale attivo solo a sessione finita (altrimenti terminaleGuard chiede la password)
    this.terminale.disattiva();
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
      await this.terminale.attiva();
    }
  }

  @HostListener('document:keydown', ['$event'])
  tastiera(ev: KeyboardEvent) {
    if (!this.token || ev.ctrlKey || ev.altKey || ev.metaKey)
      return;
    if (this.scelta) {
      this.tastieraScelta(ev);
      return;
    }
    // nel campo del badge (o in un altro campo, es. la password di uscita) la tastiera e' del campo
    const el = ev.target as HTMLElement | null;
    if (el?.closest('input, textarea, ion-input, ion-alert'))
      return;
    if (/^[0-9]$/.test(ev.key))
      this.tasto(ev.key);
    else if (ev.key === 'Enter')
      this.tasto('OK');
    else if (ev.key === 'Tab' || ev.key === 'Delete')
      this.tasto('C');
    else if (ev.key === 'Backspace')
      this.pin = this.pin.slice(0, -1);
    else
      return;
    ev.preventDefault();
  }

  private tastieraScelta(ev: KeyboardEvent) {
    const scelta = this.scelta!;
    const n = scelta.opzioni.length;
    if (ev.key === 'ArrowDown' || ev.key === 'ArrowUp') {
      scelta.indiceDefault = (scelta.indiceDefault + (ev.key === 'ArrowDown' ? 1 : n - 1)) % n;
      this.secondi = TimbraturaKioskPageComponent.SecondiScelta;
      setTimeout(() => document.querySelectorAll('.nvx-scelta ion-button')[scelta.indiceDefault]?.scrollIntoView({ block: 'nearest' }));
    }
    else if (ev.key === 'Enter')
      this.scegli(scelta.opzioni[scelta.indiceDefault] ?? null);
    else if (ev.key === 'Delete' || ev.key === 'Escape')
      this.annullaScelta();
    else
      return;
    ev.preventDefault();
  }

  tasto(t: string) {
    if (t === 'C') this.pin = '';
    else if (t === 'OK') this.invia();
    else if (this.pin.length < 8) this.pin += t;
  }

  async invia() {
    if (!this.idAz_Sedi || !this.token || (!this.pin && !this.badge) || this.scelta)
      return;
    const pin = this.pin || null;
    const badge = this.badge || null;
    this.pin = '';
    this.badge = '';
    const request = new GenericRequest<TimbraturaEvoluta_KioskIdentificaInModel>(TimbraturaEvoluta_KioskIdentificaInModel);
    request.data.idAz_Sedi = this.idAz_Sedi;
    request.data.kioskToken = this.token;
    request.data.pin = pin;
    request.data.badge = badge;
    try {
      const res = await firstValueFrom(this.timbraturaEvolutaService.KioskIdentifica(request, true));
      if (!res.success || !res.data) {
        this.mostraEsito({ ok: false, testo: res.messages?.map(m => m.text).join(' ') ?? '' });
        this.verificaRegistrazione();
        return;
      }
      // una sola opzione (o nessuna): non si chiede nulla
      if (res.data.opzioni.length <= 1) {
        await this.timbra(pin, badge, null);
        return;
      }
      this.esito = null;
      this.scelta = { nominativo: res.data.nominativo ?? '', opzioni: res.data.opzioni, indiceDefault: res.data.indiceDefault, pin, badge };
      this.secondi = TimbraturaKioskPageComponent.SecondiScelta;
      this.timerScelta = setInterval(() => {
        if (--this.secondi <= 0)
          this.scegli(null);
      }, 1000);
    }
    catch {
      this.mostraEsito({ ok: false, testo: this.transloco.translate('attendance.pages.timbraturaKiosk.offline') });
    }
  }

  /** Riga toccata (null: tempo scaduto, si timbra l'opzione di default). */
  async scegli(opzione: TimbraturaOpzioneModel | null) {
    const scelta = this.scelta;
    if (!scelta)
      return;
    this.annullaScelta();
    await this.timbra(scelta.pin, scelta.badge, opzione ?? scelta.opzioni[scelta.indiceDefault] ?? null);
  }

  annullaScelta() {
    clearInterval(this.timerScelta);
    this.scelta = null;
  }

  private async timbra(pin: string | null, badge: string | null, opzione: TimbraturaOpzioneModel | null) {
    const request = new GenericRequest<TimbraturaEvoluta_KioskStampInModel>(TimbraturaEvoluta_KioskStampInModel);
    request.data.idAz_Sedi = this.idAz_Sedi!;
    request.data.kioskToken = this.token;
    request.data.pin = pin;
    request.data.badge = badge;
    request.data.idAz_SubCommessaAttivita = opzione?.idAz_SubCommessaAttivita ?? 0;
    request.data.idAz_SediReparto = opzione?.idAz_SediReparto ?? null;
    try {
      const res = await firstValueFrom(this.timbraturaEvolutaService.KioskStamp(request, true));
      this.esito = res.success
        ? { ok: true, testo: this.transloco.translate('attendance.pages.timbraturaKiosk.done', { name: res.data?.nominativo ?? '', time: (res.data?.ora ?? '').substring(11, 16) }) }
        : { ok: false, testo: res.messages?.map(m => m.text).join(' ') ?? '' };
    }
    catch {
      this.esito = { ok: false, testo: this.transloco.translate('attendance.pages.timbraturaKiosk.offline') };
    }
    this.mostraEsito(this.esito);
  }

  private mostraEsito(esito: { ok: boolean, testo: string } | null) {
    this.esito = esito;
    clearTimeout(this.timerEsito);
    this.timerEsito = setTimeout(() => { this.esito = null; }, 5000);
  }

  schermoIntero() {
    this.terminale.attiva();
  }

  esci() {
    this.terminale.esci();
  }

  // dopo un esito negativo: se la registrazione della sede e' stata annullata il dispositivo la dimentica
  private verificaRegistrazione() {
    this.timbraturaEvolutaService.Sedi(new GenericRequest<TimbraturaEvoluta_SediInModel>(TimbraturaEvoluta_SediInModel), true).subscribe({
      next: res => {
        const sede = (res.data?.sedi ?? []).find(s => s.idAz_Sedi === this.idAz_Sedi);
        if (res.success && this.token && (!sede?.kioskAttivo || !sede.kioskRegistrato))
          this.dimenticaRegistrazione();
      },
      error: () => { }
    });
  }

  /** registrazione dimenticata: la pagina torna a "Registra il dispositivo" (il terminale resta com'e') */
  private dimenticaRegistrazione() {
    this.token = null;
    this.pin = '';
    this.badge = '';
    try { localStorage.removeItem(TimbraturaKioskPageComponent.Chiave); } catch { }
  }
}

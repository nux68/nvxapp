import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TimbraturaEvolutaService } from '../../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/timbratura-evoluta.service';
import {
  Az_SediTimbraturaModel, TimbraturaEvoluta_QrCodeInModel, TimbraturaEvoluta_QrCodeOutModel, TimbraturaEvoluta_SediInModel
} from '../../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/Models/timbratura-evoluta-model';
import { TerminaleService } from '../../../Utility/GestionePresenze/terminale.service';

/*
 Postazione QR (AT-TIMB.1): mostra il QR firmato della sede e lo rinnova alla scadenza. Il dipendente lo inquadra con la
 fotocamera del telefono: si apre /timbraturaqr?c=<codice> e la timbratura parte con la posizione.
 La pagina resta su un dispositivo con un amministratore collegato; "Schermo intero" avvia il terminale (TerminaleService):
 menu e barra nascosti, scelta della sede bloccata, uscita solo con la password di un CompanyPowerAdmin.
*/
@Component({
  selector: 'app-timbratura-postazione-page',
  templateUrl: './timbratura-postazione-page.component.html',
  styleUrls: ['./timbratura-postazione-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TimbraturaPostazionePageComponent {

  private static readonly ChiaveSede = 'nvx.postazione.sede';
  public title: string;
  public sedi: Az_SediTimbraturaModel[] = [];
  public idAz_Sedi: number | null = null;
  public qr: TimbraturaEvoluta_QrCodeOutModel | null = null;
  public ora = new Date();
  private timer: any;

  constructor(private timbraturaEvolutaService: TimbraturaEvolutaService,
              private transloco: TranslocoService,
              public terminale: TerminaleService) {
    this.title = this.transloco.translate('attendance.pages.timbraturaPostazione.title');
  }

  ionViewWillEnter() {
    this.timbraturaEvolutaService.Sedi(new GenericRequest<TimbraturaEvoluta_SediInModel>(TimbraturaEvoluta_SediInModel)).subscribe(res => {
      this.sedi = (res.data?.sedi ?? []).filter(s => s.qrAttivo);
      let salvata: number | null = null;
      try { salvata = Number(localStorage.getItem(TimbraturaPostazionePageComponent.ChiaveSede)) || null; } catch { }
      this.idAz_Sedi = this.sedi.find(s => s.idAz_Sedi === salvata)?.idAz_Sedi ?? this.sedi[0]?.idAz_Sedi ?? null;
      this.aggiorna();
    });
  }

  ionViewWillLeave() {
    clearTimeout(this.timer);
    // pagina lasciata col terminale attivo solo a sessione finita (altrimenti terminaleGuard chiede la password)
    this.terminale.disattiva();
  }

  cambiaSede() {
    try { localStorage.setItem(TimbraturaPostazionePageComponent.ChiaveSede, String(this.idAz_Sedi ?? '')); } catch { }
    this.aggiorna();
  }

  aggiorna() {
    clearTimeout(this.timer);
    if (!this.idAz_Sedi)
      return;
    const request = new GenericRequest<TimbraturaEvoluta_QrCodeInModel>(TimbraturaEvoluta_QrCodeInModel);
    request.data.idAz_Sedi = this.idAz_Sedi;
    request.data.origine = window.location.origin;
    this.timbraturaEvolutaService.QrCode(request).subscribe({
      next: res => {
        this.qr = res.success ? res.data : null;
        this.ora = new Date();
        // nuovo codice allo scadere (con un secondo di margine)
        this.timer = setTimeout(() => this.aggiorna(), Math.max(2, (this.qr?.scadeTraSecondi ?? 30)) * 1000 + 1000);
      },
      error: () => { this.timer = setTimeout(() => this.aggiorna(), 10000); }
    });
  }

  schermoIntero() {
    this.terminale.attiva();
  }

  esci() {
    this.terminale.esci();
  }
}

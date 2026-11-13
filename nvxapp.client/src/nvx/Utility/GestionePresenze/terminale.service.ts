import { Injectable, inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { AlertController, MenuController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../infrastructure/auth.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { TimbraturaEvolutaService } from '../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/timbratura-evoluta.service';
import {
  TimbraturaEvoluta_SediInModel, TimbraturaEvoluta_TerminaleSbloccaInModel
} from '../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/Models/timbratura-evoluta-model';

/*
 Modalita' terminale delle pagine Postazione QR e Kiosk (menu "Terminali"):
   - con "Schermo intero" la pagina va a tutto schermo, il menu a sinistra sparisce (menu disattivato: lo split-pane non lo mostra)
     e la barra della pagina si nasconde; lo schermo resta acceso (Wake Lock, dove il browser lo consente)
   - non si esce (pulsante Esci, indietro del browser o del telefono, altre navigazioni: terminaleGuard sulla route) senza la
     password di un CompanyPowerAdmin dell'azienda, verificata dal server (TimbraturaEvoluta/TerminaleSblocca)
   - il token di sessione scade dopo ~20 minuti senza chiamate (ogni risposta del server lo rinnova): finche' il terminale e'
     attivo una chiamata leggera ogni 5 minuti, e al ritorno in primo piano, lo tiene vivo. Se la sessione scade comunque
     (es. dispositivo sospeso a lungo) l'interceptor porta al login: la guardia lascia passare, non c'e' piu' nulla da proteggere
   - Esc del browser esce solo dallo schermo intero: il terminale resta bloccato e la pagina offre di tornare a tutto schermo
*/
@Injectable({
  providedIn: 'root'
})
export class TerminaleService {

  private static readonly KeepAliveMs = 5 * 60 * 1000;

  private _attivo = false;
  private keepAlive: any = null;
  private wakeLock: any = null;
  private sbloccoInCorso: Promise<boolean> | null = null;

  constructor(private menuCtrl: MenuController,
              private alertCtrl: AlertController,
              private transloco: TranslocoService,
              private authService: AuthService,
              private timbraturaEvolutaService: TimbraturaEvolutaService) { }

  /** Terminale attivo: menu nascosto, uscita solo con la password. */
  public get attivo(): boolean { return this._attivo; }

  /** La pagina e' a schermo intero (Esc del browser la riporta in finestra senza sbloccare il terminale). */
  public get schermoIntero(): boolean { return !!document.fullscreenElement; }

  /** Avvia il terminale (o torna a schermo intero se gia' attivo): va chiamato da un gesto dell'utente (clic). */
  public async attiva(): Promise<void> {
    if (!this._attivo) {
      this._attivo = true;
      await this.menuCtrl.enable(false);
      this.keepAlive = setInterval(() => this.mantieniSessione(), TerminaleService.KeepAliveMs);
      document.addEventListener('visibilitychange', this.onVisibilita);
      window.addEventListener('beforeunload', this.onBeforeUnload);
    }
    try { await document.documentElement.requestFullscreen?.(); } catch { }
    await this.richiediWakeLock();
  }

  /**
   * Uscita dal terminale: chiede la password di un CompanyPowerAdmin; true se si puo' uscire (anche quando il terminale non e'
   * attivo o la sessione e' gia' finita). Usata dal pulsante Esci e da terminaleGuard.
   */
  public esci(): Promise<boolean> {
    if (!this._attivo)
      return Promise.resolve(true);
    if (this.authService.Token == null) {
      this.disattiva();
      return Promise.resolve(true);
    }
    // indietro ripetuto mentre la richiesta e' aperta: una sola richiesta
    this.sbloccoInCorso ??= this.chiediPassword().then(ok => {
      this.sbloccoInCorso = null;
      if (ok)
        this.disattiva();
      return ok;
    });
    return this.sbloccoInCorso;
  }

  /** Spegne il terminale senza chiedere la password (pagina lasciata per fine sessione). */
  public disattiva(): void {
    if (!this._attivo)
      return;
    this._attivo = false;
    clearInterval(this.keepAlive);
    this.keepAlive = null;
    document.removeEventListener('visibilitychange', this.onVisibilita);
    window.removeEventListener('beforeunload', this.onBeforeUnload);
    this.menuCtrl.enable(true);
    try { this.wakeLock?.release?.(); } catch { }
    this.wakeLock = null;
    if (document.fullscreenElement)
      document.exitFullscreen?.().catch(() => { });
  }

  private async chiediPassword(): Promise<boolean> {
    const alert = await this.alertCtrl.create({
      header: this.transloco.translate('attendance.pages.terminale.unlockTitle'),
      message: this.transloco.translate('attendance.pages.terminale.unlockMessage'),
      backdropDismiss: false,
      inputs: [{ name: 'password', type: 'password', placeholder: this.transloco.translate('attendance.pages.terminale.password'), attributes: { autocomplete: 'off' } }],
      buttons: [
        { text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' },
        { text: 'OK', role: 'confirm' }
      ]
    });
    await alert.present();
    const { data, role } = await alert.onDidDismiss();
    const password: string = data?.values?.password ?? '';
    if (role !== 'confirm' || !password)
      return false;
    const request = new GenericRequest<TimbraturaEvoluta_TerminaleSbloccaInModel>(TimbraturaEvoluta_TerminaleSbloccaInModel);
    request.data.password = password;
    try {
      // password errata: il messaggio del server lo mostra il dialog degli errori comune
      const res = await firstValueFrom(this.timbraturaEvolutaService.TerminaleSblocca(request));
      return res.success && !!res.data?.sbloccato;
    }
    catch {
      return false;
    }
  }

  // ogni risposta del server porta un token nuovo (l'interceptor lo salva): basta una chiamata leggera
  private mantieniSessione(): void {
    if (!this._attivo || this.authService.Token == null)
      return;
    this.timbraturaEvolutaService.Sedi(new GenericRequest<TimbraturaEvoluta_SediInModel>(TimbraturaEvoluta_SediInModel))
      .subscribe({ error: () => { } });
  }

  private async richiediWakeLock(): Promise<void> {
    try {
      if (this.wakeLock || document.visibilityState !== 'visible')
        return;
      this.wakeLock = await (navigator as any).wakeLock?.request('screen') ?? null;
      this.wakeLock?.addEventListener?.('release', () => { this.wakeLock = null; });
    } catch { this.wakeLock = null; }
  }

  // tornando in primo piano: sessione rinnovata subito e schermo di nuovo tenuto acceso (il wake lock si perde in background)
  private onVisibilita = () => {
    if (document.visibilityState === 'visible') {
      this.mantieniSessione();
      this.richiediWakeLock();
    }
  };

  // ricarica o chiusura della scheda: il browser chiede conferma (ricaricando si perde comunque la sessione)
  private onBeforeUnload = (ev: BeforeUnloadEvent) => {
    ev.preventDefault();
    ev.returnValue = '';
  };
}

/** Route dei terminali: lasciare la pagina col terminale attivo richiede la password di un CompanyPowerAdmin. */
export const terminaleGuard: CanDeactivateFn<unknown> = () => inject(TerminaleService).esci();

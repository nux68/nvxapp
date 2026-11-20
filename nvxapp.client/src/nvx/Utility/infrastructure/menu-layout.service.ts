import { Injectable, signal } from '@angular/core';

// Menu laterale nella versione desktop: fisso a sinistra (aperto) o nascosto (chiuso).
// Aperto: ion-split-pane attivo da 'lg' in su (larghezza >= 992px), come prima.
// Chiuso: ion-split-pane disattivato, il menu diventa a scomparsa e la barra della pagina mostra la freccia per riaprirlo.
// Su mobile (< 992px) il menu e' sempre a scomparsa e la scelta non ha effetto.
// La scelta dell'utente e' salvata nel browser.
@Injectable({
  providedIn: 'root'
})
export class MenuLayoutService {

  private static readonly StorageKey = 'nvx-menu-desktop';
  private static readonly DesktopQuery = '(min-width: 992px)'; // breakpoint 'lg' di ion-split-pane

  private readonly desktopMedia = window.matchMedia(MenuLayoutService.DesktopQuery);

  /** larghezza da desktop (stessa soglia di ion-split-pane) */
  public readonly isDesktop = signal<boolean>(this.desktopMedia.matches);

  /** menu fisso a sinistra nella versione desktop */
  public readonly desktopMenuOpen = signal<boolean>(this.readSaved());

  constructor() {
    this.desktopMedia.addEventListener('change', e => this.isDesktop.set(e.matches));
  }

  /** valore per [when] di ion-split-pane: false lo disattiva (menu a scomparsa) */
  public get splitPaneWhen(): string | boolean {
    return this.desktopMenuOpen() ? 'lg' : false;
  }

  /** desktop con il menu chiuso: la barra della pagina mostra la freccia per riaprirlo */
  public get showReopen(): boolean {
    return this.isDesktop() && !this.desktopMenuOpen();
  }

  public toggle(): void {
    this.setOpen(!this.desktopMenuOpen());
  }

  public setOpen(open: boolean): void {
    this.desktopMenuOpen.set(open);
    try { localStorage.setItem(MenuLayoutService.StorageKey, open ? 'open' : 'closed'); } catch { }
  }

  private readSaved(): boolean {
    try {
      return localStorage.getItem(MenuLayoutService.StorageKey) !== 'closed';
    } catch {
      return true;
    }
  }
}

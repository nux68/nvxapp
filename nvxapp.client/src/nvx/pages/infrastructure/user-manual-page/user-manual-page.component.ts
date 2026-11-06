import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserManualGenerateInModel, UserManualService } from '../../../ClientServer-Service/Infrastructure/UserManual/user-manual.service';
import { SITE_NAME } from '../../../Utility/infrastructure/site-name';

/*
 Manuale utente in PDF (codice nell'url: /usermanual/<codice>). All'apertura si avvia il job che prepara il PDF; quando
 e' pronto compare nella barra dei job in alto, con il pulsante di download. Si puo' rigenerare con il pulsante.
 Titolo e descrizione: chiavi pages.userManual.manuals.<codice> (ogni ramo aggiunge le sue nelle proprie traduzioni).
*/
@Component({
  selector: 'app-user-manual-page',
  templateUrl: './user-manual-page.component.html',
  styleUrls: ['./user-manual-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class UserManualPageComponent {

  public code = '';
  public inPreparazione = false;

  constructor(private route: ActivatedRoute,
              private service: UserManualService,
              private transloco: TranslocoService) { }

  get title(): string { return this.testo('title'); }
  get description(): string { return this.testo('description'); }

  // testo del manuale con ripiego su quello generico
  private testo(campo: string): string {
    const chiave = `pages.userManual.manuals.${this.code}.${campo}`;
    const t = this.transloco.translate(chiave);
    return t !== chiave ? t : this.transloco.translate('pages.userManual.' + campo);
  }

  ionViewWillEnter() {
    this.code = this.route.snapshot.paramMap.get('code') ?? '';
    this.genera();
  }

  genera() {
    if (!this.code)
      return;
    const request = new GenericRequest<UserManualGenerateInModel>(UserManualGenerateInModel);
    request.data = { code: this.code, siteName: SITE_NAME };
    this.inPreparazione = true;
    this.service.Generate(request).subscribe({ next: () => this.inPreparazione = false, error: () => this.inPreparazione = false });
  }
}

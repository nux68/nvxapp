import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

/** messaggio del modulo di contatto */
export class ShowcaseContactModel {
  nome: string = '';
  email: string = '';
  messaggio: string = '';
}

// Showcase: recapiti e modulo di contatto. Template: i testi sono in ShowcaseService.Contenuti.contact.
// Il modulo non passa dal server: prepara il messaggio nel programma di posta del visitatore.
@Component({
  selector: 'app-showcase-contact-page',
  templateUrl: './showcase-contact-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ShowcaseContactPageComponent extends BasePageConfirmCancelComponent<ShowcaseContactModel> {

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              public showcaseService: ShowcaseService,
              private transloco: TranslocoService) {

    super(navCtrl, userInterfaceService, fb);

    this.btnConfirm.text = this.transloco.translate('common.buttons.send');
    this.btnConfirm.image = 'send-outline';
    this.btnConfirm.disabled = true;
  }

  get Title(): string { return this.transloco.translate('showcase.menu.contact'); }

  get EditForm(): FormGroup {
    // campi non vuoti (non bastano gli spazi) e indirizzo email plausibile
    return this.fb.group({
      nome: ['', [Validators.required, Validators.pattern(/\S/)]],
      email: ['', [Validators.required, Validators.pattern(/^\s*\S+@\S+\.\S+\s*$/)]],
      messaggio: ['', [Validators.required, Validators.pattern(/\S/)]]
    });
  }

  override ButtonCancelClickEv = (param: object) => {
    this.navCtrl.navigateRoot('/home');
  }

  LoadData = (): Observable<ShowcaseContactModel> => {
    // rientrando nella pagina resta quanto gia' scritto
    return of(Object.assign(new ShowcaseContactModel(), this._editForm.value));
  };

  SaveData = (editModel: ShowcaseContactModel): Observable<boolean> => {
    const nome = editModel.nome.trim();
    const oggetto = this.transloco.translate('showcase.contactPage.mailSubject', { name: nome });
    const testo = `${editModel.messaggio.trim()}\n\n${nome} - ${editModel.email.trim()}`;
    window.location.href = `mailto:${this.c.email}?subject=${encodeURIComponent(oggetto)}&body=${encodeURIComponent(testo)}`;
    return of(true);
  };

  // il messaggio si completa nel programma di posta: si resta sulla pagina
  override AfterSave(saved: boolean): void { }

  public get c(): ShowcaseContenuti['contact'] {
    return this.showcaseService.Contenuti.contact;
  }
}

import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NotificationPreferenceGetInModel, NotificationPreferenceModel, NotificationPreferencePutInModel } from '../../../ClientServer-Service/Infrastructure/Notification/Models/notification-model';
import { NotificationService } from '../../../ClientServer-Service/Infrastructure/Notification/notification.service';

// dati della pagina: preferenze per categoria e invio email configurato sul server
export class NotificationPreferenceEditModel {
  public preference: NotificationPreferenceModel[] = [];
  public emailConfigured = false;
}

/*
 Preferenze delle notifiche dell'utente collegato (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.4): per ogni categoria
 (quelle delle funzioni base e degli applicativi attivi, titoli tradotti dal server) si sceglie se riceverle nell'app e per email.
 L'email non si puo' scegliere se la categoria non la prevede o se l'invio delle email non e' configurato sul server.
*/
@Component({
  selector: 'app-notification-preference-page',
  templateUrl: './notification-preference-page.component.html',
  styleUrls: ['./notification-preference-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class NotificationPreferencePageComponent extends BasePageConfirmCancelComponent<NotificationPreferenceEditModel> {

  // categorie come arrivate dal server (titolo, email ammessa): il salvataggio del form base sostituisce
  // le righe del modello con i soli valori del form
  public categorie: NotificationPreferenceModel[] = [];

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private notificationService: NotificationService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('pages.notificationPreference.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      preference: this.fb.array([]),
    });
  }

  get preferenceArray(): FormArray {
    return this._editForm.get('preference') as FormArray;
  }

  LoadData = (): Observable<NotificationPreferenceEditModel | null> => {
    return this.notificationService.NotificationPreferenceGet(new GenericRequest<NotificationPreferenceGetInModel>(NotificationPreferenceGetInModel)).pipe(
      map(res => {
        if (!res.success || !res.data)
          return null;
        const model = new NotificationPreferenceEditModel();
        model.preference = res.data.preference ?? [];
        model.emailConfigured = res.data.emailConfigured;
        this.categorie = [...model.preference];
        this.costruisciRighe(model);
        return model;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );
  };

  // una riga del form per categoria (il form base riempie solo i controlli esistenti)
  private costruisciRighe(model: NotificationPreferenceEditModel): void {
    this.preferenceArray.clear();
    for (const p of model.preference) {
      this.preferenceArray.push(this.fb.group({
        categoria: [p.categoria],
        app: [p.app],
        email: [{ value: p.email, disabled: !p.emailAllowed || !model.emailConfigured }],
      }));
    }
  }

  SaveData = (editModel: NotificationPreferenceEditModel): Observable<boolean> => {
    // valori grezzi: l'email disabilitata resta quella salvata
    const righe: { categoria: string, app: boolean, email: boolean }[] = this._editForm.getRawValue().preference;
    const request = new GenericRequest<NotificationPreferencePutInModel>(NotificationPreferencePutInModel);
    request.data.preference = this.categorie.map(p => {
      const r = righe.find(x => x.categoria === p.categoria);
      return { ...p, app: r ? !!r.app : p.app, email: r ? !!r.email : p.email };
    });

    return this.notificationService.NotificationPreferencePut(request).pipe(
      map(res => res.success),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  // in caso di errore si resta sulla pagina
  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }

  // titolo della categoria della riga i
  titolo(i: number): string {
    return this.categorie[i]?.titolo ?? '';
  }

  emailAllowed(i: number): boolean {
    return !!this.categorie[i]?.emailAllowed;
  }
}

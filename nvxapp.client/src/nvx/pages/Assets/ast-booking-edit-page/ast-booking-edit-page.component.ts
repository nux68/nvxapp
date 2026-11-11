import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../ClientServer-Service/Assets/ast.service';
import * as P from '../../../ClientServer-Service/Assets/Models/ast-prenotazioni-model';
import { astDataOraLocale, astPerInputDataOra } from '../../../Utility/Assets/ast-format.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-ast-booking-edit-page',
  templateUrl: './ast-booking-edit-page.component.html',
  styleUrls: ['./ast-booking-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Prenotazione (AST-PREN.2): risorsa, inizio e fine; il server rifiuta le sovrapposizioni con altre prenotazioni della risorsa
export class AstBookingEditPageComponent extends BasePageConfirmCancelComponent<P.AstPrenotazioneModel> {

  private item: P.AstPrenotazioneModel | null = null;
  private idRisorsa: number | null = null;
  public risorse: P.AstRisorsaModel[] = [];
  public durataMaxOre = 0;
  public anticipoMaxGiorni = 0;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: AstService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    const st = history.state ?? {};
    this.item = st.item ?? null;
    this.idRisorsa = st.idRisorsa ?? null;
    this.risorse = st.risorse ?? [];
    this.durataMaxOre = st.durataMaxOre ?? 0;
    this.anticipoMaxGiorni = st.anticipoMaxGiorni ?? 0;
  }

  get Title(): string { return this.transloco.translate(this.item?.id ? 'ast.pages.bookingEdit.title' : 'ast.pages.bookingEdit.titleNew'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      idAst_Risorsa: [null, [Validators.required]],
      inizio: [null, [Validators.required]],
      fine: [null, [Validators.required]],
      titolo: [null, [Validators.maxLength(200)]],
      note: [null, [Validators.maxLength(500)]],
    });
  }

  LoadData = (): Observable<P.AstPrenotazioneModel> => {
    if (this.item)
      return of({ ...this.item, inizio: astPerInputDataOra(this.item.inizio), fine: astPerInputDataOra(this.item.fine) });
    // nuova: dalla prossima ora piena, per un'ora
    const d = new Date();
    d.setMinutes(0, 0, 0);
    d.setHours(d.getHours() + 1);
    const f = new Date(d.getTime() + 3600000);
    return of({ ...new P.AstPrenotazioneModel(), idAst_Risorsa: this.idRisorsa ?? (null as any), inizio: astDataOraLocale(d), fine: astDataOraLocale(f) });
  };

  SaveData = (m: P.AstPrenotazioneModel): Observable<boolean> => {
    const request = new GenericRequest<P.AstPrenotazionePutInModel>(P.AstPrenotazionePutInModel);
    request.data.prenotazione = { ...new P.AstPrenotazioneModel(), id: this.item?.id ?? 0, idAst_Risorsa: m.idAst_Risorsa,
                                  inizio: m.inizio, fine: m.fine, titolo: m.titolo, note: m.note };
    return this.service.PrenotazionePut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }
}

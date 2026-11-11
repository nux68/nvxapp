import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../ClientServer-Service/Assets/ast.service';
import * as F from '../../../ClientServer-Service/Assets/Models/ast-flotta-model';
import * as P from '../../../ClientServer-Service/Assets/Models/ast-prenotazioni-model';
import { astNumero } from '../../../Utility/Assets/ast-format.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-ast-resource-edit-page',
  templateUrl: './ast-resource-edit-page.component.html',
  styleUrls: ['./ast-resource-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Risorsa prenotabile (CompanyPowerAdmin): tipo, descrizione, sede; capienza per le sale, veicolo in pool per le auto
export class AstResourceEditPageComponent extends BasePageConfirmCancelComponent<P.AstRisorsaModel> {

  private item: P.AstRisorsaModel | null = null;
  public sedi: F.AstLookupModel[] = [];
  public veicoli: F.AstLookupModel[] = [];
  public readonly tipi = [0, 1, 2];

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: AstService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    const st = history.state ?? {};
    this.item = st.item ?? null;
    this.sedi = st.sedi ?? [];
    this.veicoli = st.veicoli ?? [];
  }

  get Title(): string { return this.transloco.translate(this.item?.id ? 'ast.pages.resourceEdit.title' : 'ast.pages.resourceEdit.titleNew'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      tipo: [0],
      descrizione: [null, [Validators.required, Validators.maxLength(100)]],
      idAz_Sedi: [null],
      capienza: [null],
      idAst_Veicolo: [null],
      attiva: [true],
      note: [null, [Validators.maxLength(500)]],
    });
  }

  LoadData = (): Observable<P.AstRisorsaModel> => of({ ...new P.AstRisorsaModel(), ...(this.item ?? {}) });

  SaveData = (m: P.AstRisorsaModel): Observable<boolean> => {
    const request = new GenericRequest<P.AstRisorsaPutInModel>(P.AstRisorsaPutInModel);
    request.data.risorsa = { ...m, id: this.item?.id ?? 0, capienza: astNumero(m.capienza) };
    return this.service.RisorsaPut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }
}

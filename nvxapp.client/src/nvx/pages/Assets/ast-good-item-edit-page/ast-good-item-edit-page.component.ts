import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../ClientServer-Service/Assets/ast.service';
import * as F from '../../../ClientServer-Service/Assets/Models/ast-flotta-model';
import * as B from '../../../ClientServer-Service/Assets/Models/ast-beni-model';
import { astNumero, astOggi } from '../../../Utility/Assets/ast-format.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

export type AstGoodItemKind = 'categoria' | 'consegna' | 'restituzione';

@Component({
  selector: 'app-ast-good-item-edit-page',
  templateUrl: './ast-good-item-edit-page.component.html',
  styleUrls: ['./ast-good-item-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Beni (AST-BENI): categoria, consegna di un bene a un dipendente o restituzione. Consegna e restituzione chiedono al dipendente
 la presa visione del verbale (con il documentale attivo diventa il verbale di consegna da firmare).
*/
export class AstGoodItemEditPageComponent extends BasePageConfirmCancelComponent<any> {

  public kind: AstGoodItemKind = 'categoria';
  public bene: B.AstBeneModel | null = null;
  public dipendenti: F.AstDipendenteModel[] = [];
  private item: any = null;
  public readonly statiRientro = [0, 2, 3];

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: AstService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    const st = history.state ?? {};
    this.kind = st.kind ?? 'categoria';
    this.item = st.item;
    this.bene = st.bene ?? null;
    // si consegna a dipendenti in forza
    this.dipendenti = (st.dipendenti ?? []).filter((d: F.AstDipendenteModel) => d.attivo && !d.dataCessazione);
  }

  get Title(): string {
    const t = this.transloco.translate('ast.pages.goodItem.' + this.kind + (this.kind === 'categoria' && !this.item?.id ? 'New' : ''));
    return this.bene ? t + ' - ' + this.bene.codiceInventario : t;
  }

  get EditForm(): FormGroup {
    const req = (k: AstGoodItemKind) => this.kind === k ? [Validators.required] : [];
    return this.fb.group({
      descrizione: [null, [...req('categoria'), Validators.maxLength(100)]],
      attivo: [true],
      ordine: [0],
      idDip_Anagrafica: [null, req('consegna')],
      data: [null, this.kind !== 'categoria' ? [Validators.required] : []],
      condizioni: [null, [Validators.maxLength(500)]],
      note: [null, [Validators.maxLength(500)]],
      statoBene: [0],
    });
  }

  LoadData = (): Observable<any> => {
    const x = this.item ?? {};
    switch (this.kind) {
      case 'categoria': return of({ ...new B.AstCategoriaModel(), ...x });
      case 'consegna': return of({ data: astOggi(), idDip_Anagrafica: null, condizioni: null, note: null });
      default: return of({ data: astOggi(), condizioni: null, statoBene: 0 });
    }
  };

  SaveData = (m: any): Observable<boolean> => {
    const ok = (o: Observable<{ success: boolean }>) => o.pipe(map(res => res.success), catchError(() => of(false)));
    switch (this.kind) {
      case 'categoria': {
        const r = new GenericRequest<B.AstCategoriaPutInModel>(B.AstCategoriaPutInModel);
        r.data.categoria = { ...new B.AstCategoriaModel(), id: this.item?.id ?? 0, descrizione: m.descrizione, attivo: !!m.attivo, ordine: astNumero(m.ordine) ?? 0 };
        return ok(this.service.CategoriaPut(r));
      }
      case 'consegna': {
        const r = new GenericRequest<B.AstConsegnaPutInModel>(B.AstConsegnaPutInModel);
        r.data = { idAst_Bene: this.bene?.id ?? 0, idDip_Anagrafica: m.idDip_Anagrafica, dataConsegna: m.data, condizioniConsegna: m.condizioni, note: m.note };
        return ok(this.service.ConsegnaPut(r));
      }
      default: {
        const r = new GenericRequest<B.AstRestituzionePutInModel>(B.AstRestituzionePutInModel);
        r.data = { idAst_BeneAssegnazione: this.item?.id ?? 0, dataRestituzione: m.data, condizioniRestituzione: m.condizioni, statoBene: m.statoBene };
        return ok(this.service.RestituzionePut(r));
      }
    }
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }
}

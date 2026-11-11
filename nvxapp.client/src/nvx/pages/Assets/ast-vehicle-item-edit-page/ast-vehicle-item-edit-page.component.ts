import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../ClientServer-Service/Assets/ast.service';
import * as F from '../../../ClientServer-Service/Assets/Models/ast-flotta-model';
import { astDataOraLocale, astNumero, astOggi, astPerInputData, astPerInputDataOra } from '../../../Utility/Assets/ast-format.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { AstVehicleItemKind } from '../ast-vehicle-edit-page/ast-vehicle-edit-page.component';

@Component({
  selector: 'app-ast-vehicle-item-edit-page',
  templateUrl: './ast-vehicle-item-edit-page.component.html',
  styleUrls: ['./ast-vehicle-item-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Riga di un veicolo (AST-FLO): assegnazione a un dipendente (fringe benefit e rimborso km dell'autovettura in Common),
 scadenza (nello scadenzario), costo o multa. I campi dipendono dal tipo, passato nello stato della navigazione.
*/
export class AstVehicleItemEditPageComponent extends BasePageConfirmCancelComponent<any> {

  public kind: AstVehicleItemKind = 'assegnazione';
  public veicolo: { id: number; targa: string; emissioniCO2: number | null } = { id: 0, targa: '', emissioniCO2: null };
  public dipendenti: F.AstDipendenteModel[] = [];
  private item: any = null;
  public readonly tipiScadenza = [0, 1, 2, 3, 4];
  public readonly tipiCosto = [0, 1, 2, 3, 4, 5, 6];
  public readonly statiMulta = [0, 1, 2, 3];

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: AstService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    const st = history.state ?? {};
    this.kind = st.kind ?? 'assegnazione';
    this.veicolo = st.veicolo ?? this.veicolo;
    this.dipendenti = (st.dipendenti ?? []).filter((d: F.AstDipendenteModel) => d.attivo || d.id === st.item?.idDip_Anagrafica);
    this.item = st.item;
  }

  get Title(): string {
    return this.transloco.translate('ast.pages.vehicleItem.' + this.kind + (this.item?.id ? '' : 'New')) + ' - ' + this.veicolo.targa;
  }

  get EditForm(): FormGroup {
    const req = (k: AstVehicleItemKind) => this.kind === k ? [Validators.required] : [];
    return this.fb.group({
      // assegnazione
      idDip_Anagrafica: [null, req('assegnazione')],
      dataInizio: [null, req('assegnazione')],
      dataFine: [null],
      usoPromiscuo: [true],
      percentualeFringe: [0],
      rimborsoKm: [0],
      // scadenza
      tipo: [0],
      descrizione: [null, [Validators.maxLength(500)]],
      dataScadenza: [null, req('scadenza')],
      km: [null],
      importo: [null],
      dataEsecuzione: [null],
      // costo
      data: [null, req('costo')],
      documento: [null, [Validators.maxLength(50)]],
      // multa
      dataInfrazione: [null, req('multa')],
      numeroVerbale: [null, [Validators.maxLength(50)]],
      luogo: [null, [Validators.maxLength(200)]],
      idConducente: [null],
      stato: [0],
      dataPagamento: [null],
      note: [null, [Validators.maxLength(500)]],
    });
  }

  LoadData = (): Observable<any> => {
    const x = this.item ?? {};
    switch (this.kind) {
      case 'assegnazione':
        return of({ ...new F.AstVeicoloAssegnazioneModel(), dataInizio: astOggi(), ...x,
                    ...(x.dataInizio ? { dataInizio: astPerInputData(x.dataInizio) } : {}), dataFine: astPerInputData(x.dataFine) });
      case 'scadenza':
        return of({ ...new F.AstVeicoloScadenzaModel(), ...x, dataScadenza: astPerInputData(x.dataScadenza), dataEsecuzione: astPerInputData(x.dataEsecuzione) });
      case 'costo':
        return of({ ...new F.AstVeicoloCostoModel(), data: astOggi(), ...x, ...(x.data ? { data: astPerInputData(x.data) } : {}) });
      default:
        return of({ ...new F.AstMultaModel(), dataInfrazione: astDataOraLocale(new Date()), ...x,
                    ...(x.dataInfrazione ? { dataInfrazione: astPerInputDataOra(x.dataInfrazione) } : {}),
                    idConducente: x.idDip_Anagrafica ?? null, dataPagamento: astPerInputData(x.dataPagamento) });
    }
  };

  SaveData = (m: any): Observable<boolean> => {
    const id = this.item?.id ?? 0;
    const idAst_Veicolo = this.veicolo.id;
    const ok = (o: Observable<{ success: boolean }>) => o.pipe(map(res => res.success), catchError(() => of(false)));
    switch (this.kind) {
      case 'assegnazione': {
        const r = new GenericRequest<F.AstAssegnazionePutInModel>(F.AstAssegnazionePutInModel);
        r.data.assegnazione = { ...new F.AstVeicoloAssegnazioneModel(), id, idAst_Veicolo, idDip_Anagrafica: m.idDip_Anagrafica,
                                dataInizio: m.dataInizio, dataFine: m.dataFine || null, usoPromiscuo: !!m.usoPromiscuo,
                                percentualeFringe: astNumero(m.percentualeFringe) ?? 0, rimborsoKm: astNumero(m.rimborsoKm) ?? 0,
                                idDip_RapportoAutovetture: this.item?.idDip_RapportoAutovetture ?? null, note: m.note };
        return ok(this.service.AssegnazionePut(r));
      }
      case 'scadenza': {
        const r = new GenericRequest<F.AstVeicoloScadenzaPutInModel>(F.AstVeicoloScadenzaPutInModel);
        r.data.scadenza = { id, idAst_Veicolo, tipo: m.tipo, descrizione: m.descrizione, dataScadenza: m.dataScadenza, km: astNumero(m.km),
                            importo: astNumero(m.importo), dataEsecuzione: m.dataEsecuzione || null, note: m.note };
        return ok(this.service.VeicoloScadenzaPut(r));
      }
      case 'costo': {
        const r = new GenericRequest<F.AstVeicoloCostoPutInModel>(F.AstVeicoloCostoPutInModel);
        r.data.costo = { id, idAst_Veicolo, data: m.data, tipo: m.tipo, importo: astNumero(m.importo) ?? 0, km: astNumero(m.km),
                         descrizione: m.descrizione, documento: m.documento };
        return ok(this.service.VeicoloCostoPut(r));
      }
      default: {
        const r = new GenericRequest<F.AstMultaPutInModel>(F.AstMultaPutInModel);
        r.data.multa = { ...new F.AstMultaModel(), id, idAst_Veicolo, dataInfrazione: m.dataInfrazione, numeroVerbale: m.numeroVerbale,
                         luogo: m.luogo, descrizione: m.descrizione, importo: astNumero(m.importo) ?? 0, idDip_Anagrafica: m.idConducente ?? null,
                         stato: m.stato, dataPagamento: m.dataPagamento || null, note: m.note };
        return ok(this.service.MultaPut(r));
      }
    }
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }
}

import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ModalController, NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService, hrData, hrOggi, hrPerInput } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { CommonOrganizzazioneService } from '../../../ClientServer-Service/Infrastructure/Common/common-organizzazione.service';
import { Az_SediCommonModel, CommonOrganizzazione_SediGetAll_InModel } from '../../../ClientServer-Service/Infrastructure/Common/Models/common-organizzazione-model';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { hrScegliDipendenti, hrScegliFile } from '../hr-ui';

@Component({
  selector: 'app-hr-course-edition-page',
  templateUrl: './hr-course-edition-page.component.html',
  styleUrls: ['./hr-course-edition-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Edizione di un corso (HR-FOR.1-2): date, sede (comune), docente e partecipanti con esito e attestato. Con l'esito "superato"
 il server calcola la scadenza (validita' del corso), la registra nello scadenzario e aggiunge la competenza al dipendente.
 Gli attestati si caricano per i partecipanti gia' salvati.
*/
export class HrCourseEditionPageComponent extends BasePageConfirmCancelComponent<M.HrEdizioneModel> {

  public corso = '';
  public validitaMesi: number | null = null;
  public sedi: Az_SediCommonModel[] = [];
  public partecipanti: M.HrPartecipanteModel[] = [];
  public readonly esiti = [0, 1, 2, 3];
  public readonly data = hrData;
  private dipendenti: M.HrDipendenteBreveModel[] = [];
  private id = 0;
  private idCorso = 0;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: HrService,
              private organizzazione: CommonOrganizzazioneService,
              private modalCtrl: ModalController,
              public fabMenuService: FabMenuService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('hr.pages.courseEdition.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      dataInizio: [hrOggi(), [Validators.required]],
      dataFine: [null],
      idAz_Sedi: [null],
      luogo: [null, [Validators.maxLength(200)]],
      docente: [null, [Validators.maxLength(200)]],
      note: [null, [Validators.maxLength(2000)]],
    });
  }

  override ionViewWillEnter() {
    this.organizzazione.SediGetAll(new GenericRequest(CommonOrganizzazione_SediGetAll_InModel)).subscribe(res => this.sedi = res.success ? res.data.az_Sedi : []);
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('hr.pages.courseEdition.addParticipants'), 'person-add-outline', () => this.aggiungi()),
    ];
    super.ionViewWillEnter();
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  LoadData = (): Observable<M.HrEdizioneModel | null> => {
    this.id = this.id || history.state?.id || 0;
    this.idCorso = this.idCorso || history.state?.idHr_Corso || 0;
    const request = new GenericRequest<M.HrEdizioneGetInModel>(M.HrEdizioneGetInModel);
    request.data = { id: this.id, idHr_Corso: this.idCorso };
    return this.service.EdizioneGet(request).pipe(map(res => res.success ? this.mostra(res.data) : null), catchError(() => of(null)));
  };

  private mostra(d: M.HrEdizioneGetOutModel): M.HrEdizioneModel {
    this.corso = d.corso;
    this.validitaMesi = d.validitaMesi;
    this.dipendenti = d.dipendenti;
    this.idCorso = d.edizione.idHr_Corso;
    this.partecipanti = d.edizione.partecipanti.map(p => ({ ...p, dataConseguimento: hrPerInput(p.dataConseguimento) }));
    return { ...d.edizione, dataInizio: hrPerInput(d.edizione.dataInizio) ?? hrOggi(), dataFine: hrPerInput(d.edizione.dataFine) };
  }

  async aggiungi() {
    const scelti = await hrScegliDipendenti(this.modalCtrl, this.dipendenti.filter(d => d.attivo), 'multi');
    for (const id of scelti ?? []) {
      if (this.partecipanti.some(p => p.idDip_Anagrafica === id)) continue;
      const d = this.dipendenti.find(x => x.id === id);
      this.partecipanti = [...this.partecipanti, Object.assign(new M.HrPartecipanteModel(), { idDip_Anagrafica: id, dipendente: `${d?.cognome ?? ''} ${d?.nome ?? ''}`.trim() })];
    }
  }

  togli(p: M.HrPartecipanteModel) {
    this.partecipanti = this.partecipanti.filter(x => x !== p);
  }

  async attestato(p: M.HrPartecipanteModel) {
    const file = await hrScegliFile('application/pdf,image/*,.pdf');
    if (!file) return;
    this.service.AttestatoUpload({ idPartecipante: p.id }, file).subscribe(res => { if (res.success) p.attestatoNomeFile = res.data.partecipante.attestatoNomeFile; });
  }

  scaricaAttestato(p: M.HrPartecipanteModel) {
    this.service.AttestatoDownload(p.id);
  }

  SaveData = (editModel: M.HrEdizioneModel): Observable<boolean> => {
    const request = new GenericRequest<M.HrEdizionePutInModel>(M.HrEdizionePutInModel);
    request.data.edizione = {
      ...editModel, id: this.id, idHr_Corso: this.idCorso, dataFine: editModel.dataFine || null,
      partecipanti: this.partecipanti.map(p => ({ ...p, esito: Number(p.esito), dataConseguimento: p.esito == M.HrEsitoCorso.Superato ? (p.dataConseguimento || null) : null }))
    };
    return this.service.EdizionePut(request).pipe(map(res => {
      if (res.success) {
        this.id = res.data.edizione.id;
        this.partecipanti = res.data.edizione.partecipanti.map(p => ({ ...p, dataConseguimento: hrPerInput(p.dataConseguimento) }));
      }
      return res.success;
    }), catchError(() => of(false)));
  };

  // si resta sulla pagina: dopo il salvataggio si vedono le scadenze calcolate e si possono caricare gli attestati
  override AfterSave(saved: boolean): void { }
}

import { Component, ChangeDetectionStrategy } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError, of } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { Az_ClienteGetInModel, Az_ClienteModel, Az_ClientePutInModel, TipoCliente } from '../../../ClientServer-Service/Infrastructure/Az_Cliente/Models/az-cliente-model';
import { AzClienteService } from '../../../ClientServer-Service/Infrastructure/Az_Cliente/az-cliente.service';
import { Az_ClienteContatto_GetAll_InModel, Az_ClienteContattoDeleteInModel, Az_ClienteContattoModel } from '../../../ClientServer-Service/Infrastructure/Az_ClienteContatto/Models/az-cliente-contatto-model';
import { AzClienteContattoService } from '../../../ClientServer-Service/Infrastructure/Az_ClienteContatto/az-cliente-contatto.service';
import { CommonDataEventsService } from '../../../Utility/infrastructure/common-data-events.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { CustomerContactDialogComponent } from './customer-contact-dialog/customer-contact-dialog.component';
import { ModalController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';

@Component({
  selector: 'app-customer-edit-page',
  templateUrl: './customer-edit-page.component.html',
  styleUrls: ['./customer-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CustomerEditPageComponent extends BasePageConfirmCancelComponent<Az_ClienteModel> {

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private stringHelperService: StringHelperService,
    private azClienteService: AzClienteService,
    private azClienteContattoService: AzClienteContattoService,
    private commonDataEventsService: CommonDataEventsService,
    private collectionDialogService: CollectionDialogService,
    private modalCtrl: ModalController,
    private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    const t = (k: string) => this.transloco.translate(k);
    this.colonneContatti = [
      { key: 'nominativo', title: t('components.customerContact.nominativo'), mobile: 'title',
        value: x => [x.cognome, x.nome].filter(v => !!v).join(' ') + (x.principale ? ' (' + t('components.customerContact.principale') + ')' : '') },
      { key: 'ruolo', title: t('components.customerContact.ruolo'), mobile: 'subtitle' },
      { key: 'telefono', title: t('components.customerContact.telefono'), value: x => x.cellulare || x.telefono || '' },
      { key: 'email', title: t('components.customerContact.email'), mobile: 'note' },
    ];
    const btnEdit = userInterfaceService.Btn_Modifica;
    btnEdit.event = (x: Az_ClienteContattoModel) => this.editContatto(x);
    const btnDelete = userInterfaceService.Btn_Cancella;
    btnDelete.event = (x: Az_ClienteContattoModel) => this.deleteContatto(x);
    this.azioniContatti = [{ button: btnDelete }, { button: btnEdit }];
  }

  public readonly tipi = [TipoCliente.Cliente, TipoCliente.Fornitore, TipoCliente.Entrambi];
  public contatti: Az_ClienteContattoModel[] | null = null;
  public readonly colonneContatti: NvxListColumn<Az_ClienteContattoModel>[];
  public readonly azioniContatti: NvxListAction<Az_ClienteContattoModel>[];
  public readonly principale = (x: Az_ClienteContattoModel) => x.principale;

  get Title(): string { return this.transloco.translate('pages.customerEdit.title'); }
  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(100)]],
      tipo: [TipoCliente.Cliente],
      attivo: [true],

      partitaIva: [null, [Validators.pattern(/^\d{11}$/)]],
      codiceFiscale: [null, [Validators.pattern(/^([A-Za-z0-9]{16}|\d{11})$/)]],

      indirizzo: [null, [Validators.maxLength(100)]],
      cap: [null, [Validators.maxLength(10)]],
      citta: [null, [Validators.maxLength(50)]],
      provincia: [null, [Validators.pattern(/^[A-Za-z]{2}$/)]],
      nazione: [null, [Validators.pattern(/^[A-Za-z]{2}$/)]],

      telefono: [null, [Validators.maxLength(30)]],
      email: [null, [Validators.email, Validators.maxLength(100)]],
      pec: [null, [Validators.email, Validators.maxLength(100)]],
      referente: [null, [Validators.maxLength(100)]],

      codiceDestinatario: [null, [Validators.pattern(/^[A-Za-z0-9]{6,7}$/)]],

      note: [null, [Validators.maxLength(500)]]
    });
  }

  LoadData = (): Observable<Az_ClienteModel | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Az_ClienteGetInModel> = new GenericRequest<Az_ClienteGetInModel>(Az_ClienteGetInModel);
      request.data.id = state.id;
      this.caricaContatti(state.id);
      return this.azClienteService.Az_ClienteGet(request).pipe(
        map((res) => res.data.az_Cliente),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return of(null);
        })
      );
    } else {
      return new Observable<Az_ClienteModel | null>((subscriber) => {
        subscriber.next(new Az_ClienteModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Az_ClienteModel): Observable<boolean> => {
    let request: GenericRequest<Az_ClientePutInModel> = new GenericRequest<Az_ClientePutInModel>(Az_ClientePutInModel);
    // codici in maiuscolo come da standard
    for (const campo of ['codiceFiscale', 'provincia', 'nazione', 'codiceDestinatario'] as const)
      if (editModel[campo]) editModel[campo] = editModel[campo].toUpperCase();
    request.data.az_Cliente = editModel;
    return this.azClienteService.Az_ClientePut(request).pipe(
      map(res => {
        if (!res.success)
          return false;
        this.commonDataEventsService.ClientiModificati();
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false];
      })
    );
  };

  // ---------------------------------------------------------------- persone di riferimento
  caricaContatti(idAz_Cliente: number) {
    const request = new GenericRequest<Az_ClienteContatto_GetAll_InModel>(Az_ClienteContatto_GetAll_InModel);
    request.data.idAz_Cliente = idAz_Cliente;
    this.azClienteContattoService.GetAll(request).subscribe(res => this.contatti = res.success && res.data ? res.data.az_ClienteContatto : []);
  }

  async editContatto(item: Az_ClienteContattoModel | null) {
    const contatto = item ? { ...item } : Object.assign(new Az_ClienteContattoModel(), { idAz_Cliente: this._editModel.id });
    const modal = await this.modalCtrl.create({ component: CustomerContactDialogComponent, componentProps: { contatto } });
    await modal.present();
    const { role } = await modal.onDidDismiss();
    if (role === 'confirm') {
      this.caricaContatti(this._editModel.id);
      this.commonDataEventsService.ClientiModificati();
    }
  }

  private async deleteContatto(item: Az_ClienteContattoModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('components.customerContact.confirmDelete')))
      return;
    const request = new GenericRequest<Az_ClienteContattoDeleteInModel>(Az_ClienteContattoDeleteInModel);
    request.data.id = item.id;
    this.azClienteContattoService.Delete(request).subscribe(res => {
      if (res.success) {
        this.caricaContatti(this._editModel.id);
        this.commonDataEventsService.ClientiModificati();
      }
    });
  }
}

const matchData: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  return null;
};

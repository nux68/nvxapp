import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService, hrData } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { AcknowledgementService } from '../../../ClientServer-Service/Infrastructure/Acknowledgement/acknowledgement.service';
import { AcknowledgementConfirmInModel } from '../../../ClientServer-Service/Infrastructure/Acknowledgement/Models/acknowledgement-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { hrPeriodo } from '../hr-payslip-list-page/hr-payslip-list-page.component';

@Component({
  selector: 'app-hr-my-payslips-page',
  templateUrl: './hr-my-payslips-page.component.html',
  styleUrls: ['./hr-my-payslips-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Area del dipendente (HR-CED.3-4): i propri cedolini e CU pubblicati. Il PDF si scarica; la presa visione si conferma qui
 (canale base, servizio comune delle prese visione) oppure, con il documentale attivo, firmando il documento dalla pagina "Prese visione".
*/
export class HrMyPayslipsPageComponent {

  public title: string;
  public searchText!: string;
  public items: M.HrMioDocumentoModel[] | null = null;
  public dipendente = true;
  public readonly columns: NvxListColumn<M.HrMioDocumentoModel>[];
  public readonly actions: NvxListAction<M.HrMioDocumentoModel>[];
  public readonly daConfermare = (x: M.HrMioDocumentoModel) => x.idPresaVisione != null && !x.dataConferma;

  constructor(private userInterfaceService: UserInterfaceService,
              private service: HrService,
              private acknowledgementService: AcknowledgementService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('hr.pages.myPayslips.title');
    this.columns = [
      { key: 'periodo', title: t('hr.fields.periodo'), mobile: 'title', value: x => hrPeriodo(this.transloco, x.tipo, x.anno, x.mese), sortValue: x => x.anno * 100 + (x.mese ?? 0) },
      { key: 'tipo', title: t('hr.fields.tipoDocumento'), mobile: 'hidden', value: x => t('hr.enums.tipoDocumento.' + x.tipo) },
      { key: 'dataPubblicazione', title: t('hr.fields.dataPubblicazione'), mobile: 'note', value: x => hrData(x.dataPubblicazione), sortValue: x => x.dataPubblicazione ?? '' },
      { key: 'presaVisione', title: t('hr.fields.presaVisione'), mobile: 'subtitle',
        value: x => x.dataConferma ? t('hr.pages.payslipDetail.confirmed') + ' ' + hrData(x.dataConferma) : (x.idPresaVisione ? t('hr.pages.myPayslips.toConfirm') : '') },
    ];
    const bPdf = userInterfaceService.Btn_Pdf;
    bPdf.event = (x: M.HrMioDocumentoModel) => this.service.MioDocDownload(x.id);
    const bConferma = userInterfaceService.Btn_ConfermaLettura;
    bConferma.event = (x: M.HrMioDocumentoModel) => this.conferma(x);
    this.actions = [{ button: bConferma, visible: x => x.confermabileQui }, { button: bPdf }];
  }

  ionViewWillEnter() {
    this.load();
  }

  load() {
    this.service.MieiDocumenti(new GenericRequest<M.HrEmptyInModel>(M.HrEmptyInModel)).subscribe(res => {
      this.dipendente = res.success ? res.data.dipendente : true;
      this.items = res.success && res.data ? res.data.documenti : [];
    });
  }

  conferma(x: M.HrMioDocumentoModel) {
    const request = new GenericRequest<AcknowledgementConfirmInModel>(AcknowledgementConfirmInModel);
    request.data.id = x.idPresaVisione!;
    this.acknowledgementService.AcknowledgementConfirm(request).subscribe(res => { if (res.success) this.load(); });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}

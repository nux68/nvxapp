import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../ClientServer-Service/Assets/ast.service';
import { AstEmptyInModel } from '../../../ClientServer-Service/Assets/Models/ast-model';
import * as F from '../../../ClientServer-Service/Assets/Models/ast-flotta-model';
import * as B from '../../../ClientServer-Service/Assets/Models/ast-beni-model';
import { AstFormatService } from '../../../Utility/Assets/ast-format.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';

@Component({
  selector: 'app-ast-mine-page',
  templateUrl: './ast-mine-page.component.html',
  styleUrls: ['./ast-mine-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Le mie risorse (utente): beni in consegna con i verbali da confermare, veicolo assegnato con il fringe benefit e multe notificate
export class AstMinePageComponent {

  public title: string;
  public veicoli: F.AstMioVeicoloModel[] | null = null;
  public multe: F.AstMultaModel[] | null = null;
  public beni: B.AstBeneAssegnazioneModel[] | null = null;
  public readonly colBene: NvxListColumn<B.AstBeneAssegnazioneModel>[];
  public readonly actBene: NvxListAction<B.AstBeneAssegnazioneModel>[];
  public readonly beneRestituito = (x: B.AstBeneAssegnazioneModel) => !!x.dataRestituzione;
  public readonly verbaleDaConfermare = (x: B.AstBeneAssegnazioneModel) => (x.verbaleConsegna.richiesto && !x.verbaleConsegna.dataConferma)
                                                                          || (x.verbaleRestituzione.richiesto && !x.verbaleRestituzione.dataConferma);
  public readonly colVeicolo: NvxListColumn<F.AstMioVeicoloModel>[];
  public readonly colMulta: NvxListColumn<F.AstMultaModel>[];

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              userInterfaceService: UserInterfaceService,
              private service: AstService,
              private fmt: AstFormatService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('ast.pages.mine.title');
    this.colVeicolo = [
      { key: 'descrizione', title: t('ast.fields.veicolo'), mobile: 'title' },
      { key: 'dataInizio', title: t('ast.fields.dal'), value: x => this.fmt.data(x.dataInizio), sortValue: x => x.dataInizio },
      { key: 'dataFine', title: t('ast.fields.al'), value: x => this.fmt.data(x.dataFine), sortValue: x => x.dataFine ?? '9999' },
      { key: 'fringeAnnoCorrente', title: t('ast.fields.fringeAnno'), align: 'end', mobile: 'note',
        value: x => x.usoPromiscuo ? this.fmt.importo(x.fringeAnnoCorrente) : t('ast.pages.vehicleEdit.noFringe') },
    ];
    this.colBene = [
      { key: 'bene', title: t('ast.fields.bene'), mobile: 'title', value: x => (x.codiceInventario ?? '') + ' - ' + (x.bene ?? '') },
      { key: 'dataConsegna', title: t('ast.fields.dataConsegna'), value: x => this.fmt.data(x.dataConsegna), sortValue: x => x.dataConsegna },
      { key: 'dataRestituzione', title: t('ast.fields.dataRestituzione'), value: x => this.fmt.data(x.dataRestituzione), sortValue: x => x.dataRestituzione ?? '9999' },
      { key: 'verbale', title: t('ast.fields.verbale'), mobile: 'note',
        value: x => this.verbaleDaConfermare(x) ? t('ast.pages.mine.reportToConfirm') : x.verbaleConsegna.dataConferma ? t('ast.pages.mine.reportConfirmed') : '' },
    ];
    // i verbali si confermano nella pagina delle prese visione (o si firmano dal documentale)
    const bApri = userInterfaceService.Btn_Apri;
    bApri.event = () => this.navCtrl.navigateForward('/acknowledgements');
    this.actBene = [{ button: bApri, visible: x => this.verbaleDaConfermare(x) }];
    this.colMulta = [
      { key: 'dataInfrazione', title: t('ast.fields.data'), mobile: 'note', value: x => this.fmt.dataOra(x.dataInfrazione), sortValue: x => x.dataInfrazione },
      { key: 'targa', title: t('ast.fields.targa'), mobile: 'title' },
      { key: 'luogo', title: t('ast.fields.luogo') },
      { key: 'importo', title: t('ast.fields.importo'), align: 'end', value: x => this.fmt.importo(x.importo), sortValue: x => x.importo },
      { key: 'stato', title: t('ast.fields.stato'), mobile: 'subtitle', value: x => t('ast.enums.statoMulta.' + x.stato) },
    ];
  }

  ionViewWillEnter() {
    this.fabMenuService.fabMenuItem = [];
    this.load();
  }

  load() {
    const request = new GenericRequest<AstEmptyInModel>(AstEmptyInModel);
    this.service.MieiVeicoliGet(request).subscribe(res => {
      this.veicoli = res.success && res.data ? res.data.veicolo : [];
      this.multe = res.success && res.data ? res.data.multa : [];
    });
    this.service.MieiBeniGet(request).subscribe(res => this.beni = res.success && res.data ? res.data.assegnazione : []);
  }
}

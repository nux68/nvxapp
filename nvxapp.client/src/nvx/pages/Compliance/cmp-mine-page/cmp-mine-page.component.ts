import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CmpService } from '../../../ClientServer-Service/Compliance/cmp.service';
import * as M from '../../../ClientServer-Service/Compliance/Models/cmp-model';
import { CMP_SEZIONI, CmpColonna, cmpOrdine, cmpValore } from '../../../Utility/Compliance/cmp-sezioni';
import { NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

type CmpMiaSezione = 'formazione' | 'visite' | 'consegne' | 'nomine';

/*
 "I miei adempimenti" (CMP-81, ANALISI_COMPLIANCE_APPLICATIVO.md §3): il dipendente vede i propri corsi, le visite (solo tipo,
 data, esito di idoneita' e scadenza: mai limitazioni di dettaglio o dati sanitari), i DPI ricevuti e le nomine, con lo stato
 della presa visione. La conferma si da' nella pagina comune delle prese visione (bottone in basso).
*/
@Component({
  selector: 'app-cmp-mine-page',
  templateUrl: './cmp-mine-page.component.html',
  styleUrls: ['./cmp-mine-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CmpMinePageComponent {

  public title: string;
  public searchText!: string;
  public sezione: CmpMiaSezione = 'formazione';
  public readonly sezioni: CmpMiaSezione[] = ['formazione', 'visite', 'consegne', 'nomine'];
  public dati: M.CmpMineOutModel | null = null;
  public items: M.CmpRiga[] | null = null;
  public columns: NvxListColumn<M.CmpRiga>[] = [];
  public readonly buttonbar: ButtonItem[];
  public readonly highlight = (x: M.CmpRiga) => !!x.scaduta;
  public readonly isDisabled = (x: M.CmpRiga) => !!x.rinnovata;

  // colonne mostrate al dipendente (sottoinsieme di quelle delle sezioni)
  private readonly colonne: Record<CmpMiaSezione, string[]> = {
    formazione: ['descrizione', 'tipoCorso', 'dataCorso', 'dataScadenza'],
    visite: ['tipo', 'dataVisita', 'esito', 'dataScadenza'],
    consegne: ['dpi', 'dataConsegna', 'quantita', 'taglia', 'dataPresaVisione'],
    nomine: ['tipo', 'sede', 'dataInizio', 'dataFine', 'dataPresaVisione'],
  };

  constructor(private navCtrl: NavController,
              private fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: CmpService,
              private languageService: LanguageService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('cmp.pages.mine.title');
    // pagina comune delle prese visione, dove si confermano consegne DPI e nomine
    const btnApri = userInterfaceService.Btn_Apri;
    btnApri.text = this.transloco.translate('cmp.pages.mine.openAcknowledgements');
    btnApri.event = () => this.navCtrl.navigateForward('/acknowledgements');
    this.buttonbar = [btnApri];
  }

  ionViewWillEnter() {
    this.fabMenuService.fabMenuItem = [];
    const state = history.state ?? {};
    if (state.sezione === 'dpi')
      this.sezione = 'consegne';
    else if (this.sezioni.includes(state.sezione))
      this.sezione = state.sezione;
    this.service.Mine(new GenericRequest<M.CmpListInModel>(M.CmpListInModel)).subscribe(res => {
      this.dati = res.success && res.data ? res.data : new M.CmpMineOutModel();
      this.mostra();
    });
  }

  sezioneChanged(value: any) {
    this.sezione = value as CmpMiaSezione;
    this.mostra();
  }

  mostra() {
    const locale = this.languageService.locale;
    const tutte: CmpColonna[] = CMP_SEZIONI[this.sezione].colonne;
    this.columns = this.colonne[this.sezione].map((k, i) => {
      const c = tutte.find(x => x.key === k) ?? { key: k };
      return {
        key: k, title: this.transloco.translate('cmp.fields.' + k), mobile: i === 0 ? 'title' : c.mobile === 'hidden' ? 'subtitle' : c.mobile, width: c.width,
        value: (x: M.CmpRiga) => cmpValore(c, x, this.transloco, locale),
        sortValue: (x: M.CmpRiga) => cmpOrdine(c, x),
      } as NvxListColumn<M.CmpRiga>;
    });
    this.items = this.dati ? (this.dati as any)[this.sezione] ?? [] : null;
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}

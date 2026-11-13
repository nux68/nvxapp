import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { ModalController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';

export type SeletionPeriodoTipo = 'settimana' | 'mese';

/** periodo scelto: date "yyyy-MM-ddT00:00:00" locali, come dataLocale dei turni */
export interface SeletionPeriodoDialogResult {
  tipo: SeletionPeriodoTipo;
  dal: string;
  al: string;
}

interface Opzione {
  valore: string;
  testo: string;
  dal: Date;
  al: Date;
}

/*
 Dialog standard di selezione di un periodo (settimana o mese), come gli altri seletion-*-dialog: tipo di periodo (segmento),
 elenco dei periodi (settimane dalle 4 precedenti alle 8 successive, mesi dai 3 precedenti ai 3 successivi), Conferma/Annulla.
 Usato dal report di "I miei turni". Esito con onWillDismiss: role 'confirm' e data SeletionPeriodoDialogResult; 'cancel' se annullato.
*/
@Component({
  selector: 'app-seletion-periodo-dialog',
  templateUrl: './seletion-periodo-dialog.component.html',
  styleUrls: ['./seletion-periodo-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class SeletionPeriodoDialogComponent {

  /** titolo del dialog (default "Periodo") */
  @Input() title: string = '';
  /** tipo di periodo proposto */
  @Input() tipo: SeletionPeriodoTipo = 'settimana';

  public buttonbar: ButtonItem[] = [];
  public settimane: Opzione[] = [];
  public mesi: Opzione[] = [];
  public scelta: string = '';

  constructor(userInterfaceService: UserInterfaceService,
              private modalCtrl: ModalController,
              private transloco: TranslocoService) {
    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = () => this.conferma();
    this.buttonbar[1].event = () => this.modalCtrl.dismiss(null, 'cancel');
  }

  ionViewWillEnter() {
    if (!this.title)
      this.title = this.transloco.translate('attendance.components.seletionPeriodoDialog.title');
    const lang = this.transloco.getActiveLang();
    const f = (d: Date) => d.toLocaleDateString(lang, { day: '2-digit', month: '2-digit', year: 'numeric' });

    const oggi = new Date();
    const lunedi = new Date(oggi.getFullYear(), oggi.getMonth(), oggi.getDate() - ((oggi.getDay() + 6) % 7));
    this.settimane = [];
    for (let i = -4; i <= 8; i++) {
      const dal = new Date(lunedi.getFullYear(), lunedi.getMonth(), lunedi.getDate() + 7 * i);
      const al = new Date(dal.getFullYear(), dal.getMonth(), dal.getDate() + 6);
      this.settimane.push({ valore: 's' + i, testo: `${f(dal)} - ${f(al)}`, dal, al });
    }
    this.mesi = [];
    for (let i = -3; i <= 3; i++) {
      const dal = new Date(oggi.getFullYear(), oggi.getMonth() + i, 1);
      const al = new Date(dal.getFullYear(), dal.getMonth() + 1, 0);
      const nome = dal.toLocaleDateString(lang, { month: 'long', year: 'numeric' });
      this.mesi.push({ valore: 'm' + i, testo: nome.charAt(0).toUpperCase() + nome.slice(1), dal, al });
    }
    this.scelta = this.tipo === 'mese' ? 'm0' : 's0';
  }

  get opzioni(): Opzione[] {
    return this.tipo === 'mese' ? this.mesi : this.settimane;
  }

  cambiaTipo(v: any) {
    this.tipo = v === 'mese' ? 'mese' : 'settimana';
    this.scelta = this.tipo === 'mese' ? 'm0' : 's0';
  }

  private conferma() {
    const o = this.opzioni.find(x => x.valore === this.scelta);
    if (!o)
      return;
    const p = (n: number) => n.toString().padStart(2, '0');
    const iso = (d: Date) => `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T00:00:00`;
    this.modalCtrl.dismiss({ tipo: this.tipo, dal: iso(o.dal), al: iso(o.al) } as SeletionPeriodoDialogResult, 'confirm');
  }
}

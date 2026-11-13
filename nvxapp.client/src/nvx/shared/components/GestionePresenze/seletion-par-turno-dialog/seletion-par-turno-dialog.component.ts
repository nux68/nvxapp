import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { ModalController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { Par_TurnoModel } from '../../../../ClientServer-Service/GestionePresenze/Turni/Models/turni-model';

/*
 Dialog standard di selezione dei turni (AT-TURNI, ANALISI_ATTENDANCETRACKING_TURNI.md), come gli altri seletion-par-*-dialog:
 caselle di spunta, Conferma/Annulla. Usato dalla pianificazione per scegliere i turni di un dipendente in un giorno
 (uno o piu', compatibili; nessuno = giorno senza turno). La compatibilita' la controlla il server.
 Esito con onWillDismiss/onDidDismiss: role 'confirm' e data SeletionParTurnoDialogResult; role 'cancel' se annullato.
*/
@Component({
  selector: 'app-seletion-par-turno-dialog',
  templateUrl: './seletion-par-turno-dialog.component.html',
  styleUrls: ['./seletion-par-turno-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class SeletionParTurnoDialogComponent {

  /** turni proposti */
  @Input() turni: Par_TurnoModel[] = [];
  /** turni gia' scelti */
  @Input() selezionati: number[] = [];
  /** seconda riga del titolo (es. dipendente e giorno) */
  @Input() sottotitolo: string = '';

  public title: string;
  public buttonbar: ButtonItem[] = [];
  private scelti = new Set<number>();

  constructor(userInterfaceService: UserInterfaceService,
              private modalCtrl: ModalController,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.components.seletionParTurnoDialog.title');
    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = () => this.modalCtrl.dismiss({ id: [...this.scelti] } as SeletionParTurnoDialogResult, 'confirm');
    this.buttonbar[1].event = () => this.modalCtrl.dismiss(null, 'cancel');
  }

  ionViewWillEnter() {
    this.scelti = new Set(this.selezionati ?? []);
  }

  /** turni in ordine di orario (il riposo, senza orario, per primo) */
  get elenco(): Par_TurnoModel[] {
    return [...(this.turni ?? [])].sort((a, b) => (a.orarioTesto ?? '').localeCompare(b.orarioTesto ?? ''));
  }

  isSelezionato(id: number): boolean {
    return this.scelti.has(id);
  }

  toggle(id: number, event: any) {
    if (event?.detail?.checked)
      this.scelti.add(id);
    else
      this.scelti.delete(id);
  }
}

export interface SeletionParTurnoDialogResult {
  id: number[];
}

import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { IonCard, IonCardContent, IonContent, IonHeader, IonItem, IonLabel, IonList, IonNote, IonRadio, IonRadioGroup, IonSearchbar, IonTitle, IonToolbar, ModalController } from '@ionic/angular';
import { SharedComponentInfrastructureModule } from '../../../shared-component-infrastructure.module';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { Par_ProfiloOrarioModel } from '../../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';

/**
 * Scelta di un profilo orario (es. quello da duplicare dalla lista dei profili): elenco con ricerca per
 * codice o descrizione, un solo profilo selezionabile. Conferma: { id } con il ruolo 'confirm'.
 */
@Component({
  selector: 'app-seletion-par-profilo-orario-dialog',
  templateUrl: './seletion-par-profilo-orario-dialog.component.html',
  styleUrls: ['./seletion-par-profilo-orario-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: true,
  imports: [CommonModule, FormsModule, IonCard, IonCardContent, IonContent, IonHeader, IonItem, IonLabel, IonList, IonNote, IonRadio, IonRadioGroup, IonSearchbar, IonTitle, IonToolbar, SharedComponentInfrastructureModule]
})
export class SeletionParProfiloOrarioDialogComponent {

  /** titolo del dialog (di default: scelta del profilo orario) */
  @Input() title: string = '';

  public searchText: string = '';
  public selectedId: number | null = null;
  public buttonbar: ButtonItem[] = [];

  private readonly profili: Par_ProfiloOrarioModel[];

  constructor(userInterfaceService: UserInterfaceService,
              private modalCtrl: ModalController,
              private transloco: TranslocoService,
              sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService) {

    this.profili = [...(sharedParameterGestionePresenzeService.Par_ProfiloOrario ?? [])]
      .sort((a, b) => (a.descrizione ?? '').localeCompare(b.descrizione ?? ''));

    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].disabled = true;
    this.buttonbar[0].event = () => this.modalCtrl.dismiss({ id: this.selectedId }, 'confirm');
    this.buttonbar[1].event = () => this.modalCtrl.dismiss(null, 'cancel');
  }

  ionViewWillEnter() {
    if (!this.title)
      this.title = this.transloco.translate('attendance.components.seletionParProfiloOrarioDialog.title');
  }

  /** profili che contengono il testo cercato nel codice o nella descrizione */
  get profiliFiltrati(): Par_ProfiloOrarioModel[] {
    const testo = this.searchText.trim().toLowerCase();
    return !testo ? this.profili : this.profili.filter(p =>
      (p.codice ?? '').toLowerCase().includes(testo) || (p.descrizione ?? '').toLowerCase().includes(testo));
  }

  onSelect(id: number) {
    this.selectedId = id;
    this.buttonbar[0].disabled = false;
  }
}

export interface SeletionParProfiloOrarioDialogResult {
  id: number;
}

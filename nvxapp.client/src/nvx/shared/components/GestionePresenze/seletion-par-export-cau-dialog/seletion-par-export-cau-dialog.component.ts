import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { IonCard, IonCardContent, IonContent, IonHeader, IonItem, IonLabel, IonList, IonNote, IonRadio, IonRadioGroup, IonSearchbar, IonTitle, IonToolbar, ModalController } from '@ionic/angular';
import { SharedComponentInfrastructureModule } from '../../../shared-component-infrastructure.module';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { Par_ExportCauModel } from '../../../../ClientServer-Service/GestionePresenze/Par_ExportCau/Models/par-export-cau-model';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';

/**
 * Scelta di un modello di export (es. quello da duplicare dalla lista dei modelli): elenco con ricerca per
 * codice o descrizione, un solo modello selezionabile. Conferma: { id } con il ruolo 'confirm'.
 * Come la scelta del profilo orario (seletion-par-profilo-orario-dialog).
 */
@Component({
  selector: 'app-seletion-par-export-cau-dialog',
  templateUrl: './seletion-par-export-cau-dialog.component.html',
  styleUrls: ['./seletion-par-export-cau-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: true,
  imports: [CommonModule, FormsModule, IonCard, IonCardContent, IonContent, IonHeader, IonItem, IonLabel, IonList, IonNote, IonRadio, IonRadioGroup, IonSearchbar, IonTitle, IonToolbar, SharedComponentInfrastructureModule]
})
export class SeletionParExportCauDialogComponent {

  /** titolo del dialog (di default: scelta del modello di export) */
  @Input() title: string = '';

  public searchText: string = '';
  public selectedId: number | null = null;
  public buttonbar: ButtonItem[] = [];

  private readonly modelli: Par_ExportCauModel[];

  constructor(userInterfaceService: UserInterfaceService,
              private modalCtrl: ModalController,
              private transloco: TranslocoService,
              sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService) {

    this.modelli = [...(sharedParameterGestionePresenzeService.Par_ExportCau ?? [])]
      .sort((a, b) => (a.descrizione ?? '').localeCompare(b.descrizione ?? ''));

    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].disabled = true;
    this.buttonbar[0].event = () => this.modalCtrl.dismiss({ id: this.selectedId }, 'confirm');
    this.buttonbar[1].event = () => this.modalCtrl.dismiss(null, 'cancel');
  }

  ionViewWillEnter() {
    if (!this.title)
      this.title = this.transloco.translate('attendance.components.seletionParExportCauDialog.title');
  }

  /** modelli che contengono il testo cercato nel codice o nella descrizione */
  get modelliFiltrati(): Par_ExportCauModel[] {
    const testo = this.searchText.trim().toLowerCase();
    return !testo ? this.modelli : this.modelli.filter(p =>
      (p.codice ?? '').toLowerCase().includes(testo) || (p.descrizione ?? '').toLowerCase().includes(testo));
  }

  onSelect(id: number) {
    this.selectedId = id;
    this.buttonbar[0].disabled = false;
  }
}

export interface SeletionParExportCauDialogResult {
  id: number;
}

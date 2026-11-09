import { Component, Input, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ModalController } from '@ionic/angular';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';

/*
 Dialog standard di selezione dei dipendenti, comune a tutti gli applicativi (presenze, documentale, ...).
 Non legge i dati da nessun applicativo: chi lo apre passa l'elenco gia' pronto (items) con l'id che gli serve
 (es. idAspNetUsers per le presenze, id dell'anagrafica per il documentale).
   - mode 'single': un tocco sceglie e chiude;
   - mode 'multi' : caselle di spunta, "Seleziona tutti"/"Deseleziona tutti", Conferma/Annulla.
 Esito con onWillDismiss/onDidDismiss: role 'confirm' e data DipSelectorResult; role 'cancel' se annullato.
*/

export type DipSelectorMode = 'single' | 'multi';
export type DipSelectorId = string | number;

// voce dell'elenco: "dettaglio" e' la seconda riga (es. nome utente, codice fiscale)
export interface DipSelectorItem {
  id: DipSelectorId;
  cognome: string;
  nome: string;
  dettaglio?: string | null;
  matricola?: string | null;
}

export interface DipSelectorResult<T extends DipSelectorId = DipSelectorId> {
  mode: DipSelectorMode;
  selected: T[];
}

interface DipDisplayItem extends DipSelectorItem {
  displayLabel: string;
  initials: string;
}

@Component({
  selector: 'app-dip-selector-modal',
  templateUrl: './dip-selector-modal.component.html',
  styleUrls: ['./dip-selector-modal.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DipSelectorModalComponent implements OnInit {

  @Input() mode: DipSelectorMode = 'single';
  @Input() items: DipSelectorItem[] = [];
  @Input() preselected: DipSelectorId[] = [];

  public searchText: string = '';
  public selectedIds: Set<DipSelectorId> = new Set();
  public filteredUsers: DipDisplayItem[] = [];
  private allUsers: DipDisplayItem[] = [];
  public buttonbar: ButtonItem[] = [];

  constructor(
    private modalCtrl: ModalController,
    private userInterfaceService: UserInterfaceService
  ) {}

  ngOnInit() {
    this.allUsers = (this.items ?? []).map(u => {
      const cognome = u.cognome ?? '';
      const nome = u.nome ?? '';
      const label = `${cognome} ${nome}`.trim();
      return {
        ...u,
        cognome,
        nome,
        displayLabel: label || String(u.id),
        initials: (cognome.charAt(0) + nome.charAt(0)).toUpperCase() || '?'
      };
    }).sort((a, b) => a.displayLabel.localeCompare(b.displayLabel));

    this.selectedIds = new Set(this.preselected ?? []);
    this.applyFilter();

    if (this.mode === 'multi') {
      this.buttonbar = this.userInterfaceService.Btn_ConfermaAnnulla;
      this.buttonbar[0].disabled = this.confirmDisabled;
      this.buttonbar[0].event = () => this.confirm();
      this.buttonbar[1].event = () => this.cancel();
    } else {
      const btnAnnulla = this.userInterfaceService.Btn_Annulla;
      btnAnnulla.event = () => this.cancel();
      this.buttonbar = [btnAnnulla];
    }
  }

  public onSearchChange(): void {
    this.applyFilter();
  }

  private applyFilter(): void {
    const q = this.searchText.toLowerCase().trim();
    this.filteredUsers = q.length === 0
      ? [...this.allUsers]
      : this.allUsers.filter(u =>
          u.cognome.toLowerCase().includes(q) ||
          u.nome.toLowerCase().includes(q) ||
          (u.dettaglio ?? '').toLowerCase().includes(q) ||
          (u.matricola ?? '').toLowerCase().includes(q)
        );
  }

  public isSelected(id: DipSelectorId): boolean {
    return this.selectedIds.has(id);
  }

  public selectUser(id: DipSelectorId): void {
    if (this.mode === 'single') {
      this.dismiss([id]);
    } else {
      if (this.selectedIds.has(id)) {
        this.selectedIds.delete(id);
      } else {
        this.selectedIds.add(id);
      }
      if (this.buttonbar[0]) this.buttonbar[0].disabled = this.confirmDisabled;
    }
  }

  public confirm(): void {
    this.dismiss(Array.from(this.selectedIds));
  }

  public cancel(): void {
    this.modalCtrl.dismiss(null, 'cancel');
  }

  private dismiss(selected: DipSelectorId[]): void {
    const result: DipSelectorResult = { mode: this.mode, selected };
    this.modalCtrl.dismiss(result, 'confirm');
  }

  public get confirmDisabled(): boolean {
    return this.selectedIds.size === 0;
  }

  public selectAll(): void {
    this.filteredUsers.forEach(u => this.selectedIds.add(u.id));
    if (this.buttonbar[0]) this.buttonbar[0].disabled = this.confirmDisabled;
  }

  public clearAll(): void {
    this.selectedIds.clear();
    if (this.buttonbar[0]) this.buttonbar[0].disabled = this.confirmDisabled;
  }
}

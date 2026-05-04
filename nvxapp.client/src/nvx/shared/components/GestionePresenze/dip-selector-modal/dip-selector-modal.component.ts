import { Component, Input, OnInit } from '@angular/core';
import { ModalController } from '@ionic/angular';
import { Az_SediRepartoUserModel } from '../../../../ClientServer-Service/GestionePresenze/Az_SediRepartoUser/Models/az-reparto-user-model';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { Dip_AnagraficaModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { RoleCode } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';

export type DipSelectorMode = 'single' | 'multi';

export interface DipSelectorResult {
  mode: DipSelectorMode;
  selected: string[];  // array di idAspNetUsers
}

@Component({
  selector: 'app-dip-selector-modal',
  templateUrl: './dip-selector-modal.component.html',
  styleUrls: ['./dip-selector-modal.component.scss'],
  standalone: false
})
export class DipSelectorModalComponent implements OnInit {

  @Input() mode: DipSelectorMode = 'single';
  @Input() userList: Az_SediRepartoUserModel[] = [];
  @Input() preselected: string[] = [];

  public searchText: string = '';
  public selectedIds: Set<string> = new Set();
  public filteredUsers: DipDisplayItem[] = [];
  private allUsers: DipDisplayItem[] = [];

  constructor(
    private modalCtrl: ModalController,
    private sharedParameterService: SharedParameterGestionePresenzeService
  ) {}

  ngOnInit() {
    const anagrafica = this.sharedParameterService.Dip_Anagrafica_OnRoles([RoleCode.User]);

    this.allUsers = this.userList.map(u => {
      const dip = anagrafica.find(a => a.idAspNetUsers === u.idAspNetUsers);
      return {
        idAspNetUsers: u.idAspNetUsers,
        cognome: dip?.cognome ?? '',
        nome: dip?.nome ?? '',
        userName: dip?.userName ?? u.idAspNetUsers,
        displayLabel: dip ? `${dip.cognome} ${dip.nome}` : u.idAspNetUsers,
        initials: dip ? `${dip.cognome.charAt(0)}${dip.nome.charAt(0)}`.toUpperCase() : '?'
      };
    }).sort((a, b) => a.displayLabel.localeCompare(b.displayLabel));

    this.selectedIds = new Set(this.preselected);
    this.applyFilter();
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
          u.userName.toLowerCase().includes(q)
        );
  }

  public isSelected(id: string): boolean {
    return this.selectedIds.has(id);
  }

  public selectUser(id: string): void {
    if (this.mode === 'single') {
      this.dismiss([id]);
    } else {
      if (this.selectedIds.has(id)) {
        this.selectedIds.delete(id);
      } else {
        this.selectedIds.add(id);
      }
    }
  }

  public confirm(): void {
    this.dismiss(Array.from(this.selectedIds));
  }

  public cancel(): void {
    this.modalCtrl.dismiss(null, 'cancel');
  }

  private dismiss(selected: string[]): void {
    const result: DipSelectorResult = { mode: this.mode, selected };
    this.modalCtrl.dismiss(result, 'confirm');
  }

  public get confirmDisabled(): boolean {
    return this.selectedIds.size === 0;
  }

  public selectAll(): void {
    this.filteredUsers.forEach(u => this.selectedIds.add(u.idAspNetUsers));
  }

  public clearAll(): void {
    this.selectedIds.clear();
  }
}

export interface DipDisplayItem {
  idAspNetUsers: string;
  cognome: string;
  nome: string;
  userName: string;
  displayLabel: string;
  initials: string;
}

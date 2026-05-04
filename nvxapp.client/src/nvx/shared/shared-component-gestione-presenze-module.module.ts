import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TipoTimbraturaToLongTextPipe } from './pipe/GestionePresenze/tipo-timbratura-to-long-text.pipe';
import { TipoTimbraturaToShortTextPipe } from './pipe/GestionePresenze/tipo-timbratura-to-short-text.pipe';
import { ParGiustificativiToLongTextPipe } from './pipe/GestionePresenze/par-giustificativi-to-long-text.pipe';
import { ParGiustificativiToShortTextPipe } from './pipe/GestionePresenze/par-giustificativi-to-short-text.pipe';
import { StatoRichiestaLongTextPipe } from './pipe/GestionePresenze/stato-richiesta-long-text.pipe';
import { StatoRichiestaShortTextPipe } from './pipe/GestionePresenze/stato-richiesta-short-text.pipe';
import { TipoRichiestaToLongTextPipe } from './pipe/GestionePresenze/tipo-richiesta-to-long-text.pipe';
import { TipoRichiestaToShortTextPipe } from './pipe/GestionePresenze/tipo-richiesta-to-short-text.pipe';
import { IdDipRapportoLavoroToCognomePipe } from './pipe/GestionePresenze/id-dip-rapporto-lavoro-to-cognome.pipe';
import { IdDipRapportoLavoroToNomePipe } from './pipe/GestionePresenze/id-dip-rapporto-lavoro-to-nome.pipe';
import { IdAspNetUsersToCognomePipe } from './pipe/GestionePresenze/id-asp-net-users-to-cognome.pipe';
import { IdAspNetUsersToNomePipe } from './pipe/GestionePresenze/id-asp-net-users-to-nome.pipe';
import { ParAttivitaToLongTextPipe } from './pipe/GestionePresenze/par-attivita-to-long-text.pipe';
import { ParCompetenzaToLongTextPipe } from './pipe/GestionePresenze/par-competenza-to-long-text.pipe';
import { AzCommessaToLongTextPipe } from './pipe/GestionePresenze/az-commessa-to-long-text.pipe';
import { AzClienteToLongTextPipe } from './pipe/GestionePresenze/az-cliente-to-long-text.pipe';
import { SediRepartoUserNavigationComponent } from './components/GestionePresenze/sedi-reparto-user-navigation/sedi-reparto-user-navigation.component';
import { IonicModule } from '@ionic/angular';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AzSediRepartoToLongTextPipe } from './pipe/GestionePresenze/az-sedireparto-to-long-text.pipe';
import { SeletionSediRepartoDialogComponent } from './components/GestionePresenze/seletion-sedi-reparto-dialog/seletion-sedi-reparto-dialog.component';
import { SeletionSediRepartoUserDialogComponent } from './components/GestionePresenze/seletion-sedi-reparto-user-dialog/seletion-sedi-reparto-user-dialog.component';
import { SharedComponentInfrastructureModule } from './shared-component-infrastructure.module';
import { SeletionParAttivitaDialogComponent } from './components/GestionePresenze/seletion-par-attivita-dialog/seletion-par-attivita-dialog.component';
import { AzSediRepartoToShortTextPipe } from './pipe/GestionePresenze/az-sedireparto-to-short-text.pipe';
import { EditDipProfiloOrarioDialogComponent } from './components/GestionePresenze/edit-dip-profilo-orario-dialog/edit-dip-profilo-orario-dialog.component';
import { ParProfiloOrarioToLongTextPipePipe } from './pipe/GestionePresenze/par-profilo-orario-to-long-text-pipe.pipe';
import { SediRepartoUserSelectionComponent } from './components/GestionePresenze/sedi-reparto-user-selection/sedi-reparto-user-selection.component';
import { ParOrarioToCodicePipe } from './pipe/GestionePresenze/par-orario-to-codice.pipe';
import { ParOrarioToLongTextPipe } from './pipe/GestionePresenze/par-orario-to-long-text.pipe';
import { EditParProfiloOrarioDettaglioOrarioDialogComponent } from './components/GestionePresenze/edit-par-profilo-orario-dettaglio-orario-dialog/edit-par-profilo-orario-dettaglio-orario-dialog.component';
import { EditParOrarioDettaglioOrarioIntervalloHHDialogComponent } from './components/GestionePresenze/edit-par-orario-dettaglio-orario-intervallo-hhdialog/edit-par-orario-dettaglio-orario-intervallo-hhdialog.component';
import { TimeSheetEngineCallerComponent } from './components/GestionePresenze/time-sheet-engine-caller/time-sheet-engine-caller.component';
import { ParCausaliToLongTextPipe } from './pipe/GestionePresenze/par-causali-to-long-text.pipe';
import { ParCausaliToShortTextPipe } from './pipe/GestionePresenze/par-causali-to-short-text.pipe';
import { EditDipGGCausaliDialogComponent } from './components/GestionePresenze/edit-dip-gg-causali-dialog/edit-dip-gg-causali-dialog.component';
import { EditDipGGTimbraturaDialogComponent } from './components/GestionePresenze/edit-dip-gg-timbratura-dialog/edit-dip-gg-timbratura-dialog.component';
import { EditDipGGGiustificativiDialogComponent } from './components/GestionePresenze/edit-dip-gg-giustificativi-dialog/edit-dip-gg-giustificativi-dialog.component';
import { ParExportTipoFileToShortTextPipe } from './pipe/GestionePresenze/par-export-tipo-file-to-short-text.pipe';
import { EditParExportCauCausaliComponentComponent } from './components/GestionePresenze/edit-par-export-cau-causali-component/edit-par-export-cau-causali-component.component';
import { DipSelectorModalComponent } from './components/GestionePresenze/dip-selector-modal/dip-selector-modal.component';

@NgModule({
  declarations: [
    SediRepartoUserNavigationComponent, SediRepartoUserSelectionComponent,
    SeletionSediRepartoDialogComponent,
    SeletionSediRepartoUserDialogComponent,
    SeletionParAttivitaDialogComponent,
    EditDipProfiloOrarioDialogComponent, EditParProfiloOrarioDettaglioOrarioDialogComponent,
    EditParOrarioDettaglioOrarioIntervalloHHDialogComponent, TimeSheetEngineCallerComponent,
    EditDipGGCausaliDialogComponent, EditDipGGTimbraturaDialogComponent, DipSelectorModalComponent,
    EditDipGGGiustificativiDialogComponent, EditParExportCauCausaliComponentComponent,
  ],
  imports: [
    SharedComponentInfrastructureModule,
    CommonModule,
    IonicModule,
    FormsModule,
    ReactiveFormsModule,
    TipoTimbraturaToLongTextPipe, TipoTimbraturaToShortTextPipe, ParGiustificativiToLongTextPipe, ParGiustificativiToShortTextPipe, ParCausaliToShortTextPipe, ParCausaliToLongTextPipe,
    TipoRichiestaToShortTextPipe, TipoRichiestaToLongTextPipe, StatoRichiestaLongTextPipe, StatoRichiestaShortTextPipe,
    IdDipRapportoLavoroToNomePipe, IdDipRapportoLavoroToCognomePipe,
    IdAspNetUsersToCognomePipe, IdAspNetUsersToNomePipe,
    ParAttivitaToLongTextPipe, ParCompetenzaToLongTextPipe, AzCommessaToLongTextPipe, AzClienteToLongTextPipe,
    AzSediRepartoToLongTextPipe, AzSediRepartoToShortTextPipe, ParProfiloOrarioToLongTextPipePipe,
    ParOrarioToCodicePipe, ParOrarioToLongTextPipe, ParExportTipoFileToShortTextPipe
  ],
  exports: [
    TipoTimbraturaToLongTextPipe, TipoTimbraturaToShortTextPipe, ParGiustificativiToLongTextPipe, ParGiustificativiToShortTextPipe, ParCausaliToShortTextPipe, ParCausaliToLongTextPipe,
    TipoRichiestaToShortTextPipe, TipoRichiestaToLongTextPipe, StatoRichiestaLongTextPipe, StatoRichiestaShortTextPipe,
    IdDipRapportoLavoroToNomePipe, IdDipRapportoLavoroToCognomePipe,
    IdAspNetUsersToCognomePipe, IdAspNetUsersToNomePipe,
    ParAttivitaToLongTextPipe, ParCompetenzaToLongTextPipe, AzCommessaToLongTextPipe, AzClienteToLongTextPipe,
    SediRepartoUserNavigationComponent, SediRepartoUserSelectionComponent , SeletionSediRepartoDialogComponent, SeletionSediRepartoUserDialogComponent, SeletionParAttivitaDialogComponent, AzSediRepartoToLongTextPipe,
    EditDipProfiloOrarioDialogComponent, ParProfiloOrarioToLongTextPipePipe, EditParProfiloOrarioDettaglioOrarioDialogComponent,
    EditParOrarioDettaglioOrarioIntervalloHHDialogComponent, TimeSheetEngineCallerComponent, EditParExportCauCausaliComponentComponent,
    EditDipGGCausaliDialogComponent, EditDipGGTimbraturaDialogComponent, DipSelectorModalComponent,
    EditDipGGGiustificativiDialogComponent,
    ParOrarioToCodicePipe, ParOrarioToLongTextPipe, ParExportTipoFileToShortTextPipe
  ]
})
export class SharedComponentGestionePresenzeModuleModule { }

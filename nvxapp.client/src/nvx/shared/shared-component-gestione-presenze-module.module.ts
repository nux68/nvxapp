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
import { FormsModule } from '@angular/forms';
import { AzSediRepartoToLongTextPipe } from './pipe/GestionePresenze/az-sedireparto-to-long-text.pipe';
import { SeletionSediRepartoDialogComponent } from './components/GestionePresenze/seletion-sedi-reparto-dialog/seletion-sedi-reparto-dialog.component';
import { SeletionSediRepartoUserDialogComponent } from './components/GestionePresenze/seletion-sedi-reparto-user-dialog/seletion-sedi-reparto-user-dialog.component';
import { SharedComponentInfrastructureModule } from './shared-component-infrastructure.module';
import { SeletionParAttivitaDialogComponent } from './components/GestionePresenze/seletion-par-attivita-dialog/seletion-par-attivita-dialog.component';
import { AzSediRepartoToShortTextPipe } from './pipe/GestionePresenze/az-sedireparto-to-short-text.pipe';
import { EditDipProfiloOrarioDialogComponent } from './components/GestionePresenze/edit-dip-profilo-orario-dialog/edit-dip-profilo-orario-dialog.component';
import { ParProfiloOrarioToLongTextPipePipe } from './pipe/GestionePresenze/par-profilo-orario-to-long-text-pipe.pipe';

@NgModule({
  declarations: [
    SediRepartoUserNavigationComponent,
    SeletionSediRepartoDialogComponent,
    SeletionSediRepartoUserDialogComponent,
    SeletionParAttivitaDialogComponent,
    EditDipProfiloOrarioDialogComponent
  ],
  imports: [
    SharedComponentInfrastructureModule,
    CommonModule,
    IonicModule,
    FormsModule,
    TipoTimbraturaToLongTextPipe, TipoTimbraturaToShortTextPipe, ParGiustificativiToLongTextPipe, ParGiustificativiToShortTextPipe,
    TipoRichiestaToShortTextPipe, TipoRichiestaToLongTextPipe, StatoRichiestaLongTextPipe, StatoRichiestaShortTextPipe,
    IdDipRapportoLavoroToNomePipe, IdDipRapportoLavoroToCognomePipe,
    IdAspNetUsersToCognomePipe, IdAspNetUsersToNomePipe,
    ParAttivitaToLongTextPipe, ParCompetenzaToLongTextPipe, AzCommessaToLongTextPipe, AzClienteToLongTextPipe,
    AzSediRepartoToLongTextPipe, AzSediRepartoToShortTextPipe, ParProfiloOrarioToLongTextPipePipe
  ],
  exports: [
    TipoTimbraturaToLongTextPipe, TipoTimbraturaToShortTextPipe, ParGiustificativiToLongTextPipe, ParGiustificativiToShortTextPipe,
    TipoRichiestaToShortTextPipe, TipoRichiestaToLongTextPipe, StatoRichiestaLongTextPipe, StatoRichiestaShortTextPipe,
    IdDipRapportoLavoroToNomePipe, IdDipRapportoLavoroToCognomePipe,
    IdAspNetUsersToCognomePipe, IdAspNetUsersToNomePipe,
    ParAttivitaToLongTextPipe, ParCompetenzaToLongTextPipe, AzCommessaToLongTextPipe, AzClienteToLongTextPipe,
    SediRepartoUserNavigationComponent, SeletionSediRepartoDialogComponent, SeletionSediRepartoUserDialogComponent, SeletionParAttivitaDialogComponent, AzSediRepartoToLongTextPipe,
    EditDipProfiloOrarioDialogComponent, ParProfiloOrarioToLongTextPipePipe
  ]
})
export class SharedComponentGestionePresenzeModuleModule { }

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



@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    TipoTimbraturaToLongTextPipe, TipoTimbraturaToShortTextPipe, ParGiustificativiToLongTextPipe, ParGiustificativiToShortTextPipe,
    TipoRichiestaToShortTextPipe, TipoRichiestaToLongTextPipe, StatoRichiestaLongTextPipe, StatoRichiestaShortTextPipe
  ],
  exports: [TipoTimbraturaToLongTextPipe, TipoTimbraturaToShortTextPipe, ParGiustificativiToLongTextPipe, ParGiustificativiToShortTextPipe,
            TipoRichiestaToShortTextPipe, TipoRichiestaToLongTextPipe, StatoRichiestaLongTextPipe, StatoRichiestaShortTextPipe]
})
export class SharedComponentGestionePresenzeModuleModule { }

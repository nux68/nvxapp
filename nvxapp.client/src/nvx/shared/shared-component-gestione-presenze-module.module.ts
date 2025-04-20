import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TipoTimbraturaToLongTextPipe } from './pipe/GestionePresenze/tipo-timbratura-to-long-text.pipe';
import { TipoTimbraturaToShortTextPipe } from './pipe/GestionePresenze/tipo-timbratura-to-short-text.pipe';



@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    TipoTimbraturaToLongTextPipe, TipoTimbraturaToShortTextPipe
  ],
  exports: [TipoTimbraturaToLongTextPipe, TipoTimbraturaToShortTextPipe]
})
export class SharedComponentGestionePresenzeModuleModule { }

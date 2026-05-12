import { Pipe, PipeTransform } from '@angular/core';
import { Contatori_Giustificativo_Result } from '../../../ClientServer-Service/GestionePresenze/Contatori/Models/contatori-model';

@Pipe({
  name: 'justContatore',
  standalone: true
})
export class JustContatorePipe implements PipeTransform {
  transform(risultati: Contatori_Giustificativo_Result[], idPar_Giustificativi: number): Contatori_Giustificativo_Result | null {
    return risultati?.find(r => r.idPar_Giustificativi === idPar_Giustificativi) ?? null;
  }
}

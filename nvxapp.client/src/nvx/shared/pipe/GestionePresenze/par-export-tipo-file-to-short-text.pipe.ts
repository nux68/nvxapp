import { Pipe, PipeTransform } from '@angular/core';
import { Par_Export_TipoFile } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau/Models/par-export-cau-model';

@Pipe({
  name: 'parExportTipoFileToShortText',
  standalone: true
})
export class ParExportTipoFileToShortTextPipe implements PipeTransform {

  transform(value: Par_Export_TipoFile | null | undefined): string {
    if (value === null || value === undefined) {
      return '';
    }

    switch (value) {
      case Par_Export_TipoFile.CSV:
        return 'CSV';
      case Par_Export_TipoFile.TXT:
        return 'TXT';
      default:
        return '';
    }
  }

}

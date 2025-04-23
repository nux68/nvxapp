import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'parGiustificativiToShortText',
  standalone: true
})
export class ParGiustificativiToShortTextPipe implements PipeTransform {

  transform(value: unknown, ...args: unknown[]): unknown {
    return null;
  }

}

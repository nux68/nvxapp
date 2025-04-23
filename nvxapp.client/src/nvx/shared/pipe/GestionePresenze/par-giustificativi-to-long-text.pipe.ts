import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'parGiustificativiToLongText',
  standalone: true
})
export class ParGiustificativiToLongTextPipe implements PipeTransform {

  transform(value: unknown, ...args: unknown[]): unknown {
    return null;
  }

}

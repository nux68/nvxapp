import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'genericFilter',
  standalone: true
})
export class GenericFilterPipe implements PipeTransform {
  transform(items: any[], field: string, searchText: string): any[] {
    if (!items || !field || !searchText) {
      return items;
    }
    return items.filter(item =>
      item[field]?.toString().toLowerCase().includes(searchText.toLowerCase())
    );
  }
}




export class GenericRequest<T> implements iGenericRequest<T> {
  data: T;

  
  constructor(type: new () => T) {
    this.data = new type(); // Crea una nuova istanza di T
  }

}


export interface iGenericRequest<T> {
  data: T;
}

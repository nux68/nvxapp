


export class GenericRequest<T> implements iGenericRequest<T> {
  data: T;

  constructor() {
    
  }

}


export interface iGenericRequest<T> {
  data: T;
}

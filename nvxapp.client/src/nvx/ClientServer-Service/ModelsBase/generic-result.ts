import { Message } from "./message";




export class GenericResult<T> implements iGenericResult<T> {

  success: boolean;
  data: T | null;
  messages: Message[];

  constructor(type: new () => T) {
    this.data = new type(); // Crea una nuova istanza di T
  }

}

export interface iGenericResult<T> {
  
  success: boolean;
  messages: Message[];
  data: T | null;
}




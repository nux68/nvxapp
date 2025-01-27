import { Message } from "./message";




export class GenericResult<T> implements iGenericResult<T> {

  success: boolean;
  data: T | null;
  messages: Message[];

  constructor() {

  }

}

export interface iGenericResult<T> {
  
  success: boolean;
  messages: Message[];
  data: T | null;
}




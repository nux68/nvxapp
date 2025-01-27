import { Message } from "./message";




export class ModelResult implements iModelResult {
  
  success: boolean;
  messages: Message[];

  constructor() {
    
  }

}


export interface iModelResult {

  success: boolean;
  messages: Message[];
  
}

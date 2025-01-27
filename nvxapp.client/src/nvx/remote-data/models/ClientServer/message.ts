


export class Message   {
  text: string;
  msgType: MessageType;

  constructor() {
    
  }

}




export enum MessageType {
  Exception = 0,
  Error = 1,
  Warning = 2,
  Information = 3,
}

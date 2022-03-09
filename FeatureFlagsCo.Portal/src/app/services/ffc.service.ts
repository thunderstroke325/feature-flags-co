import { Injectable } from '@angular/core';
import Ffc from 'ffc-js-client-side-sdk';
import { IOption } from "ffc-js-client-side-sdk/esm/types";
import { IUser } from 'ffc-js-client-side-sdk/esm/types';

@Injectable({
  providedIn: 'root'
})
export class FfcService {

  async initialize(option: IOption) {
    await Ffc.init(option);
  }

  async identify(user: IUser) {
    await Ffc.identify(user);
  }

  getUser(): IUser {
    return Ffc.getUser();
  }

  async logout(): Promise<IUser> {
    return await Ffc.logout();
  }

  variation(key: string, defaultResult: string): string {
    return Ffc.variation(key, defaultResult);
  }
}

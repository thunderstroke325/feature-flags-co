import { Injectable } from '@angular/core';
import Ffc from 'ffc-js-client-side-sdk';
import { IOption } from "ffc-js-client-side-sdk/esm/types";

@Injectable({
  providedIn: 'root'
})
export class FfcService {

  async initialize(option: IOption) {
    Ffc.init(option);

    await Ffc.waitUntilReady();
  }

  variation(key: string, defaultResult: string): string {
    return Ffc.variation(key, defaultResult);
  }
}

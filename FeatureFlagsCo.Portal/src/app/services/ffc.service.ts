import { Injectable } from '@angular/core';
import Ffc from 'ffc-js-client-side-sdk';
import { IOption } from "ffc-js-client-side-sdk/esm/types";

@Injectable({
  providedIn: 'root'
})
export class FfcService {

  initialize(option: IOption): void {
    Ffc.init(option);
  }

  variation(key: string, defaultResult: string): string {
    return Ffc.variation(key, defaultResult);
  }
}

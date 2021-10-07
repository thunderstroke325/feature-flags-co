import { Injectable, Inject } from '@angular/core';
import { FFCJsClient, IOption } from 'ffc-js-client-sdk/esm';

@Injectable({
  providedIn: 'root'
})
export class FfcService {

  client = FFCJsClient;
  constructor() {
  }
}
